using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OPS.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OPS.Importador
{
    public abstract class ImportadorCotaParlamentarBase
    {
        public string casaLegislativa { get; init; }

        public ILogger<ImportadorCotaParlamentarBase> logger { get; init; }

        public IConfiguration configuration { get; init; }

        public bool completo { get; init; }

        public string rootPath { get; init; }

        public string tempPath { get; init; }


        public Dictionary<string, string> arquivos = new();

        public ImportadorCotaParlamentarBase(string casaLegislativa, ILogger<ImportadorCotaParlamentarBase> logger, IConfiguration configuration)
        {
            this.casaLegislativa = casaLegislativa;
            this.logger = logger;
            this.configuration = configuration;
            this.completo = false;

            rootPath = configuration["AppSettings:SiteRootFolder"];
            tempPath = configuration["AppSettings:SiteTempFolder"];
        }

        public virtual void ImportarCompleto()
        {
            //try
            //{
            //    logger.LogInformation("Dados dos {CasaLegislativa}", casaLegislativa);
            //    ImportarParlamentares();
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, ex.Message);
            //}

            try
            {
                logger.LogInformation("Imagens dos {CasaLegislativa}", casaLegislativa);
                DownloadFotosParlamentares();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

            try
            {
                arquivos.Clear();

                var anoAnterior = DateTime.Now.Year - 1;
                logger.LogInformation("Despesas dos {CasaLegislativa} de {Ano}", casaLegislativa, anoAnterior);
                ImportarArquivoDespesas(anoAnterior);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

            try
            {
                arquivos.Clear();

                var anoAtual = DateTime.Now.Year;
                logger.LogInformation("Despesas dos {CasaLegislativa} de {Ano}", casaLegislativa, anoAtual);
                ImportarArquivoDespesas(anoAtual);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }


            try
            {
                var anoAtual = DateTime.Now;
                logger.LogInformation("Remuneração na {CasaLegislativa}", casaLegislativa);
                ImportarRemuneracao(anoAtual.Year, anoAtual.Month);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
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

        protected abstract void ProcessarDespesas(string caminhoArquivo, int ano);

        public virtual void ImportarRemuneracao(int ano, int mes) { }

        public virtual void AtualizaParlamentarValores(AppDb banco) { }

        public virtual void AtualizaCampeoesGastos(AppDb banco) { }

        public virtual void AtualizaResumoMensal(AppDb banco) { }

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
            string diretorio = (new FileInfo(caminhoArquivo)).Directory.ToString();
            if (!Directory.Exists(diretorio))
                Directory.CreateDirectory(diretorio);

            var request = (HttpWebRequest)WebRequest.Create(urlOrigem);

            request.UserAgent = "Other";
            request.Method = "HEAD";
            request.ContentType = "application/json;charset=UTF-8";
            request.Timeout = 1000000;

            using (var resp = request.GetResponse())
            {
                if (File.Exists(caminhoArquivo))
                {
                    var ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
                    long ContentLengthLocal = 0;

                    if (File.Exists(caminhoArquivo))
                        ContentLengthLocal = new FileInfo(caminhoArquivo).Length;

                    if (!completo && ContentLength == ContentLengthLocal)
                    {
                        logger.LogInformation("Sem alterações desde a última importação!");
                        return false;
                    }
                }

                if (!File.Exists(caminhoArquivo))
                {
                    using (var client = new WebClient())
                    {
                        client.Headers.Add("User-Agent: Other");
                        client.DownloadFile(urlOrigem, caminhoArquivo);
                        return true;
                    }
                }
            }

            logger.LogInformation("Sem alterações desde a última importação!");
            return false;
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
    }
}
