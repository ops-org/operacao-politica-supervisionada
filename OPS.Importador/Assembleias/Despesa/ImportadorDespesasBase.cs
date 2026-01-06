using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Utilities;
using RestSharp;

namespace OPS.Importador.Assembleias.Despesa
{

    public abstract class ImportadorDespesasBase
    {
        public ImportadorCotaParlamentarBaseConfig config { get; set; }

        public ArquivoChecksum arquivoChecksum { get; set; }

        public ILogger<ImportadorDespesasBase> logger { get; set; }

        public IDbConnection connection { get; set; }

        public string rootPath { get; set; }

        private string _tempPath { get; set; }
        public string tempPath
        {
            get
            {
                var dir = Path.Combine(_tempPath, config.Estado.ToString());

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                return dir;
            }
        }

        public string competenciaInicial { get; set; }

        public string competenciaFinal { get; set; }

        public int idEstado { get { return config.Estado.GetHashCode(); } }

        public bool importacaoIncremental { get; set; }

        private int itensProcessadosAno { get; set; }

        private decimal valorTotalProcessadoAno { get; set; }

        /// <summary>
        /// Controle de lote para agrupar os registros por fonte (arquivo/endpoint)
        /// </summary>
        protected int lote { get; set; }

        protected Dictionary<string, uint> lstHash { get; private set; }

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

        public ImportadorDespesasBase(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetService<ILogger<ImportadorDespesasBase>>();
            connection = serviceProvider.GetService<IDbConnection>();

            var configuration = serviceProvider.GetService<IConfiguration>();
            rootPath = configuration["AppSettings:SiteRootFolder"];
            _tempPath = configuration["AppSettings:SiteTempFolder"];
            importacaoIncremental = Convert.ToBoolean(configuration["AppSettings:ImportacaoDespesas:Incremental"] ?? "false");


            httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
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
                    catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.LockDeadlock)
                    {
                        AtualizaValorTotal();
                    }
                }

                logger.LogInformation("Finalizando processamento na base de dados!");
            }
            finally
            {
                Monitor.Exit(_monitorObj);
            }
        }

        private void InsereDespesasTemp(int ano)
        {
            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };

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
                //        Console.WriteLine(JsonSerializer.Serialize(item, options));
                //    }
                //}

                CamaraEstadualDespesaTemp despesa = despesaGroup.FirstOrDefault();
                despesa.Valor = despesaGroup.Sum(x => x.Valor);

                var str = JsonSerializer.Serialize(despesa, options);
                var hash = Utils.SHA1Hash(str);
                var key = Convert.ToHexString(hash);

                if (lstHash.Remove(key)) continue;

                despesa.Hash = hash;
                connection.Insert(despesa);
                despesaInserida++;
            }

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

        public virtual Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano) { return null; }

        protected bool BaixarArquivo(string urlOrigem, string caminhoArquivo)
        {
            var caminhoArquivoDb = caminhoArquivo.Replace(_tempPath, "");
            arquivoChecksum = connection.GetList<ArquivoChecksum>(new { nome = caminhoArquivoDb }).FirstOrDefault();

            if (importacaoIncremental && File.Exists(caminhoArquivo))
            {
                var arquivoDB = connection.GetList<ArquivoChecksum>(new { nome = caminhoArquivoDb }).FirstOrDefault();
                if (arquivoDB != null && arquivoDB.Verificacao > DateTime.UtcNow.AddDays(-7))
                {
                    logger.LogWarning("Ignorando arquivo verificado recentemente '{CaminhoArquivo}' a partir de '{UrlOrigem}'", caminhoArquivo, urlOrigem);
                    return false;
                }
            }

            logger.LogDebug("Baixando arquivo '{CaminhoArquivo}' a partir de '{UrlOrigem}'", caminhoArquivo, urlOrigem);

            string diretorio = new FileInfo(caminhoArquivo).Directory.ToString();
            if (!Directory.Exists(diretorio))
                Directory.CreateDirectory(diretorio);

            var fileExt = Path.GetExtension(caminhoArquivo);
            var caminhoArquivoTmp = caminhoArquivo.Replace(fileExt, $"_tmp{fileExt}");
            if (File.Exists(caminhoArquivoTmp))
                File.Delete(caminhoArquivoTmp);

            if (config.Estado != Estado.DistritoFederal)
                httpClientResilient.DownloadFile(urlOrigem, caminhoArquivoTmp).Wait();
            else
                httpClientDefault.DownloadFile(urlOrigem, caminhoArquivoTmp).Wait();

            string checksum = ChecksumCalculator.ComputeFileChecksum(caminhoArquivoTmp);

            Monitor.Enter(_monitorObj);
            try
            {
                if (arquivoChecksum != null && arquivoChecksum.Checksum == checksum && File.Exists(caminhoArquivo))
                {
                    arquivoChecksum.Verificacao = DateTime.UtcNow;
                    connection.Update(arquivoChecksum);

                    logger.LogDebug("Arquivo '{CaminhoArquivo}' é identico ao já existente.", caminhoArquivo);

                    if (File.Exists(caminhoArquivoTmp))
                        File.Delete(caminhoArquivoTmp);

                    return false;
                }

                if (arquivoChecksum == null)
                {
                    logger.LogDebug("Arquivo '{CaminhoArquivo}' é novo.", caminhoArquivo);

                    arquivoChecksum = new ArquivoChecksum()
                    {
                        Nome = caminhoArquivoDb,
                        Checksum = checksum,
                        TamanhoBytes = (uint)new FileInfo(caminhoArquivoTmp).Length,
                        Criacao = DateTime.UtcNow,
                        Atualizacao = DateTime.UtcNow,
                        Verificacao = DateTime.UtcNow
                    };
                    arquivoChecksum.Id = (uint)connection.Insert(arquivoChecksum);
                }
                else
                {
                    if (arquivoChecksum.Checksum != checksum)
                    {
                        logger.LogDebug("Arquivo '{CaminhoArquivo}' foi atualizado.", caminhoArquivo);

                        arquivoChecksum.Checksum = checksum;
                        arquivoChecksum.TamanhoBytes = (uint)new FileInfo(caminhoArquivoTmp).Length;
                        arquivoChecksum.Revisado = false;
                    }

                    arquivoChecksum.Atualizacao = DateTime.UtcNow;
                    arquivoChecksum.Verificacao = DateTime.UtcNow;
                    connection.Update(arquivoChecksum);
                }
            }
            finally
            {

                Monitor.Exit(_monitorObj);
            }

            File.Move(caminhoArquivoTmp, caminhoArquivo, true);
            return true;
        }

        public virtual void AtualizaValorTotal()
        {
            logger.LogDebug("Atualizando valores totais");

            connection.Execute(@$"
UPDATE cl_deputado dp 
SET valor_total_ceap = coalesce((
        SELECT SUM(ds.valor_liquido) FROM cl_despesa ds WHERE ds.id_cl_deputado = dp.id
    ), 0)
WHERE id_estado = {idEstado};");
        }

        private void AjustarDadosGlobais()
        {
            // Substituir nome importação pelo utilizado na importação. Dessa forma podemos ter 2 nomes validos e padronizamos a importação de validação.
            if (config.ChaveImportacao == ChaveDespesaTemp.NomeCivil)
            {
                connection.Execute($@"
UPDATE ops_tmp.cl_despesa_temp temp
JOIN cl_deputado d ON d.nome_importacao = temp.nome_civil
SET temp.nome_civil = d.nome_civil
WHERE d.id_estado = {idEstado}
AND temp.nome_civil is not null
");
            }
            else if (config.ChaveImportacao == ChaveDespesaTemp.NomeParlamentar)
            {
                connection.Execute($@"
UPDATE ops_tmp.cl_despesa_temp temp
JOIN cl_deputado d ON d.nome_importacao = temp.nome
SET temp.nome = d.nome_parlamentar
WHERE d.id_estado = {idEstado}
AND d.nome_parlamentar IS NOT null
");
            }
        }

        public virtual void AjustarDados()
        { }

        public virtual void InsereTipoDespesaFaltante()
        {
            logger.LogDebug("Inserir despesa faltante");

            var affected = connection.Execute(@"
INSERT INTO cl_despesa_especificacao (descricao)
select distinct despesa_tipo
from ops_tmp.cl_despesa_temp
where despesa_tipo is not null
and despesa_tipo not in (
    select descricao from cl_despesa_especificacao
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
INSERT INTO cl_deputado (nome_parlamentar, nome_civil, cpf, id_estado)
select distinct nome, coalesce(nome_civil, nome), cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where cpf not in (
    select cpf 
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND cpf IS NOT NULL
);");
            }
            else if (config.ChaveImportacao == ChaveDespesaTemp.CpfParcial)
            {
                chaveImportacao = "cpf_parcial";
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, nome_civil, cpf_parcial, id_estado)
select distinct nome, coalesce(nome_civil, nome), cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where cpf not in (
    select cpf_parcial 
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND cpf_parcial IS NOT NULL
);");
            }
            else if (config.ChaveImportacao == ChaveDespesaTemp.Matricula)
            {
                chaveImportacao = "matricula";
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, nome_civil, matricula, id_estado)
select distinct nome, coalesce(nome_civil, nome), cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where cpf not in (
    select matricula 
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND matricula IS NOT NULL
);");
            }
            else if (config.ChaveImportacao == ChaveDespesaTemp.Gabinete)
            {
                chaveImportacao = "gabinete";
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, nome_civil, gabinete, id_estado)
select distinct nome, coalesce(nome_civil, nome), cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where cpf not in (
    select gabinete 
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND gabinete IS NOT NULL
);");
            }
            else if (config.ChaveImportacao != ChaveDespesaTemp.IdDeputado)
            {
                connection.Execute(@$"
UPDATE ops_tmp.cl_despesa_temp t
JOIN ops_tmp.cl_deputado_de_para d ON coalesce(t.nome, t.nome_civil) LIKE d.nome AND d.id_estado = {idEstado}
SET t.id_cl_deputado = d.id");

                if (config.ChaveImportacao == ChaveDespesaTemp.NomeCivil)
                {
                    chaveImportacao = "nome_civil";
                    affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, nome_civil, cpf, id_estado)
select distinct nome, coalesce(nome_civil, nome), cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where id_cl_deputado is null
and nome_civil not in (
    select nome_civil
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND nome_civil IS NOT null
);");
                }
                else if (config.ChaveImportacao == ChaveDespesaTemp.NomeParlamentar)
                {
                    chaveImportacao = "nome_parlamentar";
                    affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, nome_civil, cpf, id_estado)
select distinct nome, coalesce(nome_civil, nome), cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where id_cl_deputado is null
and nome not in (
    select nome_parlamentar
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND nome_parlamentar IS NOT null
);");
                }

                if (affected > 0)
                {
                    connection.Execute(@$"
INSERT IGNORE INTO ops_tmp.cl_deputado_de_para (id, nome, id_estado)
SELECT id, {chaveImportacao}, id_estado FROM ops.cl_deputado
WHERE {chaveImportacao} IS NOT NULL");

                    connection.Execute(@$"
UPDATE ops_tmp.cl_despesa_temp t
JOIN ops_tmp.cl_deputado_de_para d ON coalesce(t.nome, t.nome_civil) LIKE d.nome AND d.id_estado = {idEstado}
SET t.id_cl_deputado = d.id");
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
SELECT GROUP_CONCAT(DISTINCT nome)
FROM ops_tmp.cl_despesa_temp
WHERE id_cl_deputado is null;";
            }
            else
            {
                sqlDeputadosNaoLocalizados = @$"
SELECT GROUP_CONCAT(DISTINCT nome)
FROM ops_tmp.cl_despesa_temp
WHERE cpf NOT IN (
    SELECT {chaveImportacao}
    FROM cl_deputado 
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

            var affected = connection.Execute(@"
INSERT INTO fornecedor (nome, cnpj_cpf)
select MAX(dt.empresa), dt.cnpj_cpf
from ops_tmp.cl_despesa_temp dt
left join fornecedor f on f.cnpj_cpf = dt.cnpj_cpf
where dt.cnpj_cpf is not null
and f.id is null
-- and LENGTH(dt.cnpj_cpf) <= 14
GROUP BY dt.cnpj_cpf;
			    ");

            if (affected > 0)
            {
                logger.LogInformation("{Itens} fornecedores incluidos!", affected);
            }

        }

        public void RemoveDespesas(int ano)
        {
            var affected = connection.Execute(@$"
DELETE d
from cl_despesa d 
join cl_deputado p on p.id = d.id_cl_deputado 
where p.id_estado = {idEstado}
and d.ano_mes BETWEEN {competenciaInicial} and {competenciaFinal}
			");

        }

        public virtual void InsereDespesaFinal(int ano)
        {
            var totalTemp = connection.ExecuteScalar<int>("select count(1) from ops_tmp.cl_despesa_temp");
            if (totalTemp == 0) return;

            logger.LogDebug("Inserir despesa final");

            string condicaoSql = "";
            if (config.ChaveImportacao == ChaveDespesaTemp.Cpf)
                condicaoSql = "p.cpf = d.cpf";
            else if (config.ChaveImportacao == ChaveDespesaTemp.CpfParcial)
                condicaoSql = "p.cpf_parcial = d.cpf";
            else if (config.ChaveImportacao == ChaveDespesaTemp.Matricula)
                condicaoSql = "p.matricula = d.cpf";
            else if (config.ChaveImportacao == ChaveDespesaTemp.Gabinete)
                condicaoSql = "p.gabinete = d.cpf";
            else
            {
                condicaoSql = "p.id like d.id_cl_deputado";
            }

            var sql = @$"
INSERT IGNORE INTO cl_despesa (
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
    f.id AS id_fornecedor,
    d.data_emissao,
    concat(coalesce(d.ano, year(d.data_emissao)), LPAD(coalesce(d.mes, month(d.data_emissao)), 2, '0')) AS ano_mes,
    d.documento AS numero_documento,
    d.valor AS valor,
    CASE WHEN f.id IS NULL THEN d.empresa else null END AS favorecido,
    d.observacao,
    d.hash
FROM ops_tmp.cl_despesa_temp d
inner join cl_deputado p on id_estado = {idEstado} and {condicaoSql}
left join cl_despesa_especificacao dts on dts.descricao = d.despesa_tipo
LEFT join fornecedor f on f.cnpj_cpf = d.cnpj_cpf
ORDER BY d.id;
			";

            var affected = connection.Execute(sql, 3600);

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
                logger.LogError("Totais divergentes! Arquivo: [Itens: {LinhasArquivo}; Valor: {ValorTotalArquivo:#,###.00}] DB: [Itens: {LinhasDB}; Valor: R$ {ValorTotalFinal:#,###.00}]",
                    itensProcessadosAno, valorTotalProcessadoAno, itensTotalFinal, somaTotalFinal);

                var despesasSemParlamentar = connection.ExecuteScalar<int>(@$"
SELECT COUNT(1)
FROM ops_tmp.cl_despesa_temp d
WHERE d.id_cl_deputado IS NULL");

                if (despesasSemParlamentar > 0)
                    logger.LogError("Há deputados não identificados!");
            }
            else
            {
                logger.LogInformation("Itens na base de dados: {LinhasDB}; Valor total: R$ {ValorTotalFinal:#,###.00}", itensTotalFinal, somaTotalFinal);

            }

            if (ano == DateTime.UtcNow.Year)
            {
                var ultimaDataEmissao = connection.ExecuteScalar<DateTime>($@"
SELECT MAX(d.data_emissao) AS ultima_emissao
FROM cl_despesa d
inner join cl_deputado p ON p.id = d.id_cl_deputado AND p.id_estado = {idEstado}");

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
from cl_despesa d 
join cl_deputado p on p.id = d.id_cl_deputado 
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
            connection.Execute("truncate table ops_tmp.cl_despesa_temp");
        }

        public virtual void DeletarDespesasInexistentes(int ano)
        {
            logger.LogDebug("Sincroniza Hashes");

            var agregador = RegistrosBaseDeDadosFinalAgregados(ano);
            int itensTotalFinal = agregador.Item1;
            decimal somaTotalFinal = agregador.Item2;

            logger.LogInformation("Já existem {Itens} despesas com valor total de {ValorTotal:#,###.00} para {Estado} em {Ano} previamente importados.", itensTotalFinal, somaTotalFinal, config.Estado.ToString(), ano);

            if (!config.Completo && lstHash.Values.Any())
            {
                var itensDbCount = connection.ExecuteScalar<uint>("SELECT count(1) FROM ops_tmp.cl_despesa_temp");
                if (itensDbCount == 0 && lstHash.Count() > itensDbCount)
                {
                    logger.LogError("Tentando remover ({ItensArquivo}) mais itens que estamos incluindo ({ItensDB})! Verifique a importação.", lstHash.Count(), itensDbCount);
                    throw new BusinessException($"Tentando remover ({lstHash.Count()}) mais itens que estamos incluindo ({itensDbCount})! Verifique a importação.");
                }

                foreach (var dc in lstHash)
                {
                    connection.Execute($"delete from cl_despesa where id IN({dc.Value})");
                }

                logger.LogInformation("{TotalDeleted} despesas removidas!", lstHash.Count());
            }
        }

        public virtual void CarregarHashes(int ano)
        {
            DefinirCompetencias(ano);

            valorTotalProcessadoAno = 0;
            itensProcessadosAno = 0;

            lstHash = new Dictionary<string, uint>();
            var sql = $"select d.id, d.hash from cl_despesa d join cl_deputado p on d.id_cl_deputado = p.id where p.id_estado = {idEstado} and d.ano_mes between {competenciaInicial} and {competenciaFinal}";
            IEnumerable<dynamic> lstHashDB = connection.Query(sql);

            foreach (IDictionary<string, object> dReader in lstHashDB)
            {
                var hex = Convert.ToHexString((byte[])dReader["hash"]);
                if (!lstHash.ContainsKey(hex))
                    lstHash.Add(hex, (uint)dReader["id"]);
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
            return connection
                .GetList<DeputadoEstadual>(new
                {
                    id_estado = idEstado,
                    nome_parlamentar = nomeParlamentar
                })
                .FirstOrDefault()
                ?? new DeputadoEstadual()
                {
                    IdEstado = (ushort)idEstado,
                    NomeParlamentar = nomeParlamentar
                };
        }

        public DeputadoEstadual GetDeputadoByMatriculaOrNew(uint matricula)
        {
            return connection
                .GetList<DeputadoEstadual>(new
                {
                    id_estado = idEstado,
                    matricula
                })
                .FirstOrDefault()
                ?? new DeputadoEstadual()
                {
                    IdEstado = (ushort)idEstado,
                    Matricula = matricula
                };
        }

        public void InserirDespesaTemp(CamaraEstadualDespesaTemp despesaTemp)
        {
            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            };

            var dateToCompare = DateTime.Today;
            if (despesaTemp.Mes.HasValue) dateToCompare = (new DateTime(despesaTemp.Ano, despesaTemp.Mes.Value, 1)).AddMonths(1).AddDays(-1);

            if (despesaTemp.DataEmissao.Year != despesaTemp.Ano && despesaTemp.DataEmissao.AddMonths(3).Year != despesaTemp.Ano)
            {
                // Validar ano com 3 meses de tolerancia.
                //logger.LogWarning("Despesa com ano incorreto: {@Despesa}", despesaTemp);

                var dt = despesaTemp.DataEmissao;
                despesaTemp.DataEmissao = new DateTime(despesaTemp.Ano, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            }
            else if (despesaTemp.DataEmissao > dateToCompare)
            {
                // Validar despesa com data futura.
                //logger.LogWarning("Despesa com data incorreta/futura: {@Despesa}", despesaTemp);

                var dt = despesaTemp.DataEmissao;
                // Tentamos trocar apenas o ano.
                despesaTemp.DataEmissao = new DateTime(despesaTemp.Ano, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);

                // Caso a data permaneça invalida, alteramos também o mês, se possivel.
                if (despesaTemp.DataEmissao > dateToCompare && despesaTemp.Mes.HasValue)
                {
                    var monthLastDay = DateTime.DaysInMonth(despesaTemp.Ano, despesaTemp.Mes.Value);
                    despesaTemp.DataEmissao = new DateTime(despesaTemp.Ano, despesaTemp.Mes.Value, Math.Min(dt.Day, monthLastDay), dt.Hour, dt.Minute, dt.Second);
                }
            }

            // Zerar o valor para ignora-lo (somente aqui) para agrupar os itens iguals e com valores diferentes.
            // Para armazenamento na base de dados a hash é gerado com o valor, para que mudanças no total provoquem uma atualização.
            var valorTemp = despesaTemp.Valor;
            despesaTemp.Valor = default(decimal);
            var str = JsonSerializer.Serialize(despesaTemp, options);
            var hash = Utils.SHA1Hash(str);
            var key = Convert.ToHexString(hash);

            despesaTemp.Hash = hash;
            despesaTemp.Valor = valorTemp;
            despesasTemp.Add(despesaTemp);

            //Console.WriteLine(str);
            //Console.WriteLine("");
        }

        public T RestApiGet<T>(string address)
        {
            using RestClient client = CreateHttpClient();

            var request = new RestRequest(address);
            request.Timeout = TimeSpan.FromMinutes(5);
            request.AddHeader("Accept", "application/json");

            return client.Get<T>(request);
        }

        public T RestApiGetWithCustomDateConverter<T>(string address)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DateTimeOffsetConverterUsingDateTimeParse());

            var request = new RestRequest(address);
            request.AddHeader("Accept", "application/json");

            //if (config.Estado == Estado.Parana)
            //{
            //    request.AddHeader("recaptchaprivatetoken", "a5fbdde6daed041e187f0162c0f116faa22fcb93324099827c20b7cd61e06251b10db5956624fb548e5d69bfc13ac4c32fd5dd90e10393846faeb9c2a5e0ad02");
            //    request.AddHeader("recaptchapublictoken", TokenHelper.GerarTokenAngular("03AFcWeA6hm4oJfOh3MRN76AQtvHCIcOz9bq93ermGjGho_-g9s7XuV6sAbcFsiSGsg7vPfxwmTOTjdnBh6h-eRD9t6XLu4rX_BYiX2zm2aBNAkp4hjxP0uYxfbv622WrvvNEJxRvfEs7Oz2u2e4UQwwhUGE1AHy8JcxOE-Hqi_dYpD0efkh-dT9c5LKRazS-BOePcToEadiWYTndFcGxSYNrgtdRKjzj1JzkFvOD9HXJPeIoJDwMkVzIFxTqL-voQN69Y_CNKUus2MpstmovojIpvtkoqBvJ1A7R0Ic39ztePFkUsnDlbNYfJSqyclcP66PbKxrPzC4U9MH5O9fnyYbp6_wUg5E1RpzOdK6oV7JVLMxxFb-kY74hdelYyXU5qzyYiMyhUlHeqk8W_OgUmVXMzD7M0cZAQPDmgqupI-v6m5KQpG7zZnIY24cY_JCP5Vd0RqlSQ34I5wb8Wqmb67XAfFb0c3JTu_nZoYt8uYJTOBchbhGEOEQvC5IsBDRKe-QaZv27Ht3NeOq-4bSChQUuwWWEraH7QSYal7wHpHXj9nyboXsEzrfMGHvmWlmFZnMKXugNpYxruXPmet4bvb6VWlEMN4f5Z8x0OzsNtYvFKaiYJXvtZ8HvhROrtaEsLUCce7EkBvH_2n1C8YdPuVzDADRFObvVR4bRrb21haRNLN8Pai8N6xr6_CXdldzrP9bNiEqq0xr8BBogU0erx0z_ksHe9xOJTXvu-H2zSI94kiihbnMVsRF3BCGW_2OAzBfl6ba6AeCLiZN_vJSc3VZqtJviIC73sc-vNGBZo9JaSjORWMDpHFtoOhTjM2eM9uNQXBUJ5Jk7EhpoAtOVVSrMth9nS8Z6Vzb2G09XOo133xczomcdq9ZfbXX95VtVe84W0oxs6Oe_D0X0SybA-lvjcCH7pCmCrI-C8TKxDZLLHuPp0Kv55BRKBOLkSgSALJHc3hy_unEF1-dhac_SRC1ekBXRo1AloOg"));
            //}

            using RestClient client = CreateHttpClient();
            var response = client.Get(request);
            return JsonSerializer.Deserialize<T>(response.Content, options);

        }

        public T RestApiGetWithSqlTimestampConverter<T>(string address)
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new SqlTimestampConverter());

            var request = new RestRequest(address);
            request.AddHeader("Accept", "application/json");

            using RestClient client = CreateHttpClient();
            var response = client.Get(request);
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

        public virtual void ValidaValorTotal(decimal valorTotalArquivo, decimal valorTotalCalculado, int despesasIncluidas)
        {
            var diferenca = Math.Abs(valorTotalArquivo - valorTotalCalculado);

            if (arquivoChecksum != null)
            {
                arquivoChecksum.ValorTotal = valorTotalArquivo;
                arquivoChecksum.Divergencia = valorTotalArquivo - valorTotalCalculado;
                connection.Update(arquivoChecksum);
            }

            if (diferenca > 100)
            {
                var valores = despesasTemp.Where(x => x.Lote == lote).Select(x => x.Valor.ToString("F2")).ToList();
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

            lote++;
        }

        public void AtualizarDatasImportacaoDespesas(DateTime? dInicio = null, DateTime? dFim = null)
        {
            var importacao = connection.GetList<Importacao>(new { chave = config.Estado.ToString() }).FirstOrDefault();
            if (importacao == null)
            {
                importacao = new Importacao()
                {
                    Chave = config.Estado.ToString()
                };
                importacao.Id = (ushort)connection.Insert(importacao);
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
from cl_despesa d
join cl_deputado p ON p.id = d.id_cl_deputado
where p.id_estado = @idEstado";
                using (var dReader = connection.ExecuteReader(sql, new { idEstado }))
                {
                    if (dReader.Read())
                    {
                        importacao.PrimeiraDespesa = dReader["primeira_despesa"] != DBNull.Value ? Convert.ToDateTime(dReader["primeira_despesa"]) : (DateTime?)null;
                        importacao.UltimaDespesa = dReader["ultima_despesa"] != DBNull.Value ? Convert.ToDateTime(dReader["ultima_despesa"]) : (DateTime?)null;
                    }
                }
            }

            connection.Update(importacao);
        }

        /// <summary>
        /// Atualiza indicador 'Campeões de gastos',
        /// Os 4 deputados que mais gastaram com a CEAP desde o ínicio do mandato 55 (02/2015)
        /// </summary>
        public void AtualizaCampeoesGastos()
        {
            var strSql =
                @"truncate table cl_deputado_campeao_gasto;
    				insert into cl_deputado_campeao_gasto
    				SELECT l1.id_cl_deputado, d.nome_parlamentar, l1.valor_total, p.sigla, e.sigla
    				FROM (
    					SELECT 
    						l.id_cl_deputado,
    						sum(l.valor_liquido) as valor_total
    					FROM  cl_despesa l
    					GROUP BY l.id_cl_deputado
    					order by valor_total desc 
    					limit 4
    				) l1 
    				INNER JOIN cl_deputado d on d.id = l1.id_cl_deputado 
    				LEFT JOIN partido p on p.id = d.id_partido
    				LEFT JOIN estado e on e.id = d.id_estado;";

            connection.Execute(strSql);
        }

        public void AtualizaResumoMensal()
        {
            var strSql =
                @"truncate table cl_despesa_resumo_mensal;
    				insert into cl_despesa_resumo_mensal
    				(ano, mes, valor) (
    					select ano, mes, sum(valor_liquido)
    					from cl_despesa
    					group by ano, mes
    				);";

            connection.Execute(strSql);
        }
    }

    public class ImportadorCotaParlamentarBaseConfig
    {
        public string BaseAddress { get; set; }

        public bool Completo { get; private set; } = false;

        public Estado Estado { get; set; }

        //public ChaveDespesaTemp ChaveImportacaoDeputado { get; private set; } = ChaveDespesaTemp.Indefinido;

        public ChaveDespesaTemp ChaveImportacao { get; set; } = ChaveDespesaTemp.Indefinido;
    }
}
