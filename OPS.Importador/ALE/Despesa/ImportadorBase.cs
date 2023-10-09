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
            //try
            //{
            //    //logger.LogWarning("Dados do(a) {CasaLegislativa}", config.SiglaEstado);
            //    if (importadorParlamentar != null)
            //        importadorParlamentar.Importar();
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, ex.Message);
            //}

            //try
            //{
            //    logger.LogInformation("Imagens do(a) {CasaLegislativa}", config.SiglaEstado);
            //    DownloadFotosParlamentares();
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, ex.Message);
            //}

            try
            {
                if (importadorDespesas != null)
                    importadorDespesas.Importar(DateTime.Now.Year - 1);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

            try
            {
                if (importadorDespesas != null)
                    importadorDespesas.Importar(DateTime.Now.Year);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

            //try
            //{
            //    var anoAtual = DateTime.Now;
            //    logger.LogInformation("Remuneração do(a) {CasaLegislativa}", config.SiglaEstado);
            //    ImportarRemuneracao(anoAtual.Year, anoAtual.Month);
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, ex.Message);
            //}
        }
    }
}
