using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.MatoGrosso;

public class ImportadorParlamentarMatoGrosso : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarMatoGrosso(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.al.mt.gov.br/parlamento/deputados",
            SeletorListaParlamentares = "main .card",
            Estado = Estado.MatoGrosso,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement item)
    {
        var nomeparlamentar = item.QuerySelector(".card-body .card-title").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlPerfil = (item.QuerySelector(">a") as IHtmlAnchorElement).Href;
        deputado.IdPartido = BuscarIdPartido(item.QuerySelector(".card-body .badge").TextContent.Trim());

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        deputado.UrlFoto = (subDocument.QuerySelector("main a>img") as IHtmlImageElement)?.Source;

        var perfil = subDocument.QuerySelectorAll("ul.list-group>li")
            .Where(xx => xx.TextContent.Contains(":"))
            .Select(x => new { Key = x.TextContent.Split(':')[0].Trim(), Value = x.TextContent.Split(':')[1].Trim() });

        deputado.NomeCivil = perfil.First(x => x.Key == "Nome civil").Value.ToTitleCase().NullIfEmpty();

        ImportacaoUtils.MapearRedeSocial(deputado, subDocument.QuerySelectorAll("ul.nav a")); // Todos são as redes sociaos da AL
    }
}

