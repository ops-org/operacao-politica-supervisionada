using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using Dapper;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;

namespace OPS.Importador.ALE.Despesa
{

    public abstract class ImportadorDespesasBase
    {
        public ImportadorCotaParlamentarBaseConfig config { get; set; }

        public ILogger<ImportadorDespesasBase> logger { get; set; }

        public IDbConnection connection { get; set; }

        public string rootPath { get; set; }

        public string tempPath { get; set; }

        public int idEstado { get; set; }

        private int linhasProcessadasAno { get; set; }

        private Dictionary<string, uint> lstHash { get; set; }

        public ImportadorDespesasBase(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetService<ILogger<ImportadorDespesasBase>>();
            connection = serviceProvider.GetService<IDbConnection>();

            var configuration = serviceProvider.GetService<IConfiguration>();
            rootPath = configuration["AppSettings:SiteRootFolder"];
            tempPath = configuration["AppSettings:SiteTempFolder"];
        }

        public virtual void DownloadFotosParlamentares()
        {
            logger.LogWarning("Sem Importação de Imagens");
        }

        public virtual void ProcessarDespesas(int ano)
        {
            AjustarDados();
            InsereTipoDespesaFaltante();
            InsereDeputadoFaltante();
            InsereFornecedorFaltante();

            InsereDespesaFinal(ano);
            DeletarDespesasInexistentes();

            ValidaImportacao(ano);
            LimpaDespesaTemporaria();

            if (ano == DateTime.Now.Year)
            {
                AtualizaValorTotal();
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
                logger.LogTrace("Arquivo baixado em {ElapsedTotalSeconds} s", watch.Elapsed.TotalSeconds);
            }
        }

        private bool BaixarArquivo(string urlOrigem, string caminhoArquivo)
        {
            logger.LogTrace("Baixando arquivo '{CaminhoArquivo}' a partir de '{UrlOrigem}'", caminhoArquivo, urlOrigem);

            string diretorio = new FileInfo(caminhoArquivo).Directory.ToString();
            if (!Directory.Exists(diretorio))
            {
                Directory.CreateDirectory(diretorio);

                if (File.Exists(caminhoArquivo))
                {
                    File.Delete(caminhoArquivo);
                }

                using (HttpClient client = new())
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(Utils.DefaultUserAgent);
                    client.Timeout = TimeSpan.FromMinutes(5);
                    client.DownloadFile(urlOrigem, caminhoArquivo).Wait();
                }

                return true;
            }

            return false;
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
            if (config.ChaveImportacaoDeputado == ChaveDespesaTemp.Nome)
            {
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, cpf_parcial, id_estado)
select distinct Nome, cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where nome not in (
    select nome_parlamentar 
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND nome_parlamentar IS NOT null
);");
            }
            else if (config.ChaveImportacaoDeputado == ChaveDespesaTemp.CpfParcial)
            {
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, cpf_parcial, id_estado)
select distinct Nome, cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where cpf not in (
    select cpf_parcial 
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND cpf_parcial IS NOT NULL
);");
            }
            else if (config.ChaveImportacaoDeputado == ChaveDespesaTemp.Matricula)
            {
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, matricula, id_estado)
select distinct Nome, cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where cpf not in (
    select matricula 
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND matricula IS NOT NULL
);");
            }
            else // CPF
            {
                affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, cpf, id_estado)
select distinct Nome, cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where cpf not in (
    select cpf 
    FROM cl_deputado 
    WHERE id_estado = {idEstado} 
    AND CPF IS NOT NULL
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
            else // ChaveDespesaTemp.Nome
                condicaoSql = "(p.nome_parlamentar like d.nome or p.nome_civil like d.nome)";

            var affected = connection.Execute(@$"
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
    CASE WHEN f.id IS NULL THEN d.empresa else null END AS observacao,
    d.hash
FROM ops_tmp.cl_despesa_temp d
inner join cl_deputado p on id_estado = {idEstado} and {condicaoSql}
left join cl_despesa_especificacao dts on dts.descricao = d.despesa_tipo
LEFT join fornecedor f on f.cnpj_cpf = d.cnpj_cpf
WHERE d.hash NOT IN (
    SELECT hash FROM cl_despesa d
    inner join cl_deputado p on p.id = d.id_cl_deputado and id_estado = {idEstado}
    WHERE d.ano_mes between '{ano}01' and '{ano}12'
)
ORDER BY d.id;
			", 3600);


            if (affected > 0)
            {
                logger.LogInformation("{Itens} despesas incluidas!", affected);
            }

        }

        public virtual void ValidaImportacao(int ano)
        {
            logger.LogTrace("Validar importação");

            var totalFinal = connection.ExecuteScalar<int>(@$"
select count(1) 
from cl_despesa d 
join cl_deputado p on p.id = d.id_cl_deputado 
where p.id_estado = {idEstado}
and d.ano_mes between {ano}01 and {ano}12");

            if (linhasProcessadasAno != totalFinal)
                logger.LogError("Totais divergentes! Arquivo: {LinhasArquivo} DB: {LinhasDB}",
                    linhasProcessadasAno, totalFinal);

            var despesasSemParlamentar = connection.ExecuteScalar<int>(@$"
SELECT COUNT(1)
FROM ops_tmp.cl_despesa_temp d
left join cl_deputado p on (p.nome_parlamentar like d.nome or p.nome_civil like d.nome) and id_estado = {idEstado}
WHERE p.id IS null");

            if (despesasSemParlamentar > 0)
                logger.LogError("Há deputados não identificados!");
        }

        public virtual void LimpaDespesaTemporaria()
        {

            connection.Execute("truncate table ops_tmp.cl_despesa_temp");
        }

        public virtual void DeletarDespesasInexistentes()
        {
            logger.LogTrace("Sincroniza Hashes");

            if (!config.Completo && lstHash.Values.Any())
            {
                foreach (var id in lstHash.Values)
                {
                    var despesa = connection.Get<CamaraEstadualDespesa>(id);
                    //logger.LogTrace("Remover Registro {@CamaraEstadualDespesa}", despesa);
                    connection.Execute("delete from cf_despesa where id=@id", new { id });
                }

                logger.LogInformation("{Total} despesas removidas!", lstHash.Values.Count);
            }
        }

        public virtual void CarregarHashes(int ano)
        {
            lstHash = new Dictionary<string, uint>();
            var sql = $"select d.id, d.hash from cl_despesa d join cl_deputado p on d.id_cl_deputado = p.id where p.id_estado = {idEstado} and d.ano_mes between {ano}01 and {ano}12";
            var lstHashDB = connection.Query(sql);

            int hashIgnorado = 0;
            foreach (IDictionary<string, object> dReader in lstHashDB)
            {
                var hex = Convert.ToHexString((byte[])dReader["hash"]);
                if (!lstHash.ContainsKey(hex))
                    lstHash.Add(hex, (uint)dReader["id"]);
                else
                    hashIgnorado++;

            }

            logger.LogTrace("{Total} Hashes Carregados", lstHash.Count);

            if (hashIgnorado > 0)
                logger.LogWarning("{Total} Hashes Ignorados", hashIgnorado);
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

        public void InserirDespesaTemp(CamaraEstadualDespesaTemp despesa)
        {
            linhasProcessadasAno++;

            var str = JsonSerializer.Serialize(despesa);
            var hash = Utils.SHA1Hash(str);
            if (lstHash.Remove(Convert.ToHexString(hash)))
                return;

            despesa.Hash = hash;
            connection.Insert(despesa);
        }
    }

    public class ImportadorCotaParlamentarBaseConfig
    {
        public string BaseAddress { get; set; }

        public bool Completo { get; private set; } = false;

        public Estado Estado { get; set; }

        public ChaveDespesaTemp ChaveImportacaoDeputado { get; private set; } = ChaveDespesaTemp.Indefinido;

        public ChaveDespesaTemp ChaveImportacao { get; set; } = ChaveDespesaTemp.Indefinido;
    }
}
