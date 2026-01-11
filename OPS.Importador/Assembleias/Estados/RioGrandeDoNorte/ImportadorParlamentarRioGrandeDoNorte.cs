using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoNorte
{
    public class ImportadorParlamentarRioGrandeDoNorte : ImportadorParlamentarCrawler
    {

        public ImportadorParlamentarRioGrandeDoNorte(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                BaseAddress = "https://www.al.rn.leg.br/deputados",
                SeletorListaParlamentares = "#deputados .card-deputies",
                Estado = Estado.RioGrandeDoNorte,
            });
        }

        ///// <summary>
        ///// Forçar importação de parlamentares fora de exercício, mas que exerceram na legislatura.
        ///// </summary>
        ///// <returns></returns>
        //public override async Task Importar()
        //{
        //    ArgumentNullException.ThrowIfNull(config, nameof(config));

        //    var context = httpClient.CreateAngleSharpContext();

        //    var parlamentares = new[]
        //    {
        //        "https://www.al.rn.leg.br/deputado/118/Albert%20Dickso",
        //        "https://www.al.rn.leg.br/deputado/75/getulio-reg",
        //        "https://www.al.rn.leg.br/deputado/129/jaco-jacom",
        //        "https://www.al.rn.leg.br/deputado/104/Kelps%20Lim",
        //        "https://www.al.rn.leg.br/deputado/87/raimundo-fernande",
        //        "https://www.al.rn.leg.br/deputado/113/Souza%20Net",
        //        "https://www.al.rn.leg.br/deputado/128/subtenente-eliab",
        //        "https://www.al.rn.leg.br/deputado/92/Vivaldo%20Cost",
        //        "https://www.al.rn.leg.br/deputado/132/dr-kerginaldo-jacom",
        //    };

        //    foreach (var urlPerfil in parlamentares)
        //    {
        //        var document = await context.OpenAsyncAutoRetry(urlPerfil);
        //        if (document.StatusCode != HttpStatusCode.OK)
        //        {
        //            Console.WriteLine($"{config.BaseAddress} {document.StatusCode}");
        //        };

        //        var nomeparlamentar = document.QuerySelector(".body-new strong.title-new").TextContent.Trim().ToTitleCase();
        //        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);
        //        deputado.NomeCivil = nomeparlamentar;

        //        deputado.UrlPerfil = urlPerfil;
        //        if (deputado.Matricula == null)
        //            deputado.Matricula = Convert.ToInt32(urlPerfil.Split("/")[4]); // https://www.al.rn.leg.br/deputado/<matricula>/<slug>

        //        deputado.UrlFoto = (document.QuerySelector(".section-new img.img-fluid") as IHtmlImageElement)?.Source;

        //        var perfil = (document.QuerySelector(".section-new table") as IHtmlTableElement).Rows
        //            .Select(x => new { Key = x.Cells[0].TextContent.Trim(), Value = x.Cells[1].TextContent.Trim() });

        //        deputado.IdPartido = BuscarIdPartido(perfil.First(x => x.Key == "Partido:").Value);

        //        if (!string.IsNullOrEmpty(perfil.First(x => x.Key == "Data de Nascimento:").Value))
        //            deputado.Nascimento = DateOnly.Parse(perfil.First(x => x.Key == "Data de Nascimento:").Value);

        //        deputado.Telefone = perfil.First(x => x.Key == "Telefone de Contato:").Value;
        //        deputado.Email = perfil.First(x => x.Key == "E-mail:").Value.NullIfEmpty() ?? deputado.Email;

        //        InsertOrUpdate(deputado);
        //    }

        //    logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
        //}

        public override DeputadoEstadual ColetarDadosLista(IElement item)
        {
            var nomeparlamentar = item.QuerySelector(".name-deputies").TextContent.Trim().ToTitleCase();
            var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

            if (string.IsNullOrEmpty(deputado.NomeCivil))
                deputado.NomeCivil = deputado.NomeParlamentar;

            deputado.UrlPerfil = (item as IHtmlAnchorElement).Href;
            if (deputado.Matricula == null)
                deputado.Matricula = Convert.ToInt32(deputado.UrlPerfil.Split("/")[4]); // https://www.al.rn.leg.br/deputado/<matricula>/<slug>
                                                                                        // deputado.UrlFoto = (item.QuerySelector("img") as IHtmlImageElement)?.Source;
                                                                                        //deputado.IdPartido = BuscarIdPartido(item.QuerySelector(".party-deputies").TextContent.Trim());

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            deputado.UrlFoto = (subDocument.QuerySelector(".section-new img.img-fluid") as IHtmlImageElement)?.Source;

            var perfil = (subDocument.QuerySelector(".section-new table") as IHtmlTableElement).Rows
                .Select(x => new { Key = x.Cells[0].TextContent.Trim(), Value = x.Cells[1].TextContent.Trim() });

            deputado.IdPartido = BuscarIdPartido(perfil.First(x => x.Key == "Partido:").Value);

            if (!string.IsNullOrEmpty(perfil.First(x => x.Key == "Data de Nascimento:").Value))
                deputado.Nascimento = DateOnly.Parse(perfil.First(x => x.Key == "Data de Nascimento:").Value);

            deputado.Telefone = perfil.First(x => x.Key == "Telefone de Contato:").Value;
            deputado.Email = perfil.First(x => x.Key == "E-mail:").Value.NullIfEmpty() ?? deputado.Email;


            // ImportacaoUtils.MapearRedeSocial(deputado, subDocument.QuerySelectorAll(".deputado ul a")); // Todos são as redes sociaos da AL
        }
    }
}
