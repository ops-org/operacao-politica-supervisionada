using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Parlamentar;
using OPS.Importador.Comum.Utilities;

namespace OPS.Importador.Assembleias.Tocantins
{
    public class ImportadorParlamentarTocantins : ImportadorParlamentarCrawler
    {

        public ImportadorParlamentarTocantins(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                BaseAddress = "https://www.al.to.leg.br/perfil",
                SeletorListaParlamentares = "#list-parlamentares .quadro-deputado",
                Estado = Estados.Tocantins,
                ColetaDadosDoPerfil = false
            });
        }

        public override DeputadoEstadual ColetarDadosLista(IElement document)
        {
            var nomeparlamentar = document.QuerySelector("h3").TextContent.Trim().ToTitleCase();
            var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

            deputado.UrlPerfil = (document.QuerySelector("a.btn-perfil") as IHtmlAnchorElement).Href;
            deputado.UrlFoto = (document.QuerySelector("img.foto-deputado") as IHtmlImageElement)?.Source;
            deputado.Matricula = Convert.ToInt32(deputado.UrlPerfil.Split(@"/").Last());
            deputado.IdPartido = BuscarIdPartido(document.QuerySelector("h5").TextContent.Trim());

            deputado.Telefone = document.QuerySelectorAll("h6")[0].TextContent.Trim();
            deputado.Email = document.QuerySelectorAll("h6")[1].TextContent.Trim();

            ImportacaoUtils.MapearRedeSocial(deputado, document.QuerySelectorAll(".profile-social-container a"));

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            throw new NotImplementedException();
        }
    }
}
