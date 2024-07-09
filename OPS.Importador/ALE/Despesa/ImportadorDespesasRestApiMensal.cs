using System;
using AngleSharp;
using AngleSharp.Io.Network;
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

            var configuration = AngleSharp.Configuration.Default
                .With(new HttpClientRequester(httpClient))
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
