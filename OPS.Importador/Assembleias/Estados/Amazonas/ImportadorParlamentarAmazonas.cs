using System.Globalization;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.Amazonas
{
    public class ImportadorParlamentarAmazonas : ImportadorParlamentarCrawler
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorParlamentarAmazonas(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                BaseAddress = "https://www.aleam.gov.br/deputados/",
                SeletorListaParlamentares = ".dep .dep-int-cont",
                Estado = Estado.Amazonas,
            });
        }

        public override DeputadoEstadual ColetarDadosLista(IElement document)
        {
            var nomeparlamentar = document.QuerySelector(".dep-int-cont__title").TextContent.Trim().ToTitleCase();
            var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

            deputado.UrlFoto = (document.QuerySelector("img") as IHtmlImageElement)?.Source;
            deputado.UrlPerfil = (document.QuerySelector("a") as IHtmlAnchorElement).Href;
            deputado.IdPartido = BuscarIdPartido(document.QuerySelector(".dep-int-cont__part").TextContent.Trim());

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            deputado.NomeCivil = subDocument.QuerySelector("#nome-dep").TextContent.Trim();
            deputado.Naturalidade = subDocument.QuerySelector("#nat-dep").TextContent.Trim();
            if (subDocument.QuerySelector("#ani-dep") != null)
            {
                var aniversario = subDocument.QuerySelector("#ani-dep").TextContent.Trim();
                if (aniversario.Length == 8)
                    deputado.Nascimento = DateOnly.Parse(aniversario, cultureInfo);
            }

            deputado.Email = subDocument.QuerySelector("#email-dep").TextContent.Trim();
            deputado.Telefone = subDocument.QuerySelector("#tel-dep").TextContent.Trim().Replace("NA", "").NullIfEmpty();
            deputado.Sexo = deputado.Email.StartsWith("deputada.") ? "F" : "M";

            ImportacaoUtils.MapearRedeSocial(deputado, subDocument.QuerySelectorAll("#redes-pf-dep a"));
        }
    }
}
