using System;
using AngleSharp;
using AngleSharp.Io;

namespace OPS.Importador.ALE.Despesa
{
    public abstract class ImportadorDespesasRestApiMensal : ImportadorDespesasBase, IImportadorDespesas
    {
        public ImportadorDespesasRestApiMensal(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public void Importar(int ano)
        {
            CarregarHashes(ano);
            LimpaDespesaTemporaria();

            var handler = new DefaultHttpRequester { Timeout = TimeSpan.FromMinutes(5) };
            var config = Configuration.Default.With(handler).WithDefaultLoader();
            var context = BrowsingContext.New(config);

            for (int mes = 1; mes <= 12; mes++)
            {
                //if (ano == 2019 && mes == 1) continue;
                if (ano == DateTime.Now.Year && mes > DateTime.Today.Month) break;

                ImportarDespesas(context, ano, mes);
            }

            ProcessarDespesas(ano);
        }

        public abstract void ImportarDespesas(IBrowsingContext context, int ano, int mes);
    }

    //public class ImportadorDespesasRestApiConfig
    //{
    //    public string BaseAddress { get; internal set; }
    //    public Estado Estado { get; internal set; }
    //}
}
