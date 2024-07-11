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

        public string tempPath { get; set; }

        public int idEstado { get { return config.Estado.GetHashCode(); } }

        private int linhasProcessadasAno { get; set; }

        protected Dictionary<string, List<uint>> lstHash { get; private set; }

        public HttpClient httpClient { get; }

        public List<CamaraEstadualDespesaTemp> despesasTemp { get; set; }

        public ImportadorDespesasBase(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetService<ILogger<ImportadorDespesasBase>>();
            connection = serviceProvider.GetService<IDbConnection>();

            var configuration = serviceProvider.GetService<IConfiguration>();
            rootPath = configuration["AppSettings:SiteRootFolder"];
            tempPath = configuration["AppSettings:SiteTempFolder"];


            httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient("MyNamedClient");
            despesasTemp = new List<CamaraEstadualDespesaTemp>();
        }

        public virtual void DownloadFotosParlamentares()
        {
            logger.LogWarning("Sem Importação de Imagens");
        }

        public virtual void ProcessarDespesas(int ano)
        {
            InsereDespesasTemp();
            AjustarDados();
            InsereTipoDespesaFaltante();
            InsereDeputadoFaltante();
            InsereFornecedorFaltante();
            DeletarDespesasInexistentes();

            InsereDespesaFinal(ano);

            ValidaImportacao(ano);
            LimpaDespesaTemporaria();

            if (ano == DateTime.Now.Year)
            {
                AtualizaValorTotal();
            }
        }

        private void InsereDespesasTemp()
        {
            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };

            List<IGrouping<string, CamaraEstadualDespesaTemp>> results = despesasTemp.GroupBy(x => Convert.ToHexString(x.Hash)).ToList();
            linhasProcessadasAno = results.Count();

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

                if (lstHash.ContainsKey(key))
                {
                    // Para hashs com mais de um registro associado, manter na lista para deletar e inserir novamente.
                    // TODO: Como tratar tabela com hashs duplicadas
                    if (lstHash[key].Count == 1 && lstHash.Remove(key))
                        continue;
                }

                despesa.Hash = hash;
                connection.Insert(despesa);
            }
        }

        public virtual void ImportarRemuneracao(int ano, int mes)
        {
            logger.LogWarning("Sem Importação de Remuneração");
        }

        public virtual void AtualizaParlamentarValores() { }

        public virtual void AtualizaCampeoesGastos() { }

        public virtual void AtualizaResumoMensal() { }

        public virtual Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano) { return null; }

        protected bool TentarBaixarArquivo(string urlOrigem, string caminhoArquivo)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                return BaixarArquivo(urlOrigem, caminhoArquivo);
            }
            catch (Exception ex)
            {
                logger.LogWarning("Erro ao baixar arquivo: {Message}", ex.Message);

                // Algumas vezes ocorre do arquivo não estar disponivel, precisamos aguardar alguns instantes e tentar novamente.
                // Isso pode ser causado por um erro de rede ou atualização do arquivo.
                Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);

                return BaixarArquivo(urlOrigem, caminhoArquivo);
            }
            finally
            {
                watch.Stop();
                logger.LogTrace("Arquivo baixado em {TimeElapsed:c}", watch.Elapsed);
            }
        }

        private bool BaixarArquivo(string urlOrigem, string caminhoArquivo)
        {
            logger.LogTrace("Baixando arquivo '{CaminhoArquivo}' a partir de '{UrlOrigem}'", caminhoArquivo, urlOrigem);

            string diretorio = new FileInfo(caminhoArquivo).Directory.ToString();
            if (!Directory.Exists(diretorio))
                Directory.CreateDirectory(diretorio);
            else if (File.Exists(caminhoArquivo))
                return true;
            //File.Delete(caminhoArquivo);

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
            else // Nome
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
and d.ano_mes BETWEEN {ano}01 and {ano}12
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
##WHERE##
ORDER BY d.id;
			";

            if (config.Estado != Estado.Piaui && config.Estado != Estado.RioDeJaneiro)
            {
                sql = sql.Replace("##WHERE##", $@"
WHERE d.hash NOT IN (
    SELECT hash FROM cl_despesa d
    inner join cl_deputado p on p.id = d.id_cl_deputado and id_estado = {idEstado}
    WHERE d.ano_mes between '{ano}01' and '{ano}12'
)");
            }
            else
            {
                sql = sql.Replace("##WHERE##", "");
            }
            var affected = connection.Execute(sql, 3600);


            if (affected > 0)
            {
                logger.LogInformation("{Itens} despesas incluidas!", affected);
            }

        }

        public virtual void ValidaImportacao(int ano)
        {
            logger.LogTrace("Validar importação");

            int totalFinal = ContarRegistrosBaseDeDadosFinal(ano);
            int totalTemp = ContarRegistrosBaseDeDadosTemp(ano);

            if (linhasProcessadasAno != totalFinal)
                logger.LogError("Totais divergentes! Arquivo: {LinhasArquivo} Temp: {totalTemp} DB: {LinhasDB}", linhasProcessadasAno, totalFinal);
            else
            {
                logger.LogInformation("Itens na base de dados: {LinhasDB}", totalFinal);

                string condicaoSql = "";
                if (config.ChaveImportacao == ChaveDespesaTemp.Cpf)
                    condicaoSql = "p.cpf = d.cpf";
                else if (config.ChaveImportacao == ChaveDespesaTemp.CpfParcial)
                    condicaoSql = "p.cpf_parcial = d.cpf";
                else if (config.ChaveImportacao == ChaveDespesaTemp.Matricula)
                    condicaoSql = "p.matricula = d.cpf";
                else // ChaveDespesaTemp.Nome
                    condicaoSql = "(IFNULL(p.nome_importacao, p.nome_parlamentar) like d.nome or p.nome_civil like d.nome)";

                var despesasSemParlamentar = connection.ExecuteScalar<int>(@$"
SELECT COUNT(1)
FROM ops_tmp.cl_despesa_temp d
left join cl_deputado p on {condicaoSql} and id_estado = {idEstado}
WHERE p.id IS null");

                if (despesasSemParlamentar > 0)
                    logger.LogError("Há deputados não identificados!"); // TODO: Não pode verificar apenas por nome
            }
        }

        public virtual int ContarRegistrosBaseDeDadosFinal(int ano)
        {
            return connection.ExecuteScalar<int>(@$"
select count(1) 
from cl_despesa d 
join cl_deputado p on p.id = d.id_cl_deputado 
where p.id_estado = {idEstado}
and d.ano_mes between {ano}01 and {ano}12");
        }

        public virtual int ContarRegistrosBaseDeDadosTemp(int ano)
        {
            return connection.ExecuteScalar<int>(@$"
select count(1) from ops_tmp.cl_despesa_temp");
        }

        public virtual void LimpaDespesaTemporaria()
        {
            connection.Execute("truncate table ops_tmp.cl_despesa_temp");
            despesasTemp = new List<CamaraEstadualDespesaTemp>();
        }

        public virtual void DeletarDespesasInexistentes()
        {
            logger.LogTrace("Sincroniza Hashes");

            if (!config.Completo && lstHash.Values.Any())
            {
                var itensDbCount = connection.ExecuteScalar<uint>("SELECT count(1) FROM ops_tmp.cl_despesa_temp");
                if (itensDbCount == 0 && lstHash.Count() > itensDbCount)
                    throw new Exception($"Tentando remover ({lstHash.Count()}) mais itens que estamos incluindo ({itensDbCount})! Verifique a importação.");

                var deleted = 0;
                foreach (var dc in lstHash)
                {
                    var dcCount = dc.Value.Count;
                    if (dcCount > 1)
                    {
                        var hash = Convert.FromHexString(dc.Key);
                        var hashDbCount = connection.ExecuteScalar<uint>("SELECT count(1) FROM ops_tmp.cl_despesa_temp where hash = @hash", new { hash });
                        if (dcCount == hashDbCount)
                        {
                            connection.ExecuteScalar<uint>("DELETE FROM ops_tmp.cl_despesa_temp where hash = @hash", new { hash });
                            continue;
                        }
                    }

                    //var despesa = connection.Get<CamaraEstadualDespesa>(id);
                    //logger.LogTrace("Remover Registro {@CamaraEstadualDespesa}", despesa);
                    connection.Execute($"delete from cl_despesa where id IN({string.Join(",", dc.Value)})");
                    deleted += dcCount;
                }

                logger.LogInformation("{TotalDeleted} despesas removidas!", deleted);
            }
        }

        public virtual void CarregarHashes(int ano)
        {
            linhasProcessadasAno = 0;

            lstHash = new Dictionary<string, List<uint>>();
            var sql = SqlCarregarHashes(ano);
            IEnumerable<dynamic> lstHashDB = connection.Query(sql);

            int despesasCount = 0;
            foreach (IDictionary<string, object> dReader in lstHashDB)
            {
                despesasCount++;
                var hex = Convert.ToHexString((byte[])dReader["hash"]);
                if (!lstHash.ContainsKey(hex))
                    lstHash.Add(hex, new List<uint>() { (uint)dReader["id"] });
                else
                    lstHash[hex].Add((uint)dReader["id"]);
            }

            logger.LogTrace("{Total} Hashes Carregados", despesasCount);
        }

        public virtual string SqlCarregarHashes(int ano)
        {
            return $"select d.id, d.hash from cl_despesa d join cl_deputado p on d.id_cl_deputado = p.id where p.id_estado = {idEstado} and d.ano_mes between {ano}01 and {ano}12";
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
            // Para armazenamento na base de dados a hash é gerado com o valor, para que mudançãs no total provoquem uma atualização.
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
