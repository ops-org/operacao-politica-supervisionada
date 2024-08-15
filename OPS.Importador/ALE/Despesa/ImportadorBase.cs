using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Core;
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

            if (importadorParlamentar != null)
            {
                using (logger.BeginScope("Perfil do Parlamentar"))
                    try
                    {
                        watch.Restart();


                        importadorParlamentar.Importar().Wait();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                    finally
                    {
                        watch.Stop();
                        logger.LogInformation("Processamento em {TimeElapsed:c}", watch.Elapsed);
                    }

                using (logger.BeginScope("Foto de Perfil"))
                    try
                    {
                        watch.Restart();

                        importadorParlamentar.DownloadFotos().Wait();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                    finally
                    {
                        watch.Stop();
                        logger.LogInformation("Processamento em {TimeElapsed:c}", watch.Elapsed);
                    }
            }

            if (importadorDespesas != null)
            {
                using (logger.BeginScope("Despesas {Ano}", DateTime.Now.Year - 1))
                    try
                    {
                        watch.Restart();

                        importadorDespesas.Importar(DateTime.Now.Year - 1);
                    }
                    catch (BusinessException) { }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                    finally
                    {
                        watch.Stop();
                        logger.LogInformation("Processamento em {TimeElapsed:c}", watch.Elapsed);
                    }

                using (logger.BeginScope("Despesas {Ano}", DateTime.Now.Year))
                    try
                    {
                        watch.Restart();

                        importadorDespesas.Importar(DateTime.Now.Year);
                    }
                    catch (BusinessException) { }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                    finally
                    {
                        watch.Stop();
                        logger.LogInformation("Processamento em {TimeElapsed:c}", watch.Elapsed);
                    }
            }

            // using (logger.BeginScope("Remuneração"))
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
            //    logger.LogDebug("Processamento em {TimeElapsed:c}", watch.Elapsed);
            //}
        }
    }
}
