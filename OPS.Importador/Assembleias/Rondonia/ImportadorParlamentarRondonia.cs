using System.Globalization;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Parlamentar;

namespace OPS.Importador.Assembleias.Rondonia
{
    public class ImportadorParlamentarRondonia : ImportadorParlamentarCrawler
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorParlamentarRondonia(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                BaseAddress = "https://www.al.ro.leg.br/deputados/perfil/",
                SeletorListaParlamentares = "section>.container>.grid>div",
                Estado = Estados.Rondonia,
            });
        }

        public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
        {
            var nomeparlamentar = parlamentar.QuerySelector("a div").TextContent.Trim().ToTitleCase();
            var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

            deputado.UrlPerfil = (parlamentar.QuerySelector("a") as IHtmlAnchorElement).Href;
            deputado.UrlFoto = (parlamentar.QuerySelector("img") as IHtmlImageElement)?.Source;

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            var detalhes = subDocument.QuerySelectorAll(".col-span-4 p");
            deputado.NomeCivil = detalhes[0].TextContent.Trim().ToTitleCase();
            deputado.IdPartido = BuscarIdPartido(detalhes[1].TextContent.Trim());

            if (detalhes.Length > 2)
                deputado.Email = detalhes[2].TextContent.Trim();
        }
    }
}
