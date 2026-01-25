using AngleSharp;
using Microsoft.Extensions.Logging;
using OPS.Importador.Comum.Utilities;

namespace OPS.Importador.Comum.Despesa
{
    public abstract class ImportadorDespesasRestApiMensal : ImportadorDespesasBase, IImportadorDespesas
    {
        public ImportadorDespesasRestApiMensal(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public Task Importar(int ano)
        {
            CarregarDespesaArquivoHistorico(ano);

            using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano }))
            {
                CarregarHashes(ano);
                var context = httpClientResilient.CreateAngleSharpContext();

                ParallelOptions parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 1 // TODO: Implement IDbContextFactory and return to 3
                };

                var meses = Enumerable.Range(1, 12);
                Parallel.ForEach(meses, parallelOptions, mes =>
                {
                    //if (ano == 2019 && mes == 1) continue;
                    if (ano == DateTime.Now.Year && mes > DateTime.Today.Month) return;

                    using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = mes }))
                    {
                        // TODO: Reimportar para iniciar uso.
                        //var dataReferencia = new DateTime(ano, mes, 1);
                        //if (dataReferencia < DateTime.Today.AddMonths(-4)) // TODO: Considerar data da última importação
                        //{
                        //    CarregarDespesaTempDoHistorico(ano, mes);
                        //    return;
                        //}

                        try
                        {
                            ImportarDespesas(context, ano, mes).Wait();
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, ex.Message);
                        }

                    }
                });

                ProcessarDespesas(ano);
            }

            return Task.CompletedTask;
        }

        public abstract Task ImportarDespesas(IBrowsingContext context, int ano, int mes);
    }

    //public class ImportadorDespesasRestApiConfig
    //{
    //    public string BaseAddress { get; internal set; }
    //    public Estado Estado { get; internal set; }
    //}
}
