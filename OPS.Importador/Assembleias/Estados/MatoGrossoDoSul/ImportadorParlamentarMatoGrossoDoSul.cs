using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;

namespace OPS.Importador.Assembleias.Estados.MatoGrossoDoSul
{
    public class ImportadorParlamentarMatoGrossoDoSul : ImportadorParlamentarCrawler
    {

        public ImportadorParlamentarMatoGrossoDoSul(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                BaseAddress = "https://al.ms.gov.br/Partidos/Lista",
                SeletorListaParlamentares = "#grid-container-parties ul li.todos",
                Estado = Estado.MatoGrossoDoSul,
            });
        }

        public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
        {
            var urlPerfil = (parlamentar.QuerySelector("a.cbp-l-caption-buttonRight") as IHtmlAnchorElement).Href;
            var deputado = GetDeputadoByMatriculaOrNew(Convert.ToInt32(urlPerfil.Split(@"/").Last()));

            deputado.UrlPerfil = urlPerfil;
            deputado.UrlFoto = (parlamentar.QuerySelector(".cbp-caption-defaultWrap img") as IHtmlImageElement)?.Source;
            deputado.Matricula = Convert.ToInt32(deputado.UrlPerfil.Split(@"/").Last());

            deputado.NomeParlamentar = parlamentar.QuerySelector(".cbp-l-grid-projects-title").TextContent.Trim().ToTitleCase();
            deputado.IdPartido = BuscarIdPartido(parlamentar.QuerySelector(".cbp-l-grid-projects-desc").TextContent.Trim());

            if (string.IsNullOrEmpty(deputado.NomeCivil))
                deputado.NomeCivil = deputado.NomeParlamentar;

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            var detalhes = subDocument.QuerySelectorAll("#content>.container>.col-lg-12>.col-lg-12>p").FirstOrDefault();

            if (detalhes != null)
            {
                var emailTelefone = detalhes.TextContent.Split("-");
                deputado.Email = emailTelefone[0].Trim();
                deputado.Telefone = emailTelefone[1].Trim();
            }
        }
    }
}
