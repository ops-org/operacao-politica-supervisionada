using System.Net;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.Para
{
    public class ImportadorParlamentarPara : ImportadorParlamentarCrawler
    {

        public ImportadorParlamentarPara(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                BaseAddress = "https://alepa.pa.gov.br/Institucional/Deputados",
                SeletorListaParlamentares = "main form>script",
                Estado = Estado.Para,
            });
        }

        public override async Task Importar()
        {
            ArgumentNullException.ThrowIfNull(config, nameof(config));

            var context = httpClient.CreateAngleSharpContext();

            var document = await context.OpenAsyncAutoRetry(config.BaseAddress);
            if (document.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"{config.BaseAddress} {document.StatusCode}");
            }
            ;

            var scriptParlamentares = document.QuerySelector(config.SeletorListaParlamentares).ToHtml();
            string pattern = @"'(\d+)'";
            RegexOptions options = RegexOptions.Multiline;

            var maches = Regex.Matches(scriptParlamentares, pattern, options);
            if (maches.Count == 0) throw new Exception("Erro ao buscar a lista de deputados");

            foreach (Match m in maches)
            {
                var matricula = m.Groups[1].Value;
                var deputado = GetDeputadoByMatriculaOrNew(Convert.ToInt32(matricula));
                if (deputado == null) continue;

                deputado.UrlPerfil = $"https://alepa.pa.gov.br/Institucional/Deputado/{matricula}";
                var subDocument = await context.OpenAsyncAutoRetry(deputado.UrlPerfil);
                if (document.StatusCode != HttpStatusCode.OK)
                {
                    logger.LogError("Erro ao consultar parlamentar: {NomeDeputado} {StatusCode}", deputado.UrlPerfil, subDocument.StatusCode);
                    continue;
                }
                ;
                ColetarDadosPerfil(deputado, subDocument);

                InsertOrUpdate(deputado);
            }

            logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            deputado.NomeParlamentar = subDocument.QuerySelector("main>h1").TextContent.Trim().ToTitleCase();
            deputado.UrlFoto = (subDocument.QuerySelector(".card-politico-img-wrapper img") as IHtmlImageElement)?.Source.Split("?")[0];

            var perfil = subDocument.QuerySelectorAll(".card-politico-info-wrapper>span")
                .Where(xx => xx.TextContent.Contains(":"))
                .Select(x => new { Key = x.TextContent.Split(':')[0].Trim(), Value = x.TextContent.Split(':')[1].Trim() });

            deputado.NomeCivil = perfil.First(x => x.Key == "Nome completo").Value;
            deputado.Nascimento = DateOnly.Parse(perfil.First(x => x.Key == "Nascimento").Value.Replace(".", "/"));
            deputado.Profissao = perfil.First(x => x.Key == "Profissão").Value.ToTitleCase().NullIfEmpty();
            deputado.Email = subDocument.QuerySelector(".card-politico-info-wrapper a b").TextContent.Trim().NullIfEmpty();

            deputado.IdPartido = BuscarIdPartido(perfil.First(x => x.Key == "Partido Político").Value);
            deputado.Telefone = subDocument.QuerySelectorAll(".card-politico-info-wrapper p").FirstOrDefault(x => x.TextContent.StartsWith("Tel:"))?.TextContent.Replace("Tel:", "").Trim().NullIfEmpty();

            ImportacaoUtils.MapearRedeSocial(deputado, subDocument.QuerySelectorAll(".card-politico-social-wrapper a"));
        }

        public override DeputadoEstadual ColetarDadosLista(IElement document)
        {
            throw new NotImplementedException();
        }
    }
}
