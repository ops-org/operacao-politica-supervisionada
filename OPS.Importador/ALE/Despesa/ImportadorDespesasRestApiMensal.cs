using System;
using System.Collections.Generic;
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

                for (int mes = 1; mes <= 12; mes++)
                {
                    //if (ano == 2019 && mes == 1) continue;
                    if (ano == DateTime.Now.Year && mes > DateTime.Today.Month) break;

                    using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = mes }))
                    {
                        ImportarDespesas(context, ano, mes);
                    }
                }

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
