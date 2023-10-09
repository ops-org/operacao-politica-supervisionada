using System;
using AngleSharp;
using AngleSharp.Io;

namespace OPS.Importador.ALE.Despesa
{
    public abstract class ImportadorDespesasRestApiAnual : ImportadorDespesasBase, IImportadorDespesas
    {
        public ImportadorDespesasRestApiAnual(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public void Importar(int ano)
        {
            CarregarHashes(ano);
            LimpaDespesaTemporaria();

            var htmlRequester = new DefaultHttpRequester();
            htmlRequester.Headers["User-Agent"] = "Mozilla/5.0 (compatible; OPS_bot/1.0; +https://ops.net.br)";
            var handler = new DefaultHttpRequester { Timeout = TimeSpan.FromMinutes(5) };

            var config = Configuration.Default.With(htmlRequester).With(handler).WithDefaultLoader().WithDefaultCookies().WithCulture("pt-BR");
            var context = BrowsingContext.New(config);

            ImportarDespesas(context, ano);
            ProcessarDespesas(ano);
        }

        public abstract void ImportarDespesas(IBrowsingContext context, int ano);
    }

    //public class ImportadorDespesasRestApiConfig
    //{
    //    public string BaseAddress { get; internal set; }
    //    public Estado Estado { get; internal set; }
    //}
}
