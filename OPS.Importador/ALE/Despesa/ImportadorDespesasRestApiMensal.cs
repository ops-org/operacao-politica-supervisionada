using System;
using AngleSharp;
using AngleSharp.Io;
using Microsoft.Extensions.Logging;

namespace OPS.Importador.ALE.Despesa
{
    public abstract class ImportadorDespesasRestApiMensal : ImportadorDespesasBase, IImportadorDespesas
    {
        public ImportadorDespesasRestApiMensal(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public void Importar(int ano)
        {
            logger.LogWarning("Despesas do(a) {idEstado}:{CasaLegislativa} de {Ano}", config.Estado.GetHashCode(), config.Estado.ToString(), ano);

            CarregarHashes(ano);
            LimpaDespesaTemporaria();

            var htmlRequester = new DefaultHttpRequester();
            htmlRequester.Headers["User-Agent"] = "Mozilla/5.0 (compatible; OPS_bot/1.0; +https://ops.net.br)";
            var handler = new DefaultHttpRequester { Timeout = TimeSpan.FromMinutes(5) };
            var configuration = AngleSharp.Configuration.Default
                .With(htmlRequester)
                .With(handler)
                .WithDefaultLoader()
                .WithDefaultCookies()
                .WithCulture("pt-BR");
            var context = BrowsingContext.New(configuration);

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
