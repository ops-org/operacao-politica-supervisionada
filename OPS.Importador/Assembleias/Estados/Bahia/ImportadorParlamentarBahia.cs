using System.Globalization;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;

namespace OPS.Importador.Assembleias.Estados.Bahia
{
    public class ImportadorParlamentarBahia : ImportadorParlamentarCrawler
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorParlamentarBahia(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            // https://www.al.ba.gov.br/deputados/deputados-estaduais (Em Exercicio)
            // https://www.al.ba.gov.br/deputados/legislatura-atual

            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                BaseAddress = "https://www.al.ba.gov.br/deputados/legislatura-atual",
                SeletorListaParlamentares = ".fe-div-table>div",
                Estado = Estado.Bahia,
            });
        }

        public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
        {
            var nomeparlamentar = parlamentar.QuerySelector(".deputado-nome span").TextContent.Trim().ToTitleCase();
            var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

            deputado.UrlPerfil = (parlamentar.QuerySelector(".list-item a") as IHtmlAnchorElement).Href;
            deputado.UrlFoto = (parlamentar.QuerySelector(".list-item img") as IHtmlImageElement)?.Source;
            deputado.Matricula = Convert.ToInt32(deputado.UrlPerfil.Split(@"/").Last());
            deputado.IdPartido = BuscarIdPartido(parlamentar.QuerySelector(".partido-nome").TextContent.Trim());

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            var detalhes = subDocument.QuerySelectorAll(".dados-deputado p");
            deputado.NomeCivil = detalhes[0].QuerySelector("span").TextContent.Trim().ToTitleCase();

            var profissaoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("PROFISSÃO", StringComparison.InvariantCultureIgnoreCase));
            if (profissaoElemento is not null)
            {
                deputado.Profissao = profissaoElemento.QuerySelector("span").TextContent.Trim().ToTitleCase();
            }

            var nascimentoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("NASCIMENTO", StringComparison.InvariantCultureIgnoreCase));
            if (nascimentoElemento is not null)
            {
                var nascimentoComNaturalidade = nascimentoElemento.QuerySelector("span").TextContent.Split(',');
                deputado.Nascimento = DateOnly.Parse(nascimentoComNaturalidade[0].Trim(), cultureInfo);
                deputado.Naturalidade = nascimentoComNaturalidade[1].Trim();
            }

            var sexoElemento = detalhes.FirstOrDefault(x => x.QuerySelector("strong").TextContent.StartsWith("SEXO", StringComparison.InvariantCultureIgnoreCase));
            if (sexoElemento is not null)
                deputado.Sexo = sexoElemento.QuerySelector("span").TextContent.Trim()[0].ToString();

            var contatos = subDocument.QuerySelectorAll(".fe-dep-dados-ajsut-mobile .linha-cv strong").First(x => x.TextContent.Trim() == "Contato").ParentElement;
            var telefoneElemento = contatos.QuerySelectorAll("p").FirstOrDefault(x => x.TextContent.StartsWith("Tel"));
            if (telefoneElemento != null)
                deputado.Telefone = string.Join(" ", telefoneElemento.ParentElement.QuerySelectorAll("span").Select(x => x.TextContent.Trim()));

            deputado.Email = contatos.QuerySelector("p a span").TextContent?.Trim().NullIfEmpty();
        }
    }
}
