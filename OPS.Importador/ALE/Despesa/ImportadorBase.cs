using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE.Despesa
{
    public abstract class ImportadorBase
    {
        public ILogger<ImportadorBase> logger { get; set; }

        protected IImportadorParlamentar importadorParlamentar { get; set; }

        protected IImportadorDespesas importadorDespesas { get; set; }

        public ImportadorBase(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetService<ILogger<ImportadorBase>>();
        }

        public virtual void ImportarCompleto()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                watch.Restart();

                //logger.LogWarning("Dados do(a) {CasaLegislativa}", config.SiglaEstado);
                if (importadorParlamentar != null)
                    importadorParlamentar.Importar().Wait();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            finally
            {
                watch.Stop();
                logger.LogTrace("Processamento em {TimeElapsed:c}", watch.Elapsed);
            }

            //try
            //{
            //    watch.Restart();

            //    if (importadorParlamentar != null)
            //        importadorParlamentar.DownloadFotos().Wait();
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, ex.Message);
            //}
            //finally
            //{
            //    watch.Stop();
            //    logger.LogTrace("Processamento em {TimeElapsed:c}", watch.Elapsed);
            //}

            try
            {
                watch.Restart();

                if (importadorDespesas != null)
                    importadorDespesas.Importar(DateTime.Now.Year - 1);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            finally
            {
                watch.Stop();
                logger.LogTrace("Processamento em {TimeElapsed:c}", watch.Elapsed);
            }

            try
            {
                watch.Restart();

                if (importadorDespesas != null)
                    importadorDespesas.Importar(DateTime.Now.Year);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
            finally
            {
                watch.Stop();
                logger.LogTrace("Processamento em {TimeElapsed:c}", watch.Elapsed);
            }

            //try
            //{
            //    watch.Restart();

            //    var anoAtual = DateTime.Now;
            //    logger.LogInformation("Remuneração do(a) {CasaLegislativa}", config.SiglaEstado);
            //    ImportarRemuneracao(anoAtual.Year, anoAtual.Month);
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, ex.Message);
            //}
            //finally
            //{
            //    watch.Stop();
            //    logger.LogTrace("Processamento em {TimeElapsed:c}", watch.Elapsed);
            //}
        }
    }
}
