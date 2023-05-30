using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OPS.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Threading;
using Dapper;
using System.Net.Http;
using System.Linq;
using OPS.Core.Entity;
using System.Text.Json;

namespace OPS.Importador
{
    public abstract class ImportadorCotaParlamentarBase
    {
        protected const string tempDatabase = "ops-temp";

        public string casaLegislativa { get; init; }

        public ILogger<ImportadorCotaParlamentarBase> logger { get; init; }

        public IConfiguration configuration { get; init; }

        public readonly IDbConnection connection;

        public bool completo { get; init; } = false;

        public string rootPath { get; init; }

        public string tempPath { get; init; }

        public int idEstado { get; init; }

        //public Dictionary<string, string> arquivos = new();

        public ImportadorCotaParlamentarBase(string siglaEstado, ILogger<ImportadorCotaParlamentarBase> logger, IConfiguration configuration, IDbConnection connection)
        {
            this.casaLegislativa = siglaEstado;
            this.logger = logger;
            this.configuration = configuration;
            this.connection = connection;

            rootPath = configuration["AppSettings:SiteRootFolder"];
            tempPath = configuration["AppSettings:SiteTempFolder"];

            this.idEstado = connection.ExecuteScalar<int>("select id from estado where sigla=@sigla", new { sigla = siglaEstado });
        }

        public virtual void ImportarCompleto()
        {
            //try
            //{
            //    logger.LogInformation("Dados do(a) {CasaLegislativa}", casaLegislativa);
            //    ImportarParlamentares();
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, ex.Message);
            //}

            //try
            //{
            //    logger.LogInformation("Imagens do(a) {CasaLegislativa}", casaLegislativa);
            //    DownloadFotosParlamentares();
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, ex.Message);
            //}

            //try
            //{
            //    var anoAnterior = DateTime.Now.Year - 1;
            //    logger.LogInformation("Despesas do(a) {CasaLegislativa} de {Ano}", casaLegislativa, anoAnterior);
            //    ImportarArquivoDespesas(anoAnterior);
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, ex.Message);
            //}

            try
            {
                var anoAtual = DateTime.Now.Year;
                logger.LogInformation("Despesas do(a) {CasaLegislativa} de {Ano}", casaLegislativa, anoAtual);
                ImportarArquivoDespesas(anoAtual);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

            //try
            //{
            //    var anoAtual = DateTime.Now;
            //    logger.LogInformation("Remuneração do(a) {CasaLegislativa}", casaLegislativa);
            //    ImportarRemuneracao(anoAtual.Year, anoAtual.Month);
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, ex.Message);
            //}
        }

        public virtual void ImportarParlamentares() { }

        public virtual void DownloadFotosParlamentares() { }

        public virtual void ImportarArquivoDespesas(int ano)
        {
            Dictionary<string, string> arquivos = DefinirOrigemDestino(ano);

            foreach (var arquivo in arquivos)
            {
                var _urlOrigem = arquivo.Key;
                var _caminhoArquivo = arquivo.Value;

                if (TentarBaixarArquivo(_urlOrigem, _caminhoArquivo))
                {
                    try
                    {
                        ProcessarDespesas(_caminhoArquivo, ano);
                    }
                    catch (Exception ex)
                    {

                        logger.LogError(ex, ex.Message);

#if !DEBUG
                        //Excluir o arquivo para tentar importar novamente na proxima execução
                        if(File.Exists(_caminhoArquivo))
                            File.Delete(_caminhoArquivo);
#endif

                    }
                }
            }


        }

        protected virtual void ProcessarDespesas(string caminhoArquivo, int ano) { }

        public virtual void ImportarRemuneracao(int ano, int mes) { }

        public virtual void AtualizaParlamentarValores() { }

        public virtual void AtualizaCampeoesGastos() { }

        public virtual void AtualizaResumoMensal() { }

        public virtual Dictionary<string, string> DefinirOrigemDestino(int ano) { return null; }

        protected bool TentarBaixarArquivo(string urlOrigem, string caminhoArquivo)
        {
            try
            {
                return BaixarArquivo(urlOrigem, caminhoArquivo);
            }
            catch (Exception)
            {
                // Algumas vezes ocorre do arquivo não estar disponivel, precisamos aguardar alguns instantes e tentar novamente.
                // Isso pode ser causado por um erro de rede ou atualização do arquivo.
                Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);

                return BaixarArquivo(urlOrigem, caminhoArquivo);
            }
        }

        private bool BaixarArquivo(string urlOrigem, string caminhoArquivo)
        {
#if DEBUG
            if (File.Exists(caminhoArquivo)) return completo;
#endif

            string diretorio = (new FileInfo(caminhoArquivo)).Directory.ToString();
            if (!Directory.Exists(diretorio))
                Directory.CreateDirectory(diretorio);

            var request = (HttpWebRequest)WebRequest.Create(urlOrigem);

            request.UserAgent = "Other";
            request.Method = "HEAD";
            request.ContentType = "application/json;charset=UTF-8";
            request.Timeout = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;

            using (var resp = request.GetResponse())
            {
                if (File.Exists(caminhoArquivo))
                {
                    var ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
                    if (ContentLength > 0)
                    {
                        long ContentLengthLocal = 0;

                        if (File.Exists(caminhoArquivo))
                            ContentLengthLocal = new FileInfo(caminhoArquivo).Length;

                        if (ContentLength == ContentLengthLocal)
                        {
                            logger.LogInformation("Sem alterações desde a última importação!");
                            return completo;
                        }
                    }
                }

                if (!File.Exists(caminhoArquivo))
                {
                    using (HttpClient client = new())
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(Utils.DefaultUserAgent);
                        client.DownloadFile(urlOrigem, caminhoArquivo).Wait();
                    }

                    return true;
                }
            }

            if (!completo)
                logger.LogInformation("Sem alterações desde a última importação!");

            return completo;
        }

        protected void DescompactarArquivo(string caminhoArquivo)
        {
            //using (ZipFile file = new ZipFile(_caminhoArquivo))
            //{
            //    if (file.TestArchive(true) == false)
            //        throw new Exception("Erro no Zip da Câmara");
            //}
            var zip = new FastZip();
            zip.ExtractZip(caminhoArquivo, tempPath, null);
        }

        public virtual void AtualizaValorTotal()
        {
            connection.Execute(@$"
UPDATE cl_deputado dp 
SET valor_total_ceap = IFNULL((
        SELECT SUM(ds.valor_liquido) FROM cl_despesa ds WHERE ds.id_cl_deputado = dp.id
    ), 0)
WHERE id_estado = {idEstado};");
        }

        public virtual void InsereTipoDespesaFaltante()
        {
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
            var affected = connection.Execute(@$"
INSERT INTO cl_deputado (nome_parlamentar, matricula, id_estado)
select distinct Nome, cpf, {idEstado}
from ops_tmp.cl_despesa_temp
where cpf not in (
    select cpf from cl_deputado where id_estado = {idEstado}
);
                ");

            if (affected > 0)
            {
                logger.LogInformation("{Itens} parlamentares incluidos!", affected);
            }
        }

        public virtual void InsereFornecedorFaltante()
        {
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

        public virtual void InsereDespesaFinal()
        {
            var affected = connection.Execute(@$"
INSERT INTO cl_despesa (
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
    concat(year(d.data_emissao), LPAD(month(d.data_emissao), 2, '0')) AS ano_mes,
    d.documento AS numero_documento,
    d.valor AS valor,
    CASE WHEN f.id IS NULL THEN d.empresa else null END AS observacao,
    d.hash
FROM ops_tmp.cl_despesa_temp d
inner join cl_deputado p on id_estado = {idEstado} and (p.cpf = d.cpf OR p.matricula = d.cpf)
left join cl_despesa_especificacao dts on dts.descricao = d.despesa_tipo
LEFT join fornecedor f on f.cnpj_cpf = d.cnpj_cpf
ORDER BY d.id;
			", 3600);


            if (affected > 0)
            {
                logger.LogInformation("{Itens} despesas incluidas!", affected);
            }

        }

        public virtual void LimpaDespesaTemporaria()
        {
            connection.Execute(@"
				truncate table ops_tmp.cl_despesa_temp;
			");
        }

        public virtual void SincronizarHashes(Dictionary<string, uint> dc)
        {
            if (!completo && dc.Values.Any())
            {
                logger.LogInformation("{Total} despesas removidas!", dc.Values.Count);

                foreach (var id in dc.Values)
                {
                    connection.Execute("delete from cf_despesa where id=@id", new { id });
                }
            }
        }

        public virtual Dictionary<string, uint> ObterHashes(int ano)
        {
            var dc = new Dictionary<string, UInt32>();
            var sql = $"select d.id, d.hash from cl_despesa d join cl_deputado p on d.id_cl_deputado = p.id where p.id_estado = {idEstado} and d.ano_mes between {ano}01 and {ano}12";
            var lstHash = connection.Query(sql);
            foreach (IDictionary<string, object> dReader in lstHash)
            {
                var hex = Convert.ToHexString((byte[])dReader["hash"]);
                if (!dc.ContainsKey(hex))
                    dc.Add(hex, (UInt32)dReader["id"]);
            }

            return dc;
        }

        /// <summary>
        /// Registro já existe na base de dados?
        /// </summary>
        /// <param name="deputado"></param>
        /// <param name="lstHash"></param>
        /// <returns></returns>
        public virtual bool RegistroExistente(CamaraEstadualDespesaTemp deputado, Dictionary<string, UInt32> lstHash)
        {
            var str = JsonSerializer.Serialize(deputado);
            var hash = Utils.SHA1Hash(str);

            if (lstHash.Remove(Convert.ToHexString(hash)))
                return true;

            deputado.Hash = hash;
            return false;
        }
    }
}
