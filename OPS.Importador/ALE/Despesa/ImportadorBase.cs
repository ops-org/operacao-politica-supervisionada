using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
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

        private bool importacaoIncremental { get; init; }

        public ImportadorBase(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetRequiredService<ILogger<ImportadorBase>>();

            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            importacaoIncremental = Convert.ToBoolean(configuration["AppSettings:ImportacaoDespesas:Incremental"] ?? "false");
        }

        public virtual void ImportarCompleto()
        {
            if (importadorParlamentar != null)
            {
                ImportarPerfilParlamentar();
                ImportarImagemParlamentar();
            }

            if (importadorDespesas != null)
            {
                ImportarDespesasAnoAnterior();
                ImportarDespesasAnoAtual();
            }

            ImportarRemuneracao();
        }

        private void ImportarRemuneracao()
        {
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //using (logger.BeginScope("Remuneração"))
            //    try
            //    {
            //        watch.Restart();

            //        var anoAtual = DateTime.Now;
            //        logger.LogInformation("Remuneração do(a) {CasaLegislativa}", config.SiglaEstado);
            //        ImportarRemuneracao(anoAtual.Year, anoAtual.Month);
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.LogError(ex, ex.Message);
            //    }
            //    finally
            //    {
            //        watch.Stop();
            //        logger.LogDebug("Processamento em {TimeElapsed:c}", watch.Elapsed);
            //    }
        }

        private void ImportarDespesasAnoAtual()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            using (logger.BeginScope("Despesas {Ano}", DateTime.Now.Year))
                try
                {
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

        private void ImportarDespesasAnoAnterior()
        {
            //if (importacaoIncremental) return;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            using (logger.BeginScope("Despesas {Ano}", DateTime.Now.Year - 1))
                try
                {
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
        }

        private void ImportarImagemParlamentar()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            using (logger.BeginScope("Foto de Perfil"))
                try
                {
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

        private void ImportarPerfilParlamentar()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            using (logger.BeginScope("Perfil do Parlamentar"))
                try
                {
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
        }
    }
}
