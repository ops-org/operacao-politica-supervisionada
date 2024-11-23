using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using RestSharp;

namespace OPS.Importador.ALE.Despesa
{

    public abstract class ImportadorDespesasBase
    {
        public ImportadorCotaParlamentarBaseConfig config { get; set; }

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

        protected Dictionary<string, uint> lstHash { get; private set; }

        private HttpClient _httpClient;
        public HttpClient httpClient { get { return _httpClient ??= httpClientFactory.CreateClient("ResilientClient"); } }

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

            logger.LogInformation("Processando {Itens} despesas agrupadas em {Unicos} unicas com valor total de {ValorTotal} para {Estado} em {Ano}.", despesasTemp.Count(), itensProcessadosAno, valorTotalProcessadoAno, config.Estado.ToString(), ano);

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

        public virtual void AtualizaCampeoesGastos() { }

        public virtual void AtualizaResumoMensal() { }

        public virtual Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano) { return null; }

        protected bool BaixarArquivo(string urlOrigem, string caminhoArquivo)
        {
            if (importacaoIncremental && File.Exists(caminhoArquivo)) return false;
            logger.LogTrace($"Baixando arquivo '{caminhoArquivo}' a partir de '{urlOrigem}'");

            string diretorio = new FileInfo(caminhoArquivo).Directory.ToString();
            if (!Directory.Exists(diretorio))
                Directory.CreateDirectory(diretorio);
            else if (File.Exists(caminhoArquivo))
                File.Delete(caminhoArquivo);

            httpClient.DownloadFile(urlOrigem, caminhoArquivo).Wait();
            return true;
        }

        public virtual void AtualizaValorTotal()
        {
            logger.LogTrace("Atualizando valores totais");

            connection.Execute(@$"
UPDATE cl_deputado dp 
SET valor_total_ceap = IFNULL((
        SELECT SUM(ds.valor_liquido) FROM cl_despesa ds WHERE ds.id_cl_deputado = dp.id
    ), 0)
WHERE id_estado = {idEstado};");
        }

        public virtual void AjustarDados()
        { }

        public virtual void InsereTipoDespesaFaltante()
        {
            logger.LogTrace("Inserir despesa faltante");

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
            logger.LogTrace("Inserir parlamentar faltante");

            int affected = 0;
            if (config.ChaveImportacao == ChaveDespesaTemp.Cpf)
            {
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, nome_civil, cpf, id_estado)
select distinct Nome, Nome, cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where cpf not in (
    select cpf 
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND CPF IS NOT NULL
);");
            }
            else if (config.ChaveImportacao == ChaveDespesaTemp.CpfParcial)
            {
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, nome_civil, cpf_parcial, id_estado)
select distinct Nome, Nome, cpf, {idEstado}
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
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, nome_civil, matricula, id_estado)
select distinct Nome, Nome, cpf, {idEstado}
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
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, nome_civil, gabinete, id_estado)
select distinct Nome, Nome, cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where cpf not in (
    select gabinete 
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND gabinete IS NOT NULL
);");
            }
            else if (config.ChaveImportacao == ChaveDespesaTemp.NomeCivil)
            {
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, nome_civil, cpf_parcial, id_estado)
select distinct Nome, Nome, cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where nome not in (
    select IFNULL(nome_importacao, nome_civil)
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND nome_civil IS NOT null
);");
            }
            else if (config.ChaveImportacao == ChaveDespesaTemp.NomeParlamentar)
            {
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, nome_civil, cpf_parcial, id_estado)
select distinct Nome, Nome, cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where nome not in (
    select IFNULL(nome_importacao, nome_parlamentar)
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND nome_parlamentar IS NOT null
);");
            }
            else
            {
                var deputadosNaoLocalizados = connection.ExecuteScalar<string>(@$"
SELECT GROUP_CONCAT(DISTINCT nome)
FROM ops_tmp.cl_despesa_temp
WHERE nome NOT IN (
    SELECT IFNULL(nome_importacao, nome_parlamentar)
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND nome_parlamentar IS NOT null
);");

                if(!string.IsNullOrEmpty(deputadosNaoLocalizados))
                {
                    throw new Exception($"Deputados não cadastrados: {deputadosNaoLocalizados}");
                }
            }

            if (affected > 0)
            {
                logger.LogInformation("{Itens} parlamentares incluidos!", affected);
            }
        }

        public virtual void InsereFornecedorFaltante()
        {
            logger.LogTrace("Inserir fornecedor faltante");

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
            logger.LogTrace("Inserir despesa final");

            string condicaoSql = "";
            if (config.ChaveImportacao == ChaveDespesaTemp.Cpf)
                condicaoSql = "p.cpf = d.cpf";
            else if (config.ChaveImportacao == ChaveDespesaTemp.CpfParcial)
                condicaoSql = "p.cpf_parcial = d.cpf";
            else if (config.ChaveImportacao == ChaveDespesaTemp.Matricula)
                condicaoSql = "p.matricula = d.cpf";
            else if (config.ChaveImportacao == ChaveDespesaTemp.Gabinete)
                condicaoSql = "p.gabinete = d.cpf";
            else // ChaveDespesaTemp.Nome
                condicaoSql = "(IFNULL(p.nome_importacao, p.nome_parlamentar) like d.nome or p.nome_civil like d.nome)";

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
    concat(IFNULL(d.ano, year(d.data_emissao)), LPAD(IFNULL(d.mes, month(d.data_emissao)), 2, '0')) AS ano_mes,
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

            var totalTemp = connection.ExecuteScalar<int>("select count(1) from ops_tmp.cl_despesa_temp");
            if (affected != totalTemp)
                logger.LogWarning("{Itens} despesas incluidas. Há {Qtd} despesas que foram ignoradas!", affected, totalTemp - affected);
            else if (affected > 0)
                logger.LogInformation("{Itens} despesas incluidas!", affected);
        }

        public virtual void ValidaImportacao(int ano)
        {
            logger.LogTrace("Validar importação");

            var agregador = RegistrosBaseDeDadosFinalAgregados(ano);
            int itensTotalFinal = agregador.Item1;
            decimal somaTotalFinal = agregador.Item2;

            if (itensProcessadosAno != itensTotalFinal || somaTotalFinal != valorTotalProcessadoAno)
            {
                logger.LogError("Totais divergentes! Arquivo: [Itens: {LinhasArquivo}; Valor: {ValorTotalArquivo}] DB: [Itens: {LinhasDB}; Valor: {ValorTotalFinal}]",
                    itensProcessadosAno, valorTotalProcessadoAno, itensTotalFinal, somaTotalFinal);

                string condicaoSql = "";
                if (config.ChaveImportacao == ChaveDespesaTemp.Cpf)
                    condicaoSql = "p.cpf = d.cpf";
                else if (config.ChaveImportacao == ChaveDespesaTemp.CpfParcial)
                    condicaoSql = "p.cpf_parcial = d.cpf";
                else if (config.ChaveImportacao == ChaveDespesaTemp.Matricula)
                    condicaoSql = "p.matricula = d.cpf";
                else if (config.ChaveImportacao == ChaveDespesaTemp.Gabinete)
                    condicaoSql = "p.gabinete = d.cpf";
                else // ChaveDespesaTemp.Nome
                    condicaoSql = "(IFNULL(p.nome_importacao, p.nome_parlamentar) like d.nome or p.nome_civil like d.nome)";

                var despesasSemParlamentar = connection.ExecuteScalar<int>(@$"
SELECT COUNT(1)
FROM ops_tmp.cl_despesa_temp d
left join cl_deputado p on {condicaoSql} and id_estado = {idEstado}
WHERE p.id IS null");

                if (despesasSemParlamentar > 0)
                    logger.LogError("Há deputados não identificados!");
            }
            else
            {
                logger.LogInformation("Itens na base de dados: {LinhasDB}; Valor total: R$ {ValorTotalFinal}", itensTotalFinal, somaTotalFinal);

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
    IFNULL(sum(valor_liquido), 0) as valor_total 
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
            logger.LogTrace("Sincroniza Hashes");

            var agregador = RegistrosBaseDeDadosFinalAgregados(ano);
            int itensTotalFinal = agregador.Item1;
            decimal somaTotalFinal = agregador.Item2;

            logger.LogInformation("Já existem {Itens} despesas com valor total de {ValorTotal} para {Estado} em {Ano} previamente importados.", itensTotalFinal, somaTotalFinal, config.Estado.ToString(), ano);

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
        //    //logger.LogTrace("Novo Registro {@CamaraEstadualDespesa}", despesa);
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
            // Zerar o valor para ignora-lo (somente aqui) para agrupar os itens iguals e com valores diferentes.
            // Para armazenamento na base de dados a hash é gerado com o valor, para que mudanças no total provoquem uma atualização.
            var valorTemp = despesaTemp.Valor;
            despesaTemp.Valor = default(decimal);
            var str = JsonSerializer.Serialize(despesaTemp);
            var hash = Utils.SHA1Hash(str);
            var key = Convert.ToHexString(hash);

            despesaTemp.Hash = hash;
            despesaTemp.Valor = valorTemp;
            despesasTemp.Add(despesaTemp);
        }

        public T RestApiGet<T>(string address)
        {
            var restClient = new RestClient(httpClient);

            var request = new RestRequest(address);
            request.AddHeader("Accept", "application/json");

            RestResponse resParlamentares = restClient.GetWithAutoRetry(request);
            return JsonSerializer.Deserialize<T>(resParlamentares.Content);
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
