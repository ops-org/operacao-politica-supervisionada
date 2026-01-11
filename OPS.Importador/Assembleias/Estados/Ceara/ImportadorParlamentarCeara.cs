using System.Data;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;

namespace OPS.Importador.Assembleias.Estados.Ceara;

public class ImportadorParlamentarCeara : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarCeara(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://al.ce.gov.br/deputados",
            SeletorListaParlamentares = ".deputado_page .deputado_card",
            Estado = Estado.Ceara,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
    {
        var nomeparlamentar = parlamentar.QuerySelector(".deputado_card--nome").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlPerfil = (parlamentar.QuerySelector(".deputado_card--nome a") as IHtmlAnchorElement).Href;
        deputado.UrlFoto = (parlamentar.QuerySelector("img") as IHtmlImageElement)?.Source;
        //deputado.Matricula = Convert.ToInt32(deputado.UrlPerfil.Split(@"/").Last());
        deputado.IdPartido = BuscarIdPartido(parlamentar.QuerySelector(".deputado_card--partido").TextContent.Trim());

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        var detalhes = subDocument.QuerySelectorAll(".container>.row>.col-md-3>div>div.d-flex");

        if (string.IsNullOrEmpty(deputado.NomeCivil))
            deputado.NomeCivil = BuscarTexto(detalhes, "Nome Completo").ToTitleCase();

        deputado.Profissao = BuscarTexto(detalhes, "Profissão")?.ToTitleCase();
        deputado.Email = BuscarTexto(detalhes, "E-mails");
        deputado.Site = BuscarTexto(detalhes, "Site Pessoal");
        deputado.Telefone = BuscarTexto(detalhes, "Telefones");
    }

    public string BuscarTexto(IHtmlCollection<IElement> detalhes, string textoBuscar)
    {
        var elemento = detalhes.FirstOrDefault(x => x.QuerySelector("span.font-weight-bold").TextContent.Contains(textoBuscar, StringComparison.InvariantCultureIgnoreCase));
        if (elemento is not null)
        {
            return string.Join(", ", elemento.QuerySelectorAll(".text-black-50").Select(x => x.TextContent.Trim()));
        }

        return null;
    }
}
