using System.Data;
using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AngleSharp.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.Input;
using OPS.Core;
using OPS.Core.DTOs;
using OPS.Core.Enumerators;
using OPS.Core.Exceptions;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Utilities;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.Fornecedores;
using RestSharp;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace OPS.Importador.Comum.Despesa;

public abstract class ImportadorDespesasBase
{
    public ImportadorCotaParlamentarBaseConfig config { get; set; }

    protected readonly AppSettings appSettings;
    protected readonly FileManager fileManager;

    public ILogger<ImportadorDespesasBase> logger { get; set; }

    public IDbConnection connection { get { return dbContext.Database.GetDbConnection(); } }

    public AppDbContext dbContext { get; set; }

    public string tempFolder
    {
        get
        {
            var dir = Path.Combine(appSettings.TempFolder, "Estados", config.Estado.ToString());

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            return dir;
        }
    }

    public string competenciaInicial { get; set; }

    public string competenciaFinal { get; set; }

    public int idEstado { get { return config.Estado.GetHashCode(); } }

    private int itensProcessadosAno { get; set; }

    private decimal valorTotalProcessadoAno { get; set; }

    protected Dictionary<string, long> lstHash { get; private set; }

    private HttpClient _httpClientResilient;
    /// <summary>
    /// Client with Retry and NO AllowAutoRedirect
    /// </summary>
    public HttpClient httpClientResilient { get { return _httpClientResilient ??= httpClientFactory.CreateClient("ResilientClient"); } }

    private HttpClient _httpClientDefault;

    /// <summary>
    /// Client with AllowAutoRedirect
    /// </summary>
    public HttpClient httpClientDefault { get { return _httpClientDefault ??= httpClientFactory.CreateClient("DefaultClient"); } }

    private IHttpClientFactory httpClientFactory { get; }

    public List<CamaraEstadualDespesaTemp> despesasTemp { get; set; }
    public List<CamaraEstadualDespesaTemp> despesasHistorico { get; set; }

    private JsonSerializerOptions jsonSerializeOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    private JsonSerializerOptions jsonSerializeOptionsIgnoreDecimalValues = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { IgnoreNumericValues },
        }
    };

    public ImportadorDespesasBase(IServiceProvider serviceProvider)
    {
        logger = serviceProvider.GetRequiredService<ILogger<ImportadorDespesasBase>>();
        dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        fileManager = serviceProvider.GetRequiredService<FileManager>();
        httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

        despesasTemp = new List<CamaraEstadualDespesaTemp>();
    }

    public virtual void DownloadFotosParlamentares()
    {
        logger.LogWarning("Sem Importação de Imagens");
    }

    private static object _monitorObj = new object();

    public virtual void ProcessarDespesas(int ano)
    {
        Monitor.Enter(_monitorObj);

        try
        {
            logger.LogInformation("Iniciando processamento na base de dados!");
            LimpaDespesaTemporaria();

            InsereDespesasTemp(ano);
            AjustarDadosGlobais();
            AjustarDados();
            InsereTipoDespesaFaltante();
            InsereDeputadoFaltante();
            InsereFornecedorFaltante();
            DeletarDespesasInexistentes(ano);

            InsereDespesaFinal(ano);
            ValidaImportacao(ano);

            if (ano == DateTime.Now.Year)
            {
                try
                {
                    AtualizaValorTotal();
                }
                catch (NpgsqlException ex) when (ex.SqlState == "40P01")
                {
                    AtualizaValorTotal();
                }

                AtualizaCampeoesGastos();
            }

            logger.LogInformation("Finalizando processamento na base de dados!");
        }
        finally
        {
            Monitor.Exit(_monitorObj);
        }
    }

    protected bool CarregarDespesaArquivoHistorico(int ano)
    {
        var filename = Path.Combine(tempFolder, $"despesas-{ano}.csv");
        if (!File.Exists(filename)) return false;

        var csvCulture = CultureInfo.CreateSpecificCulture("en-US");
        var csvConfig = new CsvConfiguration(csvCulture);

        using (var reader = new StreamReader(filename, System.Text.Encoding.UTF8))
        using (var csv = new CsvReader(reader, csvConfig))
        {
            csv.Context.RegisterClassMap<CamaraEstadualDespesaTempClassMap>();
            despesasHistorico = csv.GetRecords<CamaraEstadualDespesaTemp>().ToList();
        }

        foreach (var item in despesasHistorico)
        {
            // Force to set Ano and Mes ignored on deserialization
            item.Ano = item.DataVigencia.Year;
            item.Mes = item.DataVigencia.Month;

            if (item.DataEmissao == null)
                item.DataEmissao = new DateOnly(item.Ano, item.Mes.Value, 1);
            else if (item.Mes == null)
                item.Mes = item.DataEmissao.Value.Month;
        }

        return true;
    }

    /// <summary>
    /// Padroniza preenchimento dos campos para correta verificação de alterações independente da fonte (official ou arquivo).
    /// </summary>
    /// <param name="item"></param>
    private void PadronizarValoresDespesasTemp(CamaraEstadualDespesaTemp item)
    {
        // Set default null if empty
        item.Cpf = item.Cpf.NullIfEmpty();
        item.Nome = item.Nome.NullIfEmpty();
        item.NomeCivil = item.NomeCivil.NullIfEmpty();
        item.CnpjCpf = item.CnpjCpf.NullIfEmpty();
        item.NomeFornecedor = item.NomeFornecedor.NullIfEmpty();
        item.TipoVerba = item.TipoVerba.NullIfEmpty();
        item.TipoDespesa = item.TipoDespesa.NullIfEmpty();
        item.Documento = item.Documento.NullIfEmpty();
        item.Favorecido = item.Favorecido.NullIfEmpty();
        item.Observacao = item.Observacao.NullIfEmpty();

        if (item.DataEmissao == null)
            item.DataEmissao = new DateOnly(item.Ano, item.Mes.Value, 1);
        else if (item.Mes == null)
            item.Mes = item.DataEmissao.Value.Month;
    }

    private void InsereDespesasTemp(int ano)
    {
        foreach (var item in despesasTemp)
        {
            PadronizarValoresDespesasTemp(item);
        }

        var csvCulture = CultureInfo.CreateSpecificCulture("en-US");
        var csvConfig = new CsvConfiguration(csvCulture)
        {
            ShouldQuote = args => args.FieldType == typeof(string) && !string.IsNullOrEmpty(args.Field) && (args.Row.Row > 1 /* Ignore Header */)
        };

        using (var writer = new StreamWriter(Path.Combine(tempFolder, $"despesas-{ano}.csv"), false, System.Text.Encoding.UTF8))
        using (var csv = new CsvWriter(writer, csvConfig))
        {
            csv.Context.RegisterClassMap<CamaraEstadualDespesaTempClassMap>();
            csv.WriteRecords(despesasTemp);
        }

        List<IGrouping<string, CamaraEstadualDespesaTemp>> results = despesasTemp.GroupBy(x => Convert.ToHexString(x.Hash)).ToList();
        itensProcessadosAno = results.Count();
        valorTotalProcessadoAno = results.Sum(x => x.Sum(x => x.Valor));

        logger.LogInformation("Processando {Itens} despesas agrupadas em {Unicos} unicas com valor total de {ValorTotal:#,###.00} para {Estado} em {Ano}.", despesasTemp.Count(), itensProcessadosAno, valorTotalProcessadoAno, config.Estado.ToString(), ano);

        var despesaInserida = 0;
        foreach (var despesaGroup in results)
        {
            //if (despesaGroup.Count() > 1)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine($"{despesaGroup.Count()} Itens Agrupados!");
            //    foreach (var item in despesaGroup)
            //    {
            //        Console.WriteLine(JsonSerializer.Serialize(item, jsonSerializeOptions));
            //    }
            //}

            CamaraEstadualDespesaTemp despesa = despesaGroup.FirstOrDefault();
            despesa.Valor = despesaGroup.Sum(x => x.Valor);

            var str = JsonSerializer.Serialize(despesa, jsonSerializeOptions);
            var hash = Utils.SHA1Hash(str);
            var key = Convert.ToHexString(hash);

            if (lstHash.Remove(key)) continue;

            despesa.Hash = hash;
            dbContext.CamaraEstadualDespesaTemps.Add(despesa);
            despesaInserida++;
        }

        dbContext.SaveChanges();
        if (despesaInserida > 0)
            logger.LogInformation("{Itens} despesas inseridas na tabela temporaria para {Estado} em {Ano}.", despesaInserida, config.Estado.ToString(), ano);

        despesasTemp = new List<CamaraEstadualDespesaTemp>();
    }

    public virtual void ImportarRemuneracao(int ano, int mes)
    {
        logger.LogWarning("Sem Importação de Remuneração");
    }

    public virtual void AtualizaParlamentarValores() { }

    //public virtual void AtualizaCampeoesGastos() { }

    //public virtual void AtualizaResumoMensal() { }

    public virtual Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
    {
        throw new NotImplementedException();
    }

    //protected bool BaixarArquivo(string urlOrigem, string caminhoArquivo)
    //{
    //    var caminhoArquivoDb = caminhoArquivo.Replace(appSettings.TempFolder, "");
    //    Monitor.Enter(_monitorObj);
    //    arquivoChecksum = dbContext.ArquivoChecksums.FirstOrDefault(x => x.Nome == caminhoArquivoDb);

    //    if (appSettings.ImportacaoIncremental && File.Exists(caminhoArquivo))
    //    {
    //        var arquivoDB = dbContext.ArquivoChecksums.FirstOrDefault(x => x.Nome == caminhoArquivoDb);
    //        if (arquivoDB != null && arquivoDB.Verificacao > DateTime.UtcNow.AddDays(-7))
    //        {
    //            logger.LogWarning("Ignorando arquivo verificado recentemente '{CaminhoArquivo}' a partir de '{UrlOrigem}'", caminhoArquivo, urlOrigem);
    //            return false;
    //        }
    //        else if (arquivoDB.Criacao < DateTime.UtcNow.AddMonths(-5))
    //        {
    //            logger.LogWarning("Ignorando arquivo antigo '{CaminhoArquivo}' a partir de '{UrlOrigem}'", caminhoArquivo, urlOrigem);
    //            return false;
    //        }
    //    }
    //    Monitor.Exit(_monitorObj);

    //    logger.LogInformation("Baixando arquivo '{CaminhoArquivo}' a partir de '{UrlOrigem}'", caminhoArquivo, urlOrigem);

    //    string diretorio = new FileInfo(caminhoArquivo).Directory.ToString();
    //    if (!Directory.Exists(diretorio))
    //        Directory.CreateDirectory(diretorio);

    //    var fileExt = Path.GetExtension(caminhoArquivo);
    //    var caminhoArquivoTmp = caminhoArquivo.Replace(fileExt, $"_tmp{fileExt}");
    //    if (File.Exists(caminhoArquivoTmp))
    //        File.Delete(caminhoArquivoTmp);

    //    if (config.Estado != Estados.DistritoFederal)
    //        httpClientResilient.DownloadFile(urlOrigem, caminhoArquivoTmp).Wait();
    //    else
    //        httpClientDefault.DownloadFile(urlOrigem, caminhoArquivoTmp).Wait();

    //    string checksum = ChecksumCalculator.ComputeFileChecksum(caminhoArquivoTmp);

    //    Monitor.Enter(_monitorObj);
    //    try
    //    {
    //        if (arquivoChecksum != null && arquivoChecksum.Checksum == checksum && File.Exists(caminhoArquivo))
    //        {
    //            arquivoChecksum.Verificacao = DateTime.UtcNow;
    //            dbContext.SaveChanges();

    //            logger.LogInformation("Arquivo '{CaminhoArquivo}' é identico ao já existente.", caminhoArquivo);

    //            if (File.Exists(caminhoArquivoTmp))
    //                File.Delete(caminhoArquivoTmp);

    //            return false;
    //        }

    //        if (arquivoChecksum == null)
    //        {
    //            logger.LogInformation("Arquivo '{CaminhoArquivo}' é novo.", caminhoArquivo);

    //            arquivoChecksum = new ArquivoChecksum()
    //            {
    //                Nome = caminhoArquivoDb,
    //                Checksum = checksum,
    //                TamanhoBytes = (int)new FileInfo(caminhoArquivoTmp).Length,
    //                Criacao = DateTime.UtcNow,
    //                Atualizacao = DateTime.UtcNow,
    //                Verificacao = DateTime.UtcNow
    //            };
    //            dbContext.ArquivoChecksums.Add(arquivoChecksum);
    //        }
    //        else
    //        {
    //            if (arquivoChecksum.Checksum != checksum)
    //            {
    //                logger.LogInformation("Arquivo '{CaminhoArquivo}' foi atualizado.", caminhoArquivo);

    //                arquivoChecksum.Checksum = checksum;
    //                arquivoChecksum.TamanhoBytes = (int)new FileInfo(caminhoArquivoTmp).Length;
    //                arquivoChecksum.Revisado = false;
    //            }

    //            arquivoChecksum.Atualizacao = DateTime.UtcNow;
    //            arquivoChecksum.Verificacao = DateTime.UtcNow;
    //        }

    //        dbContext.SaveChanges();
    //    }
    //    finally
    //    {

    //        Monitor.Exit(_monitorObj);
    //    }

    //    File.Move(caminhoArquivoTmp, caminhoArquivo, true);
    //    return true;
    //}

    public virtual void AtualizaValorTotal()
    {
        logger.LogDebug("Atualizando valores totais");

        connection.Execute(@$"
UPDATE assembleias.cl_deputado dp 
SET valor_total_ceap = coalesce((
        SELECT SUM(ds.valor_liquido) 
        FROM assembleias.cl_despesa ds 
        WHERE ds.id_cl_deputado = dp.id
    ), 0)
WHERE id_estado = {idEstado};");
    }

    private void AjustarDadosGlobais()
    {
        // CNPJ Especiais da Câmara Federal
        // 00000000000001 - CELULAR FUNCIONAL
        // 00000000000002 - LINHA DIRETA
        // 00000000000003 - IMÓVEL FUNCIONAL
        // 00000000000006 - RAMAL
        // 00000000000007 - CORREIOS - SEDEX CONVENCIONAL
        // 00000000000008 - PEDÁGIO
        // 00000000000009 - TAXI
        connection.Execute(@"
UPDATE temp.cl_despesa_temp SET cnpj_cpf = '00000000000001' WHERE cnpj_cpf is null and unaccent(fornecedor) ilike 'telefonia';
UPDATE temp.cl_despesa_temp SET cnpj_cpf = '00000000000007' WHERE cnpj_cpf is null and unaccent(fornecedor) ilike 'correios%';
UPDATE temp.cl_despesa_temp SET cnpj_cpf = '00000000000008' WHERE cnpj_cpf is null and unaccent(fornecedor) ilike 'pedagio';
UPDATE temp.cl_despesa_temp SET cnpj_cpf = '00000000000009' WHERE cnpj_cpf is null and unaccent(fornecedor) ilike 'taxi';
        ");

        // Substituir nome importação pelo utilizado na importação. Dessa forma podemos ter 2 nomes validos e padronizamos a importação de validação.
        if (config.ChaveImportacao == ChaveDespesaTemp.NomeCivil)
        {
            connection.Execute($@"
UPDATE temp.cl_despesa_temp dt
SET nome_civil = d.nome_civil
FROM assembleias.cl_deputado d 
WHERE d.nome_importacao ILIKE dt.nome_civil
AND d.id_estado = {idEstado}
AND dt.nome_civil IS NOT NULL
");
        }
        else if (config.ChaveImportacao == ChaveDespesaTemp.NomeParlamentar)
        {
            connection.Execute($@"
UPDATE temp.cl_despesa_temp dt
SET nome = d.nome_parlamentar
FROM assembleias.cl_deputado d 
WHERE d.nome_importacao ILIKE dt.nome
AND d.id_estado = {idEstado}
AND d.nome_parlamentar IS NOT NULL
");

            if (idEstado != Estados.Paraiba.GetHashCode())
            {
                connection.Execute($@"
UPDATE temp.cl_despesa_temp dt
SET cnpj_cpf = dp.cnpj_correto
FROM temp.fornecedor_de_para dp
WHERE dp.cnpj_incorreto = dt.cnpj_cpf
AND dp.cnpj_correto IS NOT NULL
");
            }
        }
    }

    private void ResolverFornecedor()
    {
        ResolverFornecedorPreExistente();
        ResolverFornecedorDepara();
    }

    private void ResolverFornecedorPreExistente()
    {
        connection.Execute(@$"
-- Por CNPJ para fornecedor pré-existente
UPDATE temp.cl_despesa_temp dt
set id_fornecedor = f.id
FROM fornecedor.fornecedor f 
WHERE dt.id_fornecedor IS NULL
AND f.cnpj_cpf = dt.cnpj_cpf;
");
    }

    private void ResolverFornecedorDepara()
    {
        connection.Execute(@$"
-- Por Nome exato e CPNJ
UPDATE temp.cl_despesa_temp dt
set id_fornecedor = COALESCE(f.id_fornecedor_correto, f.id_fornecedor_incorreto)
FROM temp.fornecedor_de_para f 
WHERE dt.id_fornecedor IS NULL
AND f.cnpj_incorreto = dt.cnpj_cpf
AND unaccent(lower(f.nome)) = unaccent(lower(dt.fornecedor));

-- Por CNPJ (Pode ser parcial ou invalido)
UPDATE temp.cl_despesa_temp dt
set id_fornecedor = COALESCE(f.id_fornecedor_correto, f.id_fornecedor_incorreto)
FROM temp.fornecedor_de_para f 
WHERE dt.id_fornecedor IS NULL
AND f.cnpj_incorreto = dt.cnpj_cpf;

-- Por Nome exato (accent-insensitive matching)
UPDATE temp.cl_despesa_temp dt
set id_fornecedor = COALESCE(f.id_fornecedor_correto, f.id_fornecedor_incorreto), cnpj_cpf = COALESCE(dt.cnpj_cpf, f.cnpj_correto)
FROM temp.fornecedor_de_para f 
WHERE dt.id_fornecedor IS NULL
AND unaccent(lower(f.nome)) = unaccent(lower(dt.fornecedor));
");
    }

    public virtual void AjustarDados()
    { }

    public virtual void InsereTipoDespesaFaltante()
    {
        logger.LogDebug("Inserir despesa faltante");

        var affected = connection.Execute(@"
INSERT INTO assembleias.cl_despesa_especificacao (descricao)
select distinct despesa_tipo
from temp.cl_despesa_temp
where despesa_tipo is not null
and lower(unaccent(despesa_tipo)) not in (
    select lower(unaccent(descricao)) FROM assembleias.cl_despesa_especificacao
)
ORDER BY despesa_tipo;
                ");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} tipos de despesas incluidos!", affected);
        }
    }

    public virtual void InsereDeputadoFaltante()
    {
        logger.LogDebug("Inserir parlamentar faltante");

        var chaveImportacao = "nome_parlamentar";
        int affected = 0;
        if (config.ChaveImportacao == ChaveDespesaTemp.Cpf)
        {
            chaveImportacao = "cpf";
            affected = connection.Execute(@$"
INSERT INTO assembleias.cl_deputado (nome_parlamentar, nome_civil, cpf, id_estado)
select distinct nome, coalesce(nome_civil, nome), cpf, {idEstado}
from temp.cl_despesa_temp
where cpf not in (
    select cpf 
    FROM assembleias.cl_deputado 
    WHERE id_estado = {idEstado} 
    AND cpf IS NOT NULL
);");
        }
        else if (config.ChaveImportacao == ChaveDespesaTemp.CpfParcial)
        {
            chaveImportacao = "cpf_parcial";
            affected = connection.Execute(@$"
INSERT INTO assembleias.cl_deputado (nome_parlamentar, nome_civil, cpf_parcial, id_estado)
select distinct nome, coalesce(nome_civil, nome), cpf, {idEstado}
from temp.cl_despesa_temp
where cpf not in (
    select cpf_parcial 
    FROM assembleias.cl_deputado 
    WHERE id_estado = {idEstado} 
    AND cpf_parcial IS NOT NULL
);");
        }
        else if (config.ChaveImportacao == ChaveDespesaTemp.Matricula)
        {
            chaveImportacao = "matricula";
            affected = connection.Execute(@$"
INSERT INTO assembleias.cl_deputado (nome_parlamentar, nome_civil, matricula, id_estado)
select distinct nome, coalesce(nome_civil, nome), cpf::int, {idEstado}
from temp.cl_despesa_temp
where cpf not in (
    select matricula::text 
    FROM assembleias.cl_deputado 
    WHERE id_estado = {idEstado} 
    AND matricula IS NOT NULL
);");
        }
        else if (config.ChaveImportacao == ChaveDespesaTemp.Gabinete)
        {
            chaveImportacao = "gabinete";
            affected = connection.Execute(@$"
INSERT INTO assembleias.cl_deputado (nome_parlamentar, nome_civil, gabinete, id_estado)
select distinct nome, coalesce(nome_civil, nome), cast(cpf as int), {idEstado}
from temp.cl_despesa_temp
where cpf not in (
    select gabinete::text
    FROM assembleias.cl_deputado 
    WHERE id_estado = {idEstado} 
    AND gabinete IS NOT NULL
);");
        }
        else if (config.ChaveImportacao != ChaveDespesaTemp.IdDeputado)
        {
            connection.Execute(@$"
UPDATE temp.cl_despesa_temp t
SET id_cl_deputado = d.id
FROM temp.cl_deputado_de_para d 
WHERE unaccent(coalesce(t.nome, t.nome_civil)) ILIKE unaccent(d.nome)
AND d.id_estado = {idEstado}");

            if (config.ChaveImportacao == ChaveDespesaTemp.NomeCivil)
            {
                chaveImportacao = "nome_civil";
                affected = connection.Execute(@$"
INSERT INTO assembleias.cl_deputado (nome_parlamentar, nome_civil, cpf, id_estado)
select distinct nome, coalesce(nome_civil, nome), cpf, {idEstado}
from temp.cl_despesa_temp
where id_cl_deputado is null
and lower(unaccent(nome_civil)) not in (
    select lower(unaccent(nome_civil))
    FROM assembleias.cl_deputado 
    WHERE id_estado = {idEstado} 
    AND nome_civil IS NOT null
);");
            }
            else if (config.ChaveImportacao == ChaveDespesaTemp.NomeParlamentar)
            {

                chaveImportacao = "nome_parlamentar";
                affected = connection.Execute(@$"
INSERT INTO assembleias.cl_deputado (nome_parlamentar, nome_civil, cpf, id_estado)
select distinct nome, coalesce(nome_civil, nome), cpf, {idEstado}
from temp.cl_despesa_temp
where id_cl_deputado is null
and lower(unaccent(nome)) not in (
    select lower(unaccent(nome_parlamentar))
    FROM assembleias.cl_deputado 
    WHERE id_estado = {idEstado} 
    AND nome_parlamentar IS NOT null
);");
            }

            if (affected > 0)
            {
                connection.Execute(@$"
INSERT INTO temp.cl_deputado_de_para (id, nome, id_estado)
SELECT id, {chaveImportacao}, id_estado FROM assembleias.cl_deputado
WHERE {chaveImportacao} IS NOT NULL
AND id NOT IN (select id from temp.cl_deputado_de_para)");

                connection.Execute(@$"
UPDATE temp.cl_despesa_temp t
SET id_cl_deputado = d.id
FROM temp.cl_deputado_de_para d 
WHERE coalesce(t.nome, t.nome_civil) ILIKE d.nome AND d.id_estado = {idEstado}");
            }
        }

        if (affected > 0)
        {
            logger.LogWarning("{Itens} parlamentares incluidos!", affected);
        }

        string sqlDeputadosNaoLocalizados;
        if (config.ChaveImportacao == ChaveDespesaTemp.NomeParlamentar || config.ChaveImportacao == ChaveDespesaTemp.NomeCivil)
        {
            sqlDeputadosNaoLocalizados = @$"
SELECT STRING_AGG(DISTINCT nome, ', ')
FROM temp.cl_despesa_temp
WHERE id_cl_deputado is null;";
        }
        else
        {
            sqlDeputadosNaoLocalizados = @$"
SELECT STRING_AGG(DISTINCT nome, ', ')
FROM temp.cl_despesa_temp
WHERE cpf NOT IN (
    SELECT {chaveImportacao}::text
    FROM assembleias.cl_deputado 
    WHERE id_estado = {idEstado} 
    AND {chaveImportacao} IS NOT null
);";
        }

        var deputadosNaoLocalizados = connection.ExecuteScalar<string>(sqlDeputadosNaoLocalizados);

        if (!string.IsNullOrEmpty(deputadosNaoLocalizados))
        {
            throw new Exception($"Deputados não cadastrados: {deputadosNaoLocalizados}");
        }
    }

    public virtual void InsereFornecedorFaltante()
    {
        logger.LogDebug("Inserir fornecedor faltante");

        ResolverFornecedor();

        var affected = connection.Execute(@"
    		INSERT INTO fornecedor.fornecedor (nome, cnpj_cpf)
    		select MAX(dt.fornecedor), dt.cnpj_cpf
    		from temp.cl_despesa_temp dt
    		where dt.id_fornecedor is null
            and dt.cnpj_cpf is not null
    		GROUP BY dt.cnpj_cpf;
    	");

        if (affected > 0)
            ResolverFornecedorPreExistente();


        List<string> despesasSemFornecedor = dbContext.CamaraEstadualDespesaTemps
            .AsNoTracking()
            .Where(x => x.IdFornecedor == null && x.NomeFornecedor != null)
            .Select(x => x.NomeFornecedor)
            .AsEnumerable()
            .DistinctBy(x => x.ToLower())
            .ToList();

        if (despesasSemFornecedor.Any())
        {
            dbContext.ChangeTracker.Clear();

            foreach (var nomeFornecedor in despesasSemFornecedor)
            {
                var fornecedor = new Infraestrutura.Entities.Fornecedores.Fornecedor()
                {
                    Nome = nomeFornecedor
                };
                dbContext.Fornecedores.Add(fornecedor);
                dbContext.SaveChanges();


                dbContext.FornecedorDeParas.Add(new FornecedorDePara()
                {
                    NomeFornecedor = nomeFornecedor,
                    IdFornecedorIncorreto = fornecedor.Id
                });

                dbContext.DeputadoFederalDespesaTemps
                    .Where(x => x.IdFornecedor == null && x.Fornecedor == nomeFornecedor)
                    .ExecuteUpdate(setters => setters.SetProperty(x => x.IdFornecedor, fornecedor.Id));
            }

            dbContext.SaveChanges();
            dbContext.ChangeTracker.Clear();
            affected += despesasSemFornecedor.Count();

            ResolverFornecedorPreExistente();
        }
        if (affected > 0)
        {
            ResolverFornecedor();
            logger.LogInformation("{Itens} fornecedores incluidos!", affected);
        }

        var sql = $@"update temp.cl_despesa_temp set id_fornecedor = 82624 where id_fornecedor is NULL and fornecedor is NULL";
        affected = connection.Execute(sql);
        if (affected > 0)
        {
            logger.LogWarning("{Itens} despesas com fornecedores não identificados ajustados para <Não Informado>!", affected);
        }
    }

    public void RemoveDespesas(int ano)
    {
        var affected = connection.Execute(@$"
DELETE FROM assembleias.cl_despesa d 
join assembleias.cl_deputado p on p.id = d.id_cl_deputado 
where p.id_estado = {idEstado}
and d.ano_mes BETWEEN {competenciaInicial} and {competenciaFinal}
			");

    }

    public virtual void InsereDespesaFinal(int ano)
    {
        var totalTemp = connection.ExecuteScalar<int>("select count(1) from temp.cl_despesa_temp");
        if (totalTemp == 0) return;

        logger.LogDebug("Inserir despesa final");

        string condicaoSql = "";
        if (config.ChaveImportacao == ChaveDespesaTemp.Cpf)
            condicaoSql = "p.cpf = d.cpf";
        else if (config.ChaveImportacao == ChaveDespesaTemp.CpfParcial)
            condicaoSql = "p.cpf_parcial = d.cpf";
        else if (config.ChaveImportacao == ChaveDespesaTemp.Matricula)
            condicaoSql = "p.matricula::text = d.cpf";
        else if (config.ChaveImportacao == ChaveDespesaTemp.Gabinete)
            condicaoSql = "p.gabinete::text = d.cpf";
        else
        {
            condicaoSql = "p.id = d.id_cl_deputado";
        }

        var sql = @$"
INSERT INTO assembleias.cl_despesa (
	id_cl_deputado,
    id_cl_despesa_tipo,
    id_cl_despesa_especificacao,
	id_fornecedor,
	data_emissao,
	ano_mes,
	numero_documento,
	valor_liquido,
    favorecido,
    observacao,
    hash
)
SELECT 
	p.id AS id_cl_deputado,
    dts.id_cl_despesa_tipo,
    dts.id,
    d.id_fornecedor,
    d.data_emissao,
    cast(concat(d.ano, LPAD(d.mes::text, 2, '0')) AS INT) AS ano_mes,
    d.documento AS numero_documento,
    d.valor AS valor,
    d.favorecido,
    COALESCE(d.observacao, CASE WHEN d.cnpj_cpf IS NULL AND d.fornecedor IS NOT NULL THEN CONCAT('Fornecedor Original: ', d.fornecedor) else null END) AS observacao,
    d.hash
FROM temp.cl_despesa_temp d
inner join assembleias.cl_deputado p on id_estado = {idEstado} and {condicaoSql}
left join assembleias.cl_despesa_especificacao dts on lower(unaccent(dts.descricao)) = lower(unaccent(d.despesa_tipo))
ORDER BY d.id
ON CONFLICT DO NOTHING;
			"; // TODO: Remover ON CONFLICT DO NOTHING

        var affected = connection.Execute(sql);

        if (affected != totalTemp)
            logger.LogError("{Itens} despesas incluidas. Há {Qtd} despesas que foram ignoradas!", affected, totalTemp - affected);
        else if (affected > 0)
            logger.LogInformation("{Itens} despesas incluidas!", affected);
    }

    public virtual void ValidaImportacao(int ano)
    {
        logger.LogDebug("Validar importação");

        var agregador = RegistrosBaseDeDadosFinalAgregados(ano);
        int itensTotalFinal = agregador.Item1;
        decimal somaTotalFinal = agregador.Item2;

        if (itensProcessadosAno != itensTotalFinal || somaTotalFinal != valorTotalProcessadoAno)
        {
            logger.LogError(
                "Totais divergentes! " +
                "Arquivo: [Itens: {LinhasArquivo}; Valor: {ValorTotalArquivo:#,###.00}] " +
                "DB: [Itens: {LinhasDB}; Valor: R$ {ValorTotalFinal:#,###.00}] " +
                "Diferença: [Itens: {LinhasDiferenca}; Valor: R$ {ValorTotalDiferenca:#,###.00}]",
                itensProcessadosAno, valorTotalProcessadoAno, itensTotalFinal, somaTotalFinal, itensProcessadosAno - itensTotalFinal, valorTotalProcessadoAno - somaTotalFinal);

            var despesasSemParlamentar = connection.ExecuteScalar<string>(@$"
SELECT STRING_AGG(DISTINCT d.nome, ', ')
FROM temp.cl_despesa_temp d
WHERE d.id_cl_deputado IS NULL");

            if (!string.IsNullOrEmpty(despesasSemParlamentar))
                logger.LogError("Há deputados não identificados! {ListaParlamentar}", despesasSemParlamentar);
        }
        else
        {
            logger.LogInformation("Itens na base de dados: {LinhasDB}; Valor total: R$ {ValorTotalFinal:#,###.00}", itensTotalFinal, somaTotalFinal);

        }

        if (ano == DateTime.UtcNow.Year)
        {
            var ultimaDataEmissao = connection.ExecuteScalar<DateOnly>($@"
SELECT MAX(d.data_emissao) AS ultima_emissao
FROM assembleias.cl_despesa d
join assembleias.cl_deputado p ON p.id = d.id_cl_deputado AND p.id_estado = {idEstado}").ToDateTime(TimeOnly.MinValue);

            if (ultimaDataEmissao > DateTime.Today)
                logger.LogWarning("Ultima data de emissão é uma data futura: {DataEmissao:dd/MM/yyyy}", ultimaDataEmissao);
            else if (ultimaDataEmissao < DateTime.Today.AddMonths(-1))
                logger.LogWarning("Ultima data de emissão é anterior a um mês: {DataEmissao:dd/MM/yyyy}", ultimaDataEmissao);
            else
                logger.LogInformation("Ultima data de emissão '{DataEmissao:dd/MM/yyyy}' foi a {EmissaoDias} dias", ultimaDataEmissao, Math.Abs((ultimaDataEmissao - DateTime.Today).TotalDays));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ano"></param>
    /// <returns>Numero de itens e Soma dos valores</returns>
    public virtual (int, decimal) RegistrosBaseDeDadosFinalAgregados(int ano)
    {
        var sql = @$"
select 
    count(1) as itens, 
    coalesce(sum(valor_liquido), 0) as valor_total 
FROM assembleias.cl_despesa d 
join assembleias.cl_deputado p on p.id = d.id_cl_deputado 
where p.id_estado = {idEstado}
and d.ano_mes between {competenciaInicial} and {competenciaFinal}";

        using (IDataReader reader = connection.ExecuteReader(sql))
        {
            if (reader.Read())
            {
                return (
                    Convert.ToInt32(reader["itens"].ToString()),
                    Convert.ToDecimal(reader["valor_total"].ToString())
                );
            }
        }

        return default;
    }

    public virtual void LimpaDespesaTemporaria()
    {
        connection.Execute("truncate table temp.cl_despesa_temp");
    }

    public virtual void DeletarDespesasInexistentes(int ano)
    {
        logger.LogDebug("Sincroniza Hashes");

        var agregador = RegistrosBaseDeDadosFinalAgregados(ano);
        int itensTotalFinal = agregador.Item1;
        decimal somaTotalFinal = agregador.Item2;

        logger.LogInformation(
            "Já existem {Itens} despesas com valor total de {ValorTotal:#,###.00} para {Estado} em {Ano} previamente importados.",
            itensTotalFinal, somaTotalFinal, config.Estado.ToString(), ano);

        if (lstHash.Any())
        {
            var hashCount = lstHash.Count();
            var itensDbCount = connection.ExecuteScalar<int>("SELECT count(1) FROM temp.cl_despesa_temp");
            if (hashCount > (itensDbCount + 10))
            {
                logger.LogError("Tentando remover ({ItensArquivo}) mais itens que estamos incluindo ({ItensDB})! Verifique a importação.", hashCount, itensDbCount);
                throw new BusinessException($"Tentando remover ({hashCount}) mais itens que estamos incluindo ({itensDbCount})! Verifique a importação.");
            }

            string idsToDelete = string.Join(",", lstHash.Values.Select(x => x));
            var deleted = dbContext.Database.ExecuteSqlRaw(
                $"DELETE FROM assembleias.cl_despesa WHERE id IN ({idsToDelete})"
            );

            logger.LogInformation("{TotalDeleted} despesas removidas!", hashCount);
        }
    }

    public virtual void CarregarHashes(int ano)
    {
        DefinirCompetencias(ano);

        valorTotalProcessadoAno = 0;
        itensProcessadosAno = 0;

        lstHash = new Dictionary<string, long>();
        var sql = $"select d.id, d.hash FROM assembleias.cl_despesa d join assembleias.cl_deputado p on d.id_cl_deputado = p.id where p.id_estado = {idEstado} and d.ano_mes between {competenciaInicial} and {competenciaFinal}";
        IEnumerable<dynamic> lstHashDB = connection.Query(sql);

        foreach (IDictionary<string, object> dReader in lstHashDB)
        {
            if (dReader["hash"] == null) continue;

            var hex = Convert.ToHexString((byte[])dReader["hash"]);
            if (!lstHash.ContainsKey(hex))
                lstHash.Add(hex, (long)dReader["id"]);
            else
                logger.LogError("Hash {HASH} esta duplicada na base de dados.", hex);
        }

        logger.LogInformation("{Total} Hashes Carregados", lstHash.Count());
    }

    public virtual void DefinirCompetencias(int ano)
    {
        competenciaInicial = $"{ano}01";
        competenciaFinal = $"{ano}12";
    }

    ///// <summary>
    ///// Registro já existe na base de dados?
    ///// </summary>
    ///// <param name="despesa"></param>
    ///// <param name="lstHash"></param>
    ///// <returns></returns>
    //public virtual bool RegistroExistente(CamaraEstadualDespesaTemp despesa)
    //{
    //    var str = JsonSerializer.Serialize(despesa);
    //    var hash = Utils.SHA1Hash(str);

    //    if (lstHash.Remove(Convert.ToHexString(hash)))
    //        return true;

    //    despesa.Hash = hash;
    //    //logger.LogDebug("Novo Registro {@CamaraEstadualDespesa}", despesa);
    //    return false;
    //}

    public DeputadoEstadual GetDeputadoByNameOrNew(string nomeParlamentar)
    {
        var deputado = dbContext.DeputadosEstaduais
            .Where(d => d.IdEstado == idEstado && d.NomeParlamentar == nomeParlamentar)
            .FirstOrDefault();


        return deputado ?? new DeputadoEstadual()
        {
            IdEstado = (short)idEstado,
            NomeParlamentar = nomeParlamentar
        };
    }

    public DeputadoEstadual GetDeputadoByMatriculaOrNew(int matricula)
    {
        var deputado = dbContext.DeputadosEstaduais
            .Where(d => d.IdEstado == idEstado && d.Matricula == matricula)
            .FirstOrDefault();

        return deputado ?? new DeputadoEstadual()
        {
            IdEstado = (short)idEstado,
            Matricula = matricula
        };
    }

    public void CarregarDespesaTempDoHistorico(int ano, int mes, Expression<Func<CamaraEstadualDespesaTemp, bool>> filtroAdicional = null)
    {
        var query = despesasHistorico
            .Where(x => x.Ano == ano && x.Mes == mes)
            .AsQueryable();

        if (filtroAdicional != null)
        {
            query = query.Where(filtroAdicional);
        }

        var despesasTempDoHistorico = query.ToList();

        foreach (var despesaTemp in despesasTempDoHistorico)
        {
            PadronizarValoresDespesasTemp(despesaTemp);

            despesaTemp.Hash = GerarHashTemp(despesaTemp);
            despesasTemp.Add(despesaTemp);
        }

        logger.LogInformation("{QtdDespesas} Despesas do mês {Mes:00}/{Ano} carregadas do histórico.", despesasTempDoHistorico.Count(), mes, ano);
    }

    public void InserirDespesaTemp(CamaraEstadualDespesaTemp despesaTemp)
    {
        if (despesaTemp.DataEmissao != null)
        {
            var endMonthDate = new DateOnly(despesaTemp.Ano, despesaTemp.Mes ?? despesaTemp.DataEmissao.Value.Month, 1).AddMonths(1).AddDays(-1);

            if (despesaTemp.DataEmissao?.Year != despesaTemp.Ano && despesaTemp.DataEmissao?.AddMonths(3).Year != despesaTemp.Ano)
            {
                // Validar ano com 3 meses de tolerancia.
                //logger.LogWarning("Despesa com ano incorreto: {@Despesa}", despesaTemp);

                var dt = despesaTemp.DataEmissao!.Value;
                despesaTemp.DataEmissao = new DateOnly(despesaTemp.Ano, dt.Month, dt.Day);
            }
            else if (despesaTemp.DataEmissao > endMonthDate)
            {
                // Validar despesa com data futura.
                //logger.LogWarning("Despesa com data incorreta/futura: {@Despesa}", despesaTemp);

                var dt = despesaTemp.DataEmissao!.Value;
                // Tentamos trocar apenas o ano.
                despesaTemp.DataEmissao = new DateOnly(despesaTemp.Ano, dt.Month, dt.Day);

                // Caso a data permaneça invalida, alteramos também o mês, se possivel.
                if (despesaTemp.DataEmissao > endMonthDate && despesaTemp.Mes.HasValue)
                {
                    var monthLastDay = DateTime.DaysInMonth(despesaTemp.Ano, despesaTemp.Mes.Value);
                    despesaTemp.DataEmissao = new DateOnly(despesaTemp.Ano, despesaTemp.Mes.Value, Math.Min(dt.Day, monthLastDay));
                }
            }
        }

        if (despesaTemp.Valor == 0)
        {
            logger.LogDebug("Ignorando item com valor zerado: {Despesa}", JsonSerializer.Serialize(despesaTemp, jsonSerializeOptions));
            return;
        }

        PadronizarValoresDespesasTemp(despesaTemp);
        despesaTemp.Hash = GerarHashTemp(despesaTemp);

        despesasTemp.Add(despesaTemp);

        //Console.WriteLine(str);
        //Console.WriteLine("");
    }

    private byte[] GerarHashTemp(CamaraEstadualDespesaTemp despesaTemp)
    {
        var str = JsonSerializer.Serialize(despesaTemp, jsonSerializeOptionsIgnoreDecimalValues);
        var hash = Utils.SHA1Hash(str);
        //var key = Convert.ToHexString(hash);

        return hash;
    }

    static void IgnoreNumericValues(JsonTypeInfo typeInfo)
    {
        foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties)
        {
            if (propertyInfo.PropertyType == typeof(decimal))
            {
                propertyInfo.ShouldSerialize = static (obj, value) => false;
            }
        }
    }

    public async Task<T> RestApiGet<T>(string address)
    {
        using RestClient client = CreateHttpClient();

        var request = new RestRequest(address);
        request.Timeout = TimeSpan.FromMinutes(5);
        request.AddHeader("Accept", "application/json");

        return await client.GetAsync<T>(request);
    }

    public async Task<T> RestApiGetWithCustomDateConverter<T>(string address)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new DateTimeOffsetConverterUsingDateTimeParse());

        var request = new RestRequest(address);
        request.AddHeader("Accept", "application/json");

        //if (config.Estado == Estados.Parana)
        //{
        //    request.AddHeader("recaptchaprivatetoken", "a5fbdde6daed041e187f0162c0f116faa22fcb93324099827c20b7cd61e06251b10db5956624fb548e5d69bfc13ac4c32fd5dd90e10393846faeb9c2a5e0ad02");
        //    request.AddHeader("recaptchapublictoken", TokenHelper.GerarTokenAngular("03AFcWeA6hm4oJfOh3MRN76AQtvHCIcOz9bq93ermGjGho_-g9s7XuV6sAbcFsiSGsg7vPfxwmTOTjdnBh6h-eRD9t6XLu4rX_BYiX2zm2aBNAkp4hjxP0uYxfbv622WrvvNEJxRvfEs7Oz2u2e4UQwwhUGE1AHy8JcxOE-Hqi_dYpD0efkh-dT9c5LKRazS-BOePcToEadiWYTndFcGxSYNrgtdRKjzj1JzkFvOD9HXJPeIoJDwMkVzIFxTqL-voQN69Y_CNKUus2MpstmovojIpvtkoqBvJ1A7R0Ic39ztePFkUsnDlbNYfJSqyclcP66PbKxrPzC4U9MH5O9fnyYbp6_wUg5E1RpzOdK6oV7JVLMxxFb-kY74hdelYyXU5qzyYiMyhUlHeqk8W_OgUmVXMzD7M0cZAQPDmgqupI-v6m5KQpG7zZnIY24cY_JCP5Vd0RqlSQ34I5wb8Wqmb67XAfFb0c3JTu_nZoYt8uYJTOBchbhGEOEQvC5IsBDRKe-QaZv27Ht3NeOq-4bSChQUuwWWEraH7QSYal7wHpHXj9nyboXsEzrfMGHvmWlmFZnMKXugNpYxruXPmet4bvb6VWlEMN4f5Z8x0OzsNtYvFKaiYJXvtZ8HvhROrtaEsLUCce7EkBvH_2n1C8YdPuVzDADRFObvVR4bRrb21haRNLN8Pai8N6xr6_CXdldzrP9bNiEqq0xr8BBogU0erx0z_ksHe9xOJTXvu-H2zSI94kiihbnMVsRF3BCGW_2OAzBfl6ba6AeCLiZN_vJSc3VZqtJviIC73sc-vNGBZo9JaSjORWMDpHFtoOhTjM2eM9uNQXBUJ5Jk7EhpoAtOVVSrMth9nS8Z6Vzb2G09XOo133xczomcdq9ZfbXX95VtVe84W0oxs6Oe_D0X0SybA-lvjcCH7pCmCrI-C8TKxDZLLHuPp0Kv55BRKBOLkSgSALJHc3hy_unEF1-dhac_SRC1ekBXRo1AloOg"));
        //}

        using RestClient client = CreateHttpClient();
        var response = await client.GetAsync(request);

        return JsonSerializer.Deserialize<T>(response.Content, options);

    }

    public async Task<T> RestApiGetWithSqlTimestampConverter<T>(string address)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new SqlTimestampConverter());

        var request = new RestRequest(address);
        request.AddHeader("Accept", "application/json");

        using RestClient client = CreateHttpClient();
        var response = await client.GetAsync(request);

        return JsonSerializer.Deserialize<T>(response.Content, options);

    }

    public async Task<T> RestApiGetAsync<T>(string address)
    {
        using RestClient client = CreateHttpClient();

        var request = new RestRequest(address);
        request.AddHeader("Accept", "application/json");

        try
        {
            return await client.GetAsync<T>(request);
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Timeout ao acessar '{Address}'. Aguardando 1 minuto para nova tentativa.", address);
            await Task.Delay(TimeSpan.FromMinutes(1));

            return await client.GetAsync<T>(request);
        }
    }

    public async Task<T> RestApiPostAsync<T>(string address, Dictionary<string, string> parameters)
    {
        using RestClient client = CreateHttpClient();

        var request = new RestRequest(address + ToQueryString(parameters));
        request.AddHeader("Accept", "application/json");

        foreach (var p in parameters)
            request.AddParameter(p.Key, p.Value);

        return await client.PostAsync<T>(request);
    }

    public RestClient CreateHttpClient()
    {
        var options = new RestClientOptions()
        {
            ThrowOnAnyError = true
        };
        return new RestClient(httpClientResilient, options);
    }

    private string ToQueryString(Dictionary<string, string> parameters)
    {
        if (!parameters.Any()) return string.Empty;

        return "?" + string.Join("&", parameters.Select(kvp =>
            $"{kvp.Key}={kvp.Value}"));
        //$"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
    }

    public virtual void ValidaValorTotal(string filename, decimal valorTotalArquivo, decimal valorTotalCalculado, int despesasIncluidas)
    {
        var diferenca = Math.Abs(valorTotalArquivo - valorTotalCalculado);
        fileManager.AtualizaValorTotal(dbContext, filename, valorTotalArquivo, valorTotalCalculado);

        if (diferenca > 100)
        {
            var valores = despesasTemp.Where(x => x.Origem == filename).Select(x => x.Valor.ToString("F2")).ToList();
            var valoresStr = string.Join(", ", valores);

            using (logger.BeginScope(new Dictionary<string, object> { ["Valores"] = valoresStr }))
            {
                logger.LogError("Valor Divergente! Esperado: {ValorTotalArquivo}; Encontrado: {ValorTotalCalculado}; Diferenca: {Diferenca}; Despesas Incluidas: {DespesasIncluidas}",
                valorTotalCalculado, valorTotalArquivo, diferenca, despesasIncluidas);
            }
        }
        else if (diferenca > 0)
        {
            logger.LogWarning("Valor Divergente! Esperado: {ValorTotalArquivo}; Encontrado: {ValorTotalCalculado}; Diferenca: {Diferenca}; Despesas Incluidas: {DespesasIncluidas}",
                valorTotalCalculado, valorTotalArquivo, diferenca, despesasIncluidas);
        }
    }

    public void AtualizarDatasImportacaoDespesas(DateTime? dInicio = null, DateTime? dFim = null)
    {
        var importacao = dbContext.Importacoes.FirstOrDefault(x => x.Chave == config.Estado.ToString());
        if (importacao == null)
        {
            importacao = new Importacao()
            {
                Chave = config.Estado.ToString()
            };
            dbContext.Importacoes.Add(importacao);
        }

        if (dInicio != null)
        {
            importacao.DespesasInicio = dInicio.Value;
            importacao.DespesasFim = null;
        }

        if (dFim != null)
        {
            importacao.DespesasFim = dFim.Value;

            var sql = @"
select 
    min(d.data_emissao) as primeira_despesa, 
    max(d.data_emissao) as ultima_despesa 
FROM assembleias.cl_despesa d
join assembleias.cl_deputado p ON p.id = d.id_cl_deputado
where p.id_estado = @idEstado";
            using (var dReader = connection.ExecuteReader(sql, new { idEstado }))
            {
                if (dReader.Read())
                {
                    importacao.PrimeiraDespesa = dReader["primeira_despesa"] != DBNull.Value ? DateOnly.Parse(dReader["primeira_despesa"].ToString()) : (DateOnly?)null;
                    importacao.UltimaDespesa = dReader["ultima_despesa"] != DBNull.Value ? DateOnly.Parse(dReader["ultima_despesa"].ToString()) : (DateOnly?)null;
                }
            }
        }

        dbContext.SaveChanges();
    }

    /// <summary>
    /// Atualiza indicador 'Campeões de gastos',
    /// Os 4 deputados que mais gastaram com a CEAP desde o ínicio do mandato 55 (02/2015)
    /// </summary>
    public void AtualizaCampeoesGastos()
    {
        connection.Execute(@"
TRUNCATE TABLE assembleias.cl_deputado_campeao_gasto;

INSERT INTO assembleias.cl_deputado_campeao_gasto
SELECT l1.id_cl_deputado, d.nome_parlamentar, l1.valor_total, p.sigla, e.sigla
FROM (
    SELECT 
    	l.id_cl_deputado,
    	sum(l.valor_liquido) as valor_total
    FROM  assembleias.cl_despesa l
    GROUP BY l.id_cl_deputado
    order by valor_total desc 
    limit 4
) l1 
INNER JOIN assembleias.cl_deputado d on d.id = l1.id_cl_deputado 
LEFT JOIN partido p on p.id = d.id_partido
LEFT JOIN estado e on e.id = d.id_estado");
    }

//    public void AtualizaResumoMensal()
//    {
//        connection.Execute(@"
//TRUNCATE TABLE assembleias.cl_despesa_resumo_mensal;
//INSERT INTO assembleias.cl_despesa_resumo_mensal (ano, mes, valor)
//SELECT ano, mes, sum(valor_liquido)
//FROM assembleias.cl_despesa
//GROUP BY ano, mes
//");
//    }
}

public class ImportadorCotaParlamentarBaseConfig
{
    public string BaseAddress { get; set; }

    public bool Completo { get; private set; } = false;

    public Estados Estado { get; set; }

    //public ChaveDespesaTemp ChaveImportacaoDeputado { get; private set; } = ChaveDespesaTemp.Indefinido;

    public ChaveDespesaTemp ChaveImportacao { get; set; } = ChaveDespesaTemp.Indefinido;
}

