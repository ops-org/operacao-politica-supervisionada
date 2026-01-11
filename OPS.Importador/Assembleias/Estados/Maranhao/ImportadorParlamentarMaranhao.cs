using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;

namespace OPS.Importador.Assembleias.Estados.Maranhao
{
    public class ImportadorParlamentarMaranhao : ImportadorParlamentarCrawler
    {

        public ImportadorParlamentarMaranhao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                BaseAddress = "https://www.al.ma.leg.br/sitealema/deputados/",
                SeletorListaParlamentares = "section.section .news-card",
                Estado = Estado.Maranhao,
            });
        }


        public override DeputadoEstadual ColetarDadosLista(IElement item)
        {
            var nomeparlamentar = item.QuerySelector(".news-card-title").TextContent.Trim().ToTitleCase();
            var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

            if (string.IsNullOrEmpty(deputado.NomeCivil))
                deputado.NomeCivil = deputado.NomeParlamentar;

            deputado.UrlPerfil = (item.QuerySelector(".news-card-title a") as IHtmlAnchorElement).Href;
            // deputado.UrlFoto = (item.QuerySelector("img") as IHtmlImageElement)?.Source;

            deputado.IdPartido = BuscarIdPartido(item.QuerySelector(".news-card-chapeu").TextContent.Trim());

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            deputado.UrlFoto = (subDocument.QuerySelector(".deputado img.img-full") as IHtmlImageElement)?.Source;

            var perfil = subDocument.QuerySelectorAll(".deputado>p")
                .Where(xx => xx.TextContent.Contains(":"))
                .Select(x => new { Key = x.TextContent.Split(':')[0].Trim(), Value = x.TextContent.Replace("Ramal:", "Ramal").Split(':')[1].Trim() });

            if (!string.IsNullOrEmpty(perfil.First(x => x.Key == "Aniversário").Value))
                deputado.Nascimento = DateOnly.Parse(perfil.First(x => x.Key == "Aniversário").Value);

            deputado.Profissao = perfil.First(x => x.Key == "Profissão").Value.ToTitleCase().NullIfEmpty();
            deputado.Site = perfil.First(x => x.Key == "Site").Value.NullIfEmpty();
            deputado.Email = perfil.First(x => x.Key == "E-mail").Value.NullIfEmpty() ?? deputado.Email;
            deputado.Telefone = perfil.First(x => x.Key == "Telefone").Value;

            // ImportacaoUtils.MapearRedeSocial(deputado, subDocument.QuerySelectorAll(".deputado ul a")); // Todos são as redes sociaos da AL
        }
    }
}
