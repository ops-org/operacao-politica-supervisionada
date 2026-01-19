using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Parlamentar;

namespace OPS.Importador.Assembleias.SantaCatarina
{
    public class ImportadorParlamentarSantaCatarina : ImportadorParlamentarCrawler
    {
        public ImportadorParlamentarSantaCatarina(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarCrawlerConfig
            {
                Estado = Estados.SantaCatarina,
                BaseAddress = "https://www.alesc.sc.gov.br/todos-deputados",
                SeletorListaParlamentares = "table.views-table tbody tr"
            });
        }

        public override DeputadoEstadual ColetarDadosLista(IElement document)
        {
            var itens = document.QuerySelectorAll("td");
            var colunaNome = itens[0].QuerySelector("a");

            var nomeparlamentar = colunaNome.TextContent.Trim().ToTitleCase();
            var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

            deputado.UrlPerfil = (colunaNome as IHtmlAnchorElement).Href;
            deputado.IdPartido = BuscarIdPartido(itens[1].TextContent.Trim());
            deputado.Email = itens[2].QuerySelector("a").TextContent.Trim();

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            var detalhes = subDocument.QuerySelectorAll(".deputado-resumo");
            deputado.Nascimento = DateOnly.FromDateTime(DateTime.Parse(detalhes[0].QuerySelector("span.date-display-single").Attributes["content"].Value));
            deputado.Escolaridade = detalhes[1].TextContent.Replace("Escolaridade:", "").Trim();
            deputado.Naturalidade = detalhes[2].TextContent.Replace("Origem:", "").Split('/')[0].Trim();
            //var gabinete = detalhes[3].TextContent.Trim();
            deputado.Telefone = detalhes[4].TextContent.Replace("Contatos:", "").Trim();
        }
    }
}
