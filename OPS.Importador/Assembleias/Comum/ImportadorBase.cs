using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Assembleias.Parlamentar;

namespace OPS.Importador.Assembleias.Comum
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
                    ImportarDespesas(2023); // TODO: Primeiro ano do mandato
                }
                else
                {
                    //if (!(importadorDespesas is ImportadorDespesasAmazonas
                    //    || importadorDespesas is ImportadorDespesasBahia
                    //    || importadorDespesas is ImportadorDespesasCeara
                    //    || importadorDespesas is ImportadorDespesasMaranhao
                    //    || importadorDespesas is ImportadorDespesasRioGrandeDoSul))
                    //{
                    //    ImportarDespesas(DateTime.Now.Year - 2);
                    //    ImportarDespesasAnoAnterior();
                    //}

                    ImportarDespesasAnoAtual();
                }

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

        private void ImportarDespesas(int ano)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            using (logger.BeginScope("Despesas {Ano}", ano))
                try
                {
                    importadorDespesas.Importar(ano);
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

        private void ImportarDespesasAnoAtual()
        {
            ImportarDespesas(DateTime.Now.Year);
        }

        private void ImportarDespesasAnoAnterior()
        {
            //if (importacaoIncremental) return;

            ImportarDespesas(DateTime.Now.Year - 1);
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
