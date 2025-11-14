using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using OPS.Importador.Utilities;

namespace OPS.Importador.ALE.Despesa
{
    public abstract class ImportadorDespesasRestApiMensal : ImportadorDespesasBase, IImportadorDespesas
    {
        public ImportadorDespesasRestApiMensal(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public void Importar(int ano)
        {
            using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano }))
            {
                CarregarHashes(ano);
                var context = httpClientResilient.CreateAngleSharpContext();

                ParallelOptions parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 3
                };

                var meses = Enumerable.Range(1, 12);
                Parallel.ForEach(meses, parallelOptions, mes =>
                {
                    //if (ano == 2019 && mes == 1) continue;
                    if (ano == DateTime.Now.Year && mes > DateTime.Today.Month) return;

                    using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = mes }))
                    {
                        ImportarDespesas(context, ano, mes);
                    }
                });

                ProcessarDespesas(ano);
            }
        }

        public abstract void ImportarDespesas(IBrowsingContext context, int ano, int mes);
    }

    //public class ImportadorDespesasRestApiConfig
    //{
    //    public string BaseAddress { get; internal set; }
    //    public Estado Estado { get; internal set; }
    //}
}
