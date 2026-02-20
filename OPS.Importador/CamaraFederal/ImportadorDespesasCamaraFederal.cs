using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AngleSharp;
using AngleSharp.Html.Dom;
using CsvHelper;
using Dapper;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPS.Core.Exceptions;
using OPS.Core.Utilities;
using OPS.Importador.Comum;
using OPS.Importador.Comum.Despesa;
using OPS.Importador.Comum.Utilities;
using OPS.Importador.Fornecedores;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.CamaraFederal;
using RestSharp;

namespace OPS.Importador.CamaraFederal;

public class ImportadorDespesasCamaraFederal : IImportadorDespesas
{
    const string LEGISLATURA_ANO_MES = "202302";
    protected readonly ILogger<ImportadorDespesasCamaraFederal> logger;
    protected readonly AppSettings appSettings;
    protected readonly FileManager fileManager;
    protected readonly IServiceProvider serviceProvider;
    protected readonly AppDbContext dbContext;
    protected IDbConnection connection { get { return dbContext.Database.GetDbConnection(); } }

    private int itensProcessadosAno { get; set; }
    private decimal valorTotalProcessadoAno { get; set; }

    private const int legislaturaAtual = 57;

    public HttpClient httpClient { get; }
    public CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasCamaraFederal(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        fileManager = serviceProvider.GetRequiredService<FileManager>();
        logger = serviceProvider.GetRequiredService<ILogger<ImportadorDespesasCamaraFederal>>();
        httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("ResilientClient");
    }

    public void AtualizarDatasImportacaoDespesas(DateTime? dInicio = null, DateTime? dFim = null)
    {
        var importacao = dbContext.Importacoes.FirstOrDefault(x => x.Chave == "Camara Federal");
        if (importacao == null)
        {
            importacao = new Importacao()
            {
                Chave = "Camara Federal"
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

            var sql = "select min(data_emissao) as primeira_despesa, max(data_emissao) as ultima_despesa from camara.cf_despesa";
            using (var dReader = connection.ExecuteReader(sql))
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

    #region Importação Dados CEAP CSV

    public async Task Importar(int ano)
    {
        logger.LogDebug("Despesas do(a) Camara Federal de {Ano}", ano);

        Dictionary<string, string> arquivos = DefinirUrlOrigemCaminhoDestino(ano);

        foreach (var arquivo in arquivos)
        {
            var urlOrigem = arquivo.Key;
            var caminhoArquivo = arquivo.Value;

            bool novoArquivoBaixado = await fileManager.BaixarArquivo(dbContext, urlOrigem, caminhoArquivo, null);
            if (!appSettings.ForceImport && !novoArquivoBaixado)
            {
                logger.LogInformation("Importação ignorada para arquivo previamente importado!");
                return;
            }

            try
            {
                if (caminhoArquivo.EndsWith(".zip"))
                {
                    fileManager.DescompactarArquivo(caminhoArquivo);
                    caminhoArquivo = caminhoArquivo.Replace(".zip", "");
                }

                ImportarDespesas(caminhoArquivo, ano);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                fileManager.MoverArquivoComErro(caminhoArquivo);
            }
        }
    }

    
    
    
    
    
    
    public Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
    {
        var downloadUrl = "https:
        var fullFileNameZip = Path.Combine(appSettings.TempFolder, "CamaraFederal", $"Ano-{ano}.csv.zip");

        Dictionary<string, string> arquivos = new();
        arquivos.Add(downloadUrl, fullFileNameZip);

        return arquivos;
    }

    private readonly string[] ColunasCEAP = {
            "txNomeParlamentar","cpf","ideCadastro","nuCarteiraParlamentar","nuLegislatura","sgUF","sgPartido","codLegislatura",
            "numSubCota","txtDescricao","numEspecificacaoSubCota","txtDescricaoEspecificacao","txtFornecedor","txtCNPJCPF",
            "txtNumero","indTipoDocumento","datEmissao","vlrDocumento","vlrGlosa","vlrLiquido","numMes","numAno","numParcela",
            "txtPassageiro","txtTrecho","numLote","numRessarcimento","datPagamentoRestituicao", "vlrRestituicao","nuDeputadoId","ideDocumento","urlDocumento"
        };

    public void ImportarDespesas(string file, int ano)
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        LimpaDespesaTemporaria();

        itensProcessadosAno = 0;
        valorTotalProcessadoAno = 0;
        var totalColunas = ColunasCEAP.Length;
        var lstHash = new Dictionary<string, long>();

        
        {
            var sql = $"select id, hash FROM camara.cf_despesa where ano={ano} and hash IS NOT NULL";
            IEnumerable<dynamic> lstHashDB = connection.Query(sql);

            foreach (IDictionary<string, object> dReader in lstHashDB)
            {
                var hex = Convert.ToHexString((byte[])dReader["hash"]);
                if (!lstHash.ContainsKey(hex))
                    lstHash.Add(hex, (long)dReader["id"]);
                else
                    logger.LogError("Hash {HASH} esta duplicada na base de dados.", hex);
            }

            logger.LogInformation("{Total} Hashes Carregados", lstHash.Count());

            var despesasTemp = new List<DeputadoFederalDespesaTemp>();
            var cultureInfo = new CultureInfo("en-US");

            var config = new CsvHelper.Configuration.CsvConfiguration(cultureInfo);
            config.BadDataFound = null;
            config.Delimiter = ";";
            
            
            

            using (var reader = new StreamReader(file, Encoding.GetEncoding("UTF-8")))
            using (var csv = new CsvReader(reader, config))
            {
                if (csv.Read())
                {
                    for (int i = 0; i < totalColunas - 1; i++)
                    {
                        if (csv[i] != ColunasCEAP[i])
                        {
                            throw new Exception("Mudança de integraçao detectada para o Câmara Federal");
                        }
                    }
                }

                while (csv.Read())
                {
                    var idxColuna = 0;
                    var despesaTemp = new DeputadoFederalDespesaTemp();
                    despesaTemp.NomeParlamentar = csv.GetField(idxColuna++);
                    despesaTemp.Cpf = Utils.RemoveCaracteresNaoNumericos(csv.GetField(idxColuna++)).NullIfEmpty();
                    despesaTemp.IdDeputado = csv.GetField<long?>(idxColuna++);
                    despesaTemp.NumeroCarteiraParlamentar = csv.GetField<int?>(idxColuna++);
                    despesaTemp.Legislatura = csv.GetField<int?>(idxColuna++);
                    despesaTemp.SiglaUF = csv.GetField(idxColuna++).Replace("NA", string.Empty).NullIfEmpty();
                    despesaTemp.SiglaPartido = csv.GetField(idxColuna++).NullIfEmpty();
                    despesaTemp.CodigoLegislatura = csv.GetField<int?>(idxColuna++);
                    despesaTemp.NumeroSubCota = csv.GetField<int?>(idxColuna++);
                    despesaTemp.Descricao = csv.GetField(idxColuna++);
                    despesaTemp.NumeroEspecificacaoSubCota = csv.GetField<int?>(idxColuna++)?.NullIf(0);
                    despesaTemp.DescricaoEspecificacao = csv.GetField(idxColuna++).NullIfEmpty();
                    despesaTemp.Fornecedor = csv.GetField(idxColuna++);
                    despesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(csv.GetField(idxColuna++));
                    despesaTemp.Numero = csv.GetField(idxColuna++);
                    despesaTemp.TipoDocumento = csv.GetField<int?>(idxColuna++) ?? 0;
                    despesaTemp.DataEmissao = csv.GetField<DateOnly?>(idxColuna++);

                    var valorDocumento = csv.GetField(idxColuna++);
                    if (!string.IsNullOrEmpty(valorDocumento))
                        despesaTemp.ValorDocumento = Convert.ToDecimal(valorDocumento, cultureInfo);

                    despesaTemp.ValorGlosa = Convert.ToDecimal(csv.GetField(idxColuna++), cultureInfo);
                    despesaTemp.ValorLiquido = Convert.ToDecimal(csv.GetField(idxColuna++), cultureInfo);
                    despesaTemp.Mes = csv.GetField<short>(idxColuna++);
                    despesaTemp.Ano = csv.GetField<short>(idxColuna++);
                    despesaTemp.Parcela = csv.GetField<int?>(idxColuna++)?.NullIf(0);
                    despesaTemp.Passageiro = csv.GetField(idxColuna++).NullIfEmpty();
                    despesaTemp.Trecho = csv.GetField(idxColuna++).NullIfEmpty();
                    despesaTemp.Lote = csv.GetField<int?>(idxColuna++);
                    despesaTemp.Ressarcimento = csv.GetField<int?>(idxColuna++);
                    despesaTemp.DataPagamentoRestituicao = csv.GetField<DateOnly?>(idxColuna++);
                    despesaTemp.Restituicao = csv.GetField<decimal?>(idxColuna++);
                    despesaTemp.NumeroDeputadoID = csv.GetField<int?>(idxColuna++);
                    despesaTemp.IdDocumento = csv.GetField(idxColuna++);
                    despesaTemp.UrlDocumento = csv.GetField(idxColuna++);

                    if (despesaTemp.ValorDocumento == null)
                        despesaTemp.ValorDocumento = despesaTemp.ValorLiquido + despesaTemp.ValorGlosa;

                    if (despesaTemp.UrlDocumento.Contains("/documentos/publ"))
                        despesaTemp.UrlDocumento = "1"; 
                    else if (despesaTemp.UrlDocumento.Contains("/nota-fiscal-eletronica"))
                        despesaTemp.UrlDocumento = "2"; 
                    else
                    {
                        if (!string.IsNullOrEmpty(despesaTemp.UrlDocumento))
                            logger.LogError("Documento '{Valor}' não reconhecido!", despesaTemp.UrlDocumento);

                        despesaTemp.UrlDocumento = "0";
                    }

                    if (!string.IsNullOrEmpty(despesaTemp.Passageiro))
                    {
                        despesaTemp.Passageiro = despesaTemp.Passageiro.ToString().Split(";")[0];
                        string[] partes = despesaTemp.Passageiro.ToString().Split(new[] { '/', ';' });
                        if (partes.Length > 1)
                        {
                            var antes = despesaTemp.Passageiro;
                            despesaTemp.Passageiro = "";
                            for (int y = partes.Length - 1; y >= 0; y--)
                            {
                                despesaTemp.Passageiro += " " + partes[y];
                            }
                        }
                    }

                    if (despesaTemp.DataEmissao == null)
                        despesaTemp.DataEmissao = new DateOnly(despesaTemp.Ano, despesaTemp.Mes.Value, 1);
                    else
                    {
                        
                        var dataReferencia = new DateOnly(despesaTemp.Ano, despesaTemp.Mes ?? despesaTemp.DataEmissao.Value.Month, 15);
                        var dataEmissao = despesaTemp.DataEmissao!.Value;
                        var diferencaDias = Math.Abs((dataReferencia.ToDateTime(TimeOnly.MinValue) - dataEmissao.ToDateTime(TimeOnly.MinValue)).Days);

                        if (diferencaDias > 120) 
                        {
                            logger.LogWarning("Data da despesa muito diferente do ano/mês informado. Parlamentar: {Parlamentar}, Data: {Data}, Ano/Mês: {Ano}/{Mes}",
                                despesaTemp.NomeParlamentar, despesaTemp.DataEmissao?.ToString("yyyy-MM-dd"), despesaTemp.Ano, despesaTemp.Mes);

                            var monthLastDay = DateTime.DaysInMonth(despesaTemp.Ano, dataEmissao.Month);
                            despesaTemp.DataEmissao = new DateOnly(despesaTemp.Ano, dataEmissao.Month, Math.Min(dataEmissao.Day, monthLastDay));
                        }
                    }


                    if (despesaTemp.CnpjCpf.StartsWith("000000000000"))
                    {
                        if (despesaTemp.CnpjCpf != "00000000000001" 
                            && despesaTemp.CnpjCpf != "00000000000002" 
                            && despesaTemp.CnpjCpf != "00000000000003" 
                            && despesaTemp.CnpjCpf != "00000000000006" 
                            && despesaTemp.CnpjCpf != "00000000000007" 
                            && despesaTemp.CnpjCpf != "00000000000008" 
                            && despesaTemp.CnpjCpf != "00000000000009" 
                            )
                        {
                            if (despesaTemp.CnpjCpf != "00000000000010") 
                                logger.LogWarning("Validar CPNJ '{CnpjCpf} - {NomeEmpresa}'.", despesaTemp.CnpjCpf, despesaTemp.Fornecedor);

                            despesaTemp.CnpjCpf = null;
                        }

                    }
                    else if (!string.IsNullOrEmpty(despesaTemp.CnpjCpf) && !Utils.IsCpf(despesaTemp.CnpjCpf) && !Utils.IsCnpj(despesaTemp.CnpjCpf))
                    {
                        logger.LogWarning("CNPJ/CPF inválido na despesa. Deputado: {Deputado}, CNPJ/CPF: {CnpjCpf} - {NomeEmpresa}",
                           despesaTemp.NomeParlamentar, despesaTemp.CnpjCpf, despesaTemp.Fornecedor);
                    }

                    
                    
                    var options = new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                        TypeInfoResolver = new DefaultJsonTypeInfoResolver
                        {
                            Modifiers = { IgnoreNumericValues },
                        }
                    };

                    var str = JsonSerializer.Serialize(despesaTemp, options);
                    despesaTemp.Hash = Utils.SHA1Hash(str);

                    despesasTemp.Add(despesaTemp);
                    itensProcessadosAno++;
                    valorTotalProcessadoAno += despesaTemp.ValorLiquido ?? 0;
                }
            }

            if (itensProcessadosAno > 0)
                InsereDespesasTemp(despesasTemp, lstHash);

            if (lstHash.Any())
            {
                string idsToDelete = string.Join(",", lstHash.Values.Select(x => x));
                var deleted = dbContext.Database.ExecuteSqlRaw(
                    $"DELETE FROM camara.cf_despesa WHERE id IN ({idsToDelete})"
                );
            }

            if (itensProcessadosAno > 0)
            {
                ProcessarDespesasTemp(ano);
            }

            ValidaImportacao(ano);

            
            

            dbContext.ChangeTracker.Clear();
        }


        logger.LogWarning("Importados {Items} registros com valor de {ValorTotal}", itensProcessadosAno, valorTotalProcessadoAno);


        if (ano == DateTime.Now.Year && (itensProcessadosAno > 0 || lstHash.Count > 0))
        {
            AtualizaParlamentarValoresCEAP();
            AtualizaCampeoesGastos();
            AtualizaResumoMensal();
        }
    }

    private void InsereDespesasTemp(List<DeputadoFederalDespesaTemp> despesasTemp, Dictionary<string, long> lstHash)
    {
        JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };

        List<IGrouping<string, DeputadoFederalDespesaTemp>> results = despesasTemp.GroupBy(x => Convert.ToHexString(x.Hash)).ToList();
        itensProcessadosAno = results.Count();
        valorTotalProcessadoAno = results.Sum(x => x.Sum(x => x.ValorLiquido ?? 0));

        logger.LogInformation("Processando {Itens} despesas agrupadas em {Unicos} unicas com valor total de {ValorTotal}.", despesasTemp.Count(), itensProcessadosAno, valorTotalProcessadoAno);

        var despesaInserida = 0;
        var despesasParaInserir = new List<DeputadoFederalDespesaTemp>();
        foreach (var despesaGroup in results)
        {
            DeputadoFederalDespesaTemp despesa = despesaGroup.FirstOrDefault();
            despesa.ValorDocumento = despesaGroup.Sum(x => x.ValorDocumento);
            despesa.ValorGlosa = despesaGroup.Sum(x => x.ValorGlosa);
            despesa.ValorLiquido = despesaGroup.Sum(x => x.ValorLiquido);
            despesa.Restituicao = despesaGroup.Sum(x => x.Restituicao);

            var str = JsonSerializer.Serialize(despesa, options);
            var hash = Utils.SHA1Hash(str);
            var key = Convert.ToHexString(hash);

            if (lstHash.Remove(key)) continue;

            despesa.Hash = hash;

            despesasParaInserir.Add(despesa);
            despesaInserida++;
        }

        var bulkService = new BulkInsertService<DeputadoFederalDespesaTemp>();
        bulkService.BulkInsertNoTracking(dbContext, despesasParaInserir);

        
        

        

        if (despesaInserida > 0)
            logger.LogInformation("{Itens} despesas inseridas na tabela temporaria.", despesaInserida);
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

    private void ProcessarDespesasTemp(int ano)
    {
        CorrigeDespesas();
        InsereDeputadoFaltante();
        InserePassageiroFaltante();
        InsereTrechoViagemFaltante();
        
        
        InsereMandatoFaltante();
        InsereLegislaturaFaltante();
        InsereFornecedorFaltante();
        InsereDespesaFinal(ano);
        LimpaDespesaTemporaria();
    }

    public virtual void ValidaImportacao(int ano)
    {
        logger.LogDebug("Validar importação");

        var sql = @$"
    select 
        count(1) as itens, 
        coalesce(sum(valor_liquido), 0) as valor_total 
    FROM camara.cf_despesa d 
    where d.ano = {ano}";

        int itensTotalFinal = default;
        decimal somaTotalFinal = default;
        using (IDataReader reader = connection.ExecuteReader(sql))
        {
            if (reader.Read())
            {
                itensTotalFinal = Convert.ToInt32(reader["itens"].ToString());
                somaTotalFinal = Convert.ToDecimal(reader["valor_total"].ToString());
            }
        }

        if (itensProcessadosAno != itensTotalFinal)
            logger.LogError("Totais divergentes! Arquivo: [Itens: {LinhasArquivo}; Valor: {ValorTotalArquivo}] DB: [Itens: {LinhasDB}; Valor: {ValorTotalFinal}]",
                itensProcessadosAno, valorTotalProcessadoAno, itensTotalFinal, somaTotalFinal);

        var despesasSemParlamentar = connection.ExecuteScalar<int>(@$"
    select count(1) from temp.cf_despesa_temp where id_deputado not in (select id FROM camara.cf_deputado);");

        if (despesasSemParlamentar > 0)
            logger.LogError("Há deputados não identificados!");
    }

    private void CorrigeDespesas()
    {
        connection.Execute(@"
UPDATE temp.cf_despesa_temp SET numero = NULL WHERE numero = 'S/N' OR numero = '';
UPDATE temp.cf_despesa_temp SET numero_deputado_id = NULL WHERE numero_deputado_id = 0;
UPDATE temp.cf_despesa_temp SET cnpj_cpf = NULL WHERE cnpj_cpf = '';

UPDATE temp.cf_despesa_temp SET sigla_uf = NULL WHERE sigla_uf = 'NA';
UPDATE temp.cf_despesa_temp SET numero_especificacao_sub_cota = NULL WHERE numero_especificacao_sub_cota = 0;
UPDATE temp.cf_despesa_temp SET lote = NULL WHERE lote = 0;
UPDATE temp.cf_despesa_temp SET ressarcimento = NULL WHERE ressarcimento = 0;
UPDATE temp.cf_despesa_temp SET id_documento = NULL WHERE id_documento = '0';
UPDATE temp.cf_despesa_temp SET parcela = NULL WHERE parcela = 0;
UPDATE temp.cf_despesa_temp SET sigla_partido = 'PP' WHERE sigla_partido = 'PP**';
UPDATE temp.cf_despesa_temp SET sigla_partido = null WHERE sigla_partido = 'NA';
    	");
    }

    public void InsereDeputadoFaltante()
    {
        
        var affected = connection.Execute(@"
UPDATE camara.cf_deputado d
SET id_deputado = dt.numero_deputado_id
FROM temp.cf_despesa_temp dt
WHERE d.id = dt.id_deputado
AND d.id_deputado IS NULL;
        ");

        if (affected > 0)
        {
            
            

            logger.LogInformation("{Itens} parlamentares atualizados!", affected);
        }

        
        affected = connection.Execute(@"
INSERT INTO camara.cf_deputado (id, id_deputado, nome_parlamentar, cpf)
SELECT id_deputado, numero_deputado_id, nome_parlamentar, max(cpf)
FROM temp.cf_despesa_temp
WHERE numero_deputado_id  not in (
   	SELECT id_deputado 
 			FROM camara.cf_deputado
   	WHERE id_deputado IS NOT null
)
AND numero_deputado_id IS NOT null
GROUP BY id_deputado, numero_deputado_id, nome_parlamentar;
        ");

        

        if (affected > 0)
        {
            
            

            logger.LogInformation("{Itens} parlamentares incluidos!", affected);
        }
    }

    private string InserePassageiroFaltante()
    {
        var affected = connection.Execute(@"
            INSERT INTO pessoa (nome)
            SELECT DISTINCT passageiro
            FROM temp.cf_despesa_temp
            WHERE coalesce(passageiro, '') <> ''
            AND passageiro not in (
                SELECT nome FROM pessoa
            );
        ");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} passageiros incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereTrechoViagemFaltante()
    {
        var affected = connection.Execute(@"
            INSERT INTO trecho_viagem (descricao)
            SELECT DISTINCT trecho
            FROM temp.cf_despesa_temp
            WHERE coalesce(trecho, '') <> ''
            AND trecho not in (
            	SELECT descricao FROM trecho_viagem
            );
        ");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} trechos viagem incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereTipoDespesaFaltante()
    {
        var affected = connection.Execute(@"
            INSERT INTO cf_despesa_tipo (id, descricao)
            select distinct numero_sub_cota, descricao
            from temp.cf_despesa_temp
            where numero_sub_cota  not in (
            	select id FROM camara.cf_despesa_tipo
            );
        ");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} tipos de despesa incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereTipoEspecificacaoFaltante()
    {
        var affected = connection.Execute(@"
            INSERT INTO cf_especificacao_tipo (id_cf_despesa_tipo, id_cf_especificacao, descricao)
            select distinct numero_sub_cota, numero_especificacao_sub_cota, ""descricaoEspecificacao""
            from temp.cf_despesa_temp dt
            left join cf_especificacao_tipo tp on tp.id_cf_despesa_tipo = dt.numero_sub_cota
                and tp.id_cf_especificacao = dt.numero_especificacao_sub_cota
            where numero_especificacao_sub_cota <> 0
            AND tp.descricao = null;
        ");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} tipo especificação incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereMandatoFaltante()
    {
        var affected = connection.Execute(@"
UPDATE camara.cf_mandato m
SET id_carteira_parlamantar = dt.numero_carteira_parlamentar
FROM (
    SELECT DISTINCT 
        numero_deputado_id, legislatura, numero_carteira_parlamentar, codigo_legislatura, sigla_uf, sigla_partido
    FROM temp.cf_despesa_temp
) dt
INNER JOIN camara.cf_deputado d ON d.id_deputado = dt.numero_deputado_id
LEFT JOIN estado e ON e.sigla = dt.sigla_uf
LEFT JOIN partido p ON p.sigla = dt.sigla_partido
WHERE m.id_cf_deputado = d.id
AND m.id_legislatura = dt.codigo_legislatura
AND dt.codigo_legislatura <> 0
AND m.id_carteira_parlamantar IS NULL;
    	");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} mandatos atualizados!", affected);
        }

        
        affected = connection.Execute(@"
INSERT INTO camara.cf_mandato (id_cf_deputado, id_legislatura, id_carteira_parlamantar, id_estado, id_partido)
SELECT DISTINCT d.id, dt.codigo_legislatura, dt.numero_carteira_parlamentar, e.id, p.id 
FROM ( 
    SELECT DISTINCT 
        numero_deputado_id, legislatura, numero_carteira_parlamentar, codigo_legislatura, sigla_uf, sigla_partido
    FROM temp.cf_despesa_temp
) dt
join camara.cf_deputado d on d.id_deputado = dt.numero_deputado_id
LEFT JOIN estado e ON e.sigla = dt.sigla_uf
LEFT JOIN partido p ON p.sigla = dt.sigla_partido
LEFT JOIN camara.cf_mandato m ON m.id_cf_deputado = d.id
    AND m.id_legislatura = dt.codigo_legislatura 
WHERE dt.codigo_legislatura <> 0
AND m.id IS NULL
ON CONFLICT DO NOTHING
    	");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} mandatos incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereLegislaturaFaltante()
    {
        var affected = connection.Execute(@"
    		INSERT INTO camara.cf_legislatura (id, ano)
    		select distinct codigo_legislatura, legislatura
    		from temp.cf_despesa_temp dt
    		where codigo_legislatura <> 0
    		AND codigo_legislatura  not in (
    			select id FROM camara.cf_legislatura
    		);
    	");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} legislatura incluidos!", affected);
        }

        return string.Empty;
    }

    public void InsereFornecedorFaltante()
    {
        ResolverFornecedor();

        var affected = connection.Execute(@"
    		INSERT INTO fornecedor.fornecedor (nome, cnpj_cpf)
    		select MAX(dt.fornecedor), dt.cnpj_cpf
    		from temp.cf_despesa_temp dt
    		where dt.id_fornecedor is null
            and dt.cnpj_cpf is not null
    		GROUP BY dt.cnpj_cpf;
    	");

        if (affected > 0)
            ResolverFornecedorPreExistente();


        List<string> despesasSemFornecedor = dbContext.DeputadoFederalDespesaTemps
            .AsNoTracking()
            .Where(x => x.IdFornecedor == null)
            .Select(x => x.Fornecedor)
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
UPDATE temp.cf_despesa_temp dt
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
UPDATE temp.cf_despesa_temp dt
set id_fornecedor = COALESCE(f.id_fornecedor_correto, f.id_fornecedor_incorreto)
FROM temp.fornecedor_de_para f 
WHERE dt.id_fornecedor IS NULL
AND f.cnpj_incorreto = dt.cnpj_cpf
AND unaccent(lower(f.nome)) = unaccent(lower(dt.fornecedor));

-- Por CNPJ (Pode ser parcial ou invalido)
UPDATE temp.cf_despesa_temp dt
set id_fornecedor = COALESCE(f.id_fornecedor_correto, f.id_fornecedor_incorreto)
FROM temp.fornecedor_de_para f 
WHERE dt.id_fornecedor IS NULL
AND f.cnpj_incorreto = dt.cnpj_cpf;

-- Por Nome exato (accent-insensitive matching)
UPDATE temp.cf_despesa_temp dt
set id_fornecedor = COALESCE(f.id_fornecedor_correto, f.id_fornecedor_incorreto), cnpj_cpf = COALESCE(dt.cnpj_cpf, f.cnpj_correto)
FROM temp.fornecedor_de_para f 
WHERE dt.id_fornecedor IS NULL
AND unaccent(lower(f.nome)) = unaccent(lower(dt.fornecedor));
");
    }

    public void InsereDespesaFinal(int ano)
    {
        var affected = connection.Execute(@$"
INSERT INTO camara.cf_despesa (
    ano,
    mes,
    id_documento,
    id_cf_deputado,
    id_cf_legislatura,
    id_cf_mandato,
    id_cf_despesa_tipo,
    id_cf_especificacao,
    id_fornecedor,
    id_passageiro,
    numero_documento,
    tipo_documento,
    data_emissao,
    valor_documento,
    valor_glosa,
    valor_liquido,
    valor_restituicao,
    id_trecho_viagem,
    tipo_link,
    ano_mes,
    hash
)
SELECT 
    dt.ano,
    dt.mes,
    CAST( dt.id_documento as INT4),
    d.id,
    dt.codigo_legislatura,
    m.id,
    numero_sub_cota,
    numero_especificacao_sub_cota,
    dt.id_fornecedor,
    p.id,
    numero,
    CAST(tipo_documento as SMALLINT),
    data_emissao,
    valor_documento,
    valor_glosa,
    valor_liquido,
    restituicao,
    tv.id,
    CAST(coalesce(url_documento, '0') as SMALLINT),
    concat(ano::text, LPAD(mes::text, 2, '0'))::INT8,
    dt.hash
from temp.cf_despesa_temp dt
LEFT JOIN camara.cf_deputado d on d.id_deputado = dt.numero_deputado_id
LEFT JOIN pessoa p on p.nome = dt.passageiro
LEFT JOIN trecho_viagem tv on tv.descricao = dt.trecho
left join camara.cf_mandato m on m.id_cf_deputado = d.id
    and m.id_legislatura = dt.codigo_legislatura
    and m.id_carteira_parlamantar = numero_carteira_parlamentar;
    	", 3600);

        var totalTemp = connection.ExecuteScalar<int>("select count(1) from temp.cf_despesa_temp");
        if (affected != totalTemp)
            logger.LogWarning("{Itens} despesas incluidas. Há {Qtd} despesas que foram ignoradas!", affected, totalTemp - affected);
        else if (affected > 0)
            logger.LogInformation("{Itens} despesas incluidas!", affected);
    }

    private void InsereDespesaLegislatura()
    {
        
        
        
        
        
        

        
        
        
        

        
        
        
        
        
        
        
        
    }

    public void LimpaDespesaTemporaria()
    {
        connection.Execute(@"truncate table temp.cf_despesa_temp;");
    }

    #endregion Importaçao Dados CEAP CSV


    #region Processar Resumo CEAP

    public void AtualizaParlamentarValoresCEAP()
    {
        try
        {
            
            connection.Execute(@"
UPDATE camara.cf_deputado d 
SET valor_total_ceap = COALESCE(subquery.total_valor, 0)
FROM (
    SELECT des.id_cf_deputado, 
           CASE 
               WHEN SUM(des.valor_liquido) < 0 THEN 0
               ELSE SUM(des.valor_liquido)
           END as total_valor
    FROM camara.cf_despesa des
    GROUP BY des.id_cf_deputado
) subquery
WHERE d.id = subquery.id_cf_deputado");
        }
        catch (Exception ex)
        {
            if (!ex.GetBaseException().Message.Contains("Out of range"))
                throw;
        }
    }

    
    
    
    
    public void AtualizaCampeoesGastos()
    {
        connection.Execute($@"
TRUNCATE TABLE camara.cf_deputado_campeao_gasto;

INSERT INTO camara.cf_deputado_campeao_gasto
SELECT l1.id_cf_deputado, d.nome_parlamentar, l1.valor_total, p.sigla, e.sigla
FROM (
    SELECT 
        l.id_cf_deputado,
        sum(l.valor_liquido) as valor_total
    FROM camara.cf_despesa l
    WHERE l.ano_mes >= {LEGISLATURA_ANO_MES} 
    GROUP BY l.id_cf_deputado
    ORDER BY valor_total desc 
    limit 4
) l1 
INNER JOIN camara.cf_deputado d on d.id = l1.id_cf_deputado 
LEFT JOIN partido p on p.id = d.id_partido
LEFT JOIN estado e on e.id = d.id_estado");
    }

    public void AtualizaResumoMensal()
    {
        connection.Execute(@"
TRUNCATE TABLE camara.cf_despesa_resumo_mensal;

INSERT INTO camara.cf_despesa_resumo_mensal (ano, mes, valor) 
SELECT ano, mes, sum(valor_liquido)
FROM camara.cf_despesa
GROUP BY ano, mes");
    }

    #endregion Processar Resumo CEAP


    
    
    
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    

    
    
    
    

    
    
    
    
    

    
    
    
    

    
    
    
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    
    
    
    
    
    
    

    

    
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    
    

    
    
    

    
    
    
    
    
    
























    
    
    
    
    
    



    
    
    
    
    
    
    
    
    
    
    
    

    
    
    
    
    

    
    
    
    
    

    
    
    
    
    
    


    
    
    

    
    

    
    
    
    
    
    

    
    
    
    
    
    
    
    
    

    
    

    
    

    
    
    
    
    
    

    

    private Dictionary<string, string> colunasBanco = new Dictionary<string, string>();
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    
    
    
    
    
    

    
    
    
    
    
    

    
    
    
    
    
    

    
    

    
    
    
    

    
    
    

    
    
    
    

    static readonly object padlock = new object();
    
    
    
    

    
    
    
    
    
    
    
    
    

    
    
    
    
    
    
    
    

    
    

    
    
    
    
    
    
    

    
    
    
    
    
    
    
    
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    

    
    
    
    
    
    
    
    

    
    
    

    
    
    
    
    
    
    
    
    
    

    
    
    
    
    
    


    
    
    
    
    
    

    
    
    
    
    
    

    
    
    
    
    
    
    

    
    
    
    

    
    
    
    
    
    

    
    
    
    
    
    
    
    
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    

    
    
    
    
    
    
    
    
    
    
    

    
    
    
    
    
    
    
    

    
    
    
    
    
    
    

    
    
    

    
    
    
    
    
    

    
    

    
    
    
    
    

    
    
    
    
    

    
    

    
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    

    
    
    

    
    

    
    
    
    
    
    
    
    

    
    
    

    
    
    

    
    
    
    

    
    
    
    
    

    
    

    
    

    
    
    
    
    
    
    
    

    

    
    
    
    
    
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    
    
    
    
    
    

    
    
    
    
    
    
    
    
    
    

    
    
    
    
    

    
    
    
    
    
    
    
    
    

    
    
    
    
    

    
    
    

    
    
    
    
    
    
    
    
    
    

    
    
    
    
    
    
    
    
    

    
    
    
    

    
    
    
    
    

    

    
    
    

    
    

    
    
    
    
    
    
    

    
    
    
    
    

    
    
    
    

    
    
    
    

    
    
    
    

    
    
    
    

    
    
    
    
    
    
    
    

    
    
    
    
    
    
    






    
    

    
    
    
    
    
    
    



    
    
    

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    

    public void AtualizaParlamentarValores()
    {
        connection.Execute(@"
UPDATE camara.cf_deputado d
SET 
  valor_total_salario = COALESCE(s.salario, 0),
	valor_total_auxilio_moradia = COALESCE(s.auxilio, 0),
	valor_total_remuneracao = COALESCE(s.verba, s.rem_func, 0)
FROM (
	SELECT 
		d.id,
		(SELECT SUM(valor) FROM camara.cf_deputado_remuneracao WHERE id_cf_deputado = d.id) as salario,
		(SELECT SUM(valor) FROM camara.cf_deputado_auxilio_moradia WHERE id_cf_deputado = d.id) as auxilio,
		(SELECT SUM(valor) FROM camara.cf_deputado_verba_gabinete WHERE id_cf_deputado = d.id) as verba,
		(SELECT SUM(valor_total) FROM camara.cf_funcionario_remuneracao WHERE id_cf_deputado = d.id) as rem_func
	FROM camara.cf_deputado d
) s
WHERE d.id = s.id;");
    }
}
