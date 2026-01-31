using System.Globalization;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Parlamentar;

namespace OPS.Importador.Assembleias.Amapa
{
    public class ImportadorParlamentarAmapa : ImportadorParlamentarCrawler
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorParlamentarAmapa(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                BaseAddress = "http://www.al.ap.gov.br/pagina.php?pg=exibir_legislatura",
                SeletorListaParlamentares = ".container .ls-box:not(.ls-box-filter)",
                Estado = Estados.Amapa,
            });
        }

        ///// <summary>
        ///// Importação manual para deputados especificos (inativos e que não aparecem na lista principal)
        ///// </summary>
        ///// <returns></returns>
        //public override async Task Importar()
        //{
        //    var context = httpClient.CreateAngleSharpContext();

        //    var document = await context.OpenAsyncAutoRetry(config.BaseAddress);
        //    if (document.StatusCode != HttpStatusCode.OK)
        //    {
        //        Console.WriteLine($"{config.BaseAddress} {document.StatusCode}");
        //    };

        //    var parlamentares = new[] { 29, 30, 36, 39, 60, 63, 76, 77, 80, 82, 90 };
        //    foreach (var matricula in parlamentares)
        //    {
        //        DeputadoEstadual deputado = new()
        //        {
        //            Matricula = (int)matricula,
        //            IdEstado = (short)Estado.Amapa.GetHashCode(),
        //            UrlPerfil = $"http://www.al.ap.gov.br/pagina.php?pg=exibir_parlamentar&iddeputado={matricula}"
        //        };

        //        var subDocument = await context.OpenAsyncAutoRetry(deputado.UrlPerfil);
        //        if (document.StatusCode != HttpStatusCode.OK)
        //        {
        //            logger.LogError("Erro ao consultar parlamentar: {NomeDeputado} {StatusCode}", deputado.UrlPerfil, subDocument.StatusCode);
        //            continue;
        //        };

        //        deputado.NomeParlamentar = subDocument
        //           .QuerySelector("h3.ls-title-3")
        //           .TextContent
        //           .Replace("Deputada", "")
        //           .Replace("Deputado", "")
        //           .Trim()
        //           .ToTitleCase();

        //        deputado.UrlFoto = (subDocument.QuerySelector("img.foto-deputado") as IHtmlImageElement)?.Source;

        //        ColetarDadosPerfil(deputado, subDocument);

        //        InsertOrUpdate(deputado);
        //    }
        //}

        public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
        {
            // http://www.al.ap.gov.br/pagina.php?pg=exibir_parlamentar&iddeputado=78
            var urlPerfil = (parlamentar.QuerySelector("a") as IHtmlAnchorElement).Href;
            var matricula = (int)Convert.ToInt32(urlPerfil.Split("iddeputado=")[1]);

            var deputado = GetDeputadoByMatriculaOrNew(matricula);

            deputado.NomeParlamentar = parlamentar
                .QuerySelector("p.ls-title-5")
                .TextContent
                .Replace("Deputada", "")
                .Replace("Deputado", "")
                .Trim()
                .ToTitleCase();

            deputado.UrlPerfil = urlPerfil;
            deputado.UrlFoto = (parlamentar.QuerySelector("img.foto-deputado") as IHtmlImageElement)?.Source;

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            List<string> perfil = subDocument
                .QuerySelectorAll(".ls-box")[2] // Perfil
                .QuerySelectorAll("p")
                .Select(x => x.TextContent.Trim())
                .ToList();

            for (int i = 0; i < perfil.Count; i += 2)
            {
                switch (perfil[i])
                {
                    case "Nome Civil:": deputado.NomeCivil = perfil[i + 1].ToTitleCase(); break;
                    case "E-mail:": deputado.Email = perfil[i + 1]; break;
                    case "Aniversário:": deputado.Nascimento = DateOnly.Parse(perfil[i + 1], cultureInfo); break;
                    case "Profissão:": deputado.Profissao = perfil[i + 1].ToTitleCase().NullIfEmpty(); break;
                    case "Partido:": deputado.IdPartido = BuscarIdPartido(perfil[i + 1]); break;
                    case "Legislatura(s):": return;
                    default: throw new Exception($"Verificar alterações na pagina! {deputado.UrlPerfil}");
                }
            }
        }
    }
}
