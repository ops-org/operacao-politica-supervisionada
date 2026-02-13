using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPS.Core.Exceptions;
using OPS.Importador.Assembleias.Piaui;
using OPS.Importador.Assembleias.RioDeJaneiro;
using OPS.Importador.Comum.Despesa;
using OPS.Importador.Comum.Parlamentar;

namespace OPS.Importador.Comum
{
    public abstract class ImportadorBase
    {
        public ILogger<ImportadorBase> logger { get; set; }

        protected IImportadorParlamentar importadorParlamentar { get; set; }

        protected IImportadorDespesas importadorDespesas { get; set; }

        protected readonly AppSettings appSettings;

        public ImportadorBase(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetRequiredService<ILogger<ImportadorBase>>();
            appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        }

        public virtual async Task ImportarCompleto()
        {
            if (importadorParlamentar != null)
            {
                importadorParlamentar.AtualizarDatasImportacaoParlamentar(pInicio: DateTime.UtcNow);
                ImportarPerfilParlamentar();
                importadorParlamentar.AtualizarDatasImportacaoParlamentar(pFim: DateTime.UtcNow);

                ImportarImagemParlamentar();
            }

            if (importadorDespesas != null)
            {
                importadorDespesas.AtualizarDatasImportacaoDespesas(dInicio: DateTime.UtcNow);

                if (importadorDespesas is ImportadorDespesasPiaui || importadorDespesas is ImportadorDespesasRioDeJaneiro) // importadorDespesas is ImportadorDespesasMinasGerais
                {
                    // Dados por mandato
                    await ImportarDespesas(2023); // TODO: Primeiro ano do mandato
                }
                else
                {
                    //if (!(importadorDespesas is ImportadorDespesasAmazonas
                    //    || importadorDespesas is ImportadorDespesasBahia
                    //    || importadorDespesas is ImportadorDespesasCeara
                    //    || importadorDespesas is ImportadorDespesasMaranhao
                    //    || importadorDespesas is ImportadorDespesasRioGrandeDoSul))
                    //{
                    //    await ImportarDespesas(DateTime.Now.Year - 2);
                    //    await ImportarDespesasAnoAnterior();
                    //}

                    //await ImportarDespesasAnoAtual();

                    //for (int ano = 2023; ano <= 2026; ano++)
                    //    await ImportarDespesas(ano);

                    await ImportarDespesas(2025);
                    await ImportarDespesas(2026);
                }

                //for (int ano = 2008; ano <= 2026; ano++)
                //    await ImportarDespesas(ano);

                importadorDespesas.AtualizarDatasImportacaoDespesas(dFim: DateTime.UtcNow);
            }

            //ImportarRemuneracao();
        }

        //private void ImportarRemuneracao()
        //{
        //    var watch = System.Diagnostics.Stopwatch.StartNew();
        //    using (logger.BeginScope("Remuneração"))
        //        try
        //        {
        //            watch.Restart();

        //            var anoAtual = DateTime.Now;
        //            logger.LogInformation("Remuneração do(a) {CasaLegislativa}", config.SiglaEstado);
        //            ImportarRemuneracao(anoAtual.Year, anoAtual.Month);
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.LogError(ex, ex.Message);
        //        }
        //        finally
        //        {
        //            watch.Stop();
        //            logger.LogDebug("Processamento em {TimeElapsed:c}", watch.Elapsed);
        //        }
        //}

        private async Task ImportarDespesas(int ano)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            using (logger.BeginScope("Despesas {Ano}", ano))
                try
                {
                    await importadorDespesas.Importar(ano);
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

        private async Task ImportarDespesasAnoAtual()
        {
            await ImportarDespesas(DateTime.Now.Year);
        }

        private async Task ImportarDespesasAnoAnterior()
        {
            //if (appSettings.ImportacaoIncremental) return;

            await ImportarDespesas(DateTime.Now.Year - 1);
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
