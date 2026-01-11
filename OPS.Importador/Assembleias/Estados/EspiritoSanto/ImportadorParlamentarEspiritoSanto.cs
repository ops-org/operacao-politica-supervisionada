using System.Globalization;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;

namespace OPS.Importador.Assembleias.Estados.EspiritoSanto
{
    public class ImportadorParlamentarEspiritoSanto : ImportadorParlamentarCrawler
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorParlamentarEspiritoSanto(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                BaseAddress = "https://www.al.es.gov.br/Deputado/Lista",
                SeletorListaParlamentares = "#divListaDeputados>div",
                Estado = Estado.EspiritoSanto,
            });
        }

        public override DeputadoEstadual ColetarDadosLista(IElement item)
        {
            var nomeparlamentar = item.QuerySelector(".nomeDeputadoLista").TextContent.Replace("(licenciado)", "").Trim().ToTitleCase();
            var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

            deputado.UrlPerfil = (item.QuerySelector("a.linkLimpo") as IHtmlAnchorElement).Href;
            deputado.UrlFoto = (item.QuerySelector("#divImagemDeputado img") as IHtmlImageElement)?.Source;

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            var dados = subDocument
                .QuerySelectorAll(".fonte-dados-deputado>div")
                .Select(x => new { Key = x.QuerySelector("label").TextContent.Trim(), Value = x.QuerySelector("span").TextContent.Trim() });

            if (!string.IsNullOrEmpty(dados.First(x => x.Key == "Partido:").Value))
                deputado.IdPartido = BuscarIdPartido(dados.First(x => x.Key == "Partido:").Value);

            deputado.NomeCivil = dados.FirstOrDefault(x => x.Key == "Nome Civil:")?.Value;
            deputado.Naturalidade = dados.FirstOrDefault(x => x.Key == "Naturalidade:")?.Value;
            deputado.Nascimento = DateOnly.Parse(dados.First(x => x.Key == "Data de Nascimento:").Value, cultureInfo);
            deputado.Telefone = dados.FirstOrDefault(x => x.Key == "Telefone:")?.Value;
            deputado.Email = dados.FirstOrDefault(x => x.Key == "E-mail:")?.Value;
        }
    }
}
