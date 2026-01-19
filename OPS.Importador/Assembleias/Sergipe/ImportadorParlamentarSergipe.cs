using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Parlamentar;
using OPS.Importador.Comum.Utilities;

namespace OPS.Importador.Assembleias.Sergipe
{
    public class ImportadorParlamentarSergipe : ImportadorParlamentarCrawler
    {

        public ImportadorParlamentarSergipe(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                BaseAddress = "https://aleselegis.al.se.leg.br/spl/parlamentares.aspx", //?leg=19
                SeletorListaParlamentares = "#ContentPlaceHolder1_parlamentares_lista .custom-user-profile",
                Estado = Estados.Sergipe,
            });
        }

        public override DeputadoEstadual ColetarDadosLista(IElement document)
        {
            var nomeparlamentar = document.QuerySelector("a.kt-widget__username").TextContent.Trim().ToTitleCase();
            var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

            deputado.UrlPerfil = (document.QuerySelector("a.kt-widget__username") as IHtmlAnchorElement).Href;
            deputado.UrlFoto = (document.QuerySelector("img.kt-widget__img") as IHtmlImageElement)?.Source;
            deputado.Matricula = Convert.ToInt32(deputado.UrlPerfil.Split(@"=").Last());
            deputado.IdPartido = BuscarIdPartido(document.QuerySelector("span.kt-widget__username").TextContent.Replace("(", "").Replace(")", "").Trim());

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument document)
        {

            var perfil = document
                .QuerySelectorAll("#dados_parlamentar .kt-widget__info")
                .Select(x => new { Key = x.QuerySelector(".kt-widget__label").TextContent.Trim(), Value = x.QuerySelector(".kt-widget__data").TextContent.Trim() })
                .ToList();

            if (string.IsNullOrEmpty(deputado.NomeCivil))
                deputado.NomeCivil = perfil.FirstOrDefault(x => x.Key == "Nome civil:")?.Value;

            deputado.Telefone = perfil.FirstOrDefault(x => x.Key == "Telefone(s):")?.Value;
            //deputado.Celular = perfil.FirstOrDefault(x => x.Key == "Celular:")?.Value;
            deputado.Email = perfil.FirstOrDefault(x => x.Key == "E-mail:")?.Value;

            ImportacaoUtils.MapearRedeSocial(deputado, document.QuerySelectorAll("#dados_parlamentar_links a"));
            deputado.Site = null;
        }
    }
}
