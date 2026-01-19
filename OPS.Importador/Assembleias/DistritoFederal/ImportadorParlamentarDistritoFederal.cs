using System.Globalization;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Parlamentar;

namespace OPS.Importador.Assembleias.DistritoFederal
{
    public class ImportadorParlamentarDistritoFederal : ImportadorParlamentarCrawler
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        public ImportadorParlamentarDistritoFederal(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            /// Atenção: Estruturas diferentes
            /// https://www.cl.df.gov.br/web/guest/dep-fora-exercicio
            /// https://www.cl.df.gov.br/web/guest/deputados-2023-2026
            /// https://www.cl.df.gov.br/web/guest/deputados-2019-2022
            /// https://www.cl.df.gov.br/web/guest/legislaturas-anteriores/-/asset_publisher/2jS3/content/-2015-2018-s-c3-a9tima-legislatura?_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_assetEntryId=10794633&_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_redirect=https%3A%2F%2Fwww.cl.df.gov.br%2Fweb%2Fguest%2Flegislaturas-anteriores%3Fp_p_id%3Dcom_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3%26p_p_lifecycle%3D0%26p_p_state%3Dnormal%26p_p_mode%3Dview%26_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_cur%3D0%26p_r_p_resetCur%3Dfalse%26_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_assetEntryId%3D10794633

            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                BaseAddress = "https://www.cl.df.gov.br/web/guest/deputados-2023-2026",
                SeletorListaParlamentares = ".deputados-interno div",
                Estado = Estados.DistritoFederal,
            });
        }

        public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
        {
            var nomeparlamentar = parlamentar.QuerySelector(".card-title").TextContent.Trim().ToTitleCase();
            var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

            deputado.UrlPerfil = (parlamentar.QuerySelector("a") as IHtmlAnchorElement).Href;
            var partido = parlamentar.QuerySelector(".card-text").TextContent.Trim();
            if (partido.Contains("("))
            {
                var arrPartidos = partido.Split(new[] { '(', ')' });
                partido = arrPartidos[arrPartidos[0].Length > arrPartidos[1].Length ? 1 : 0].Trim();
            }
            deputado.IdPartido = BuscarIdPartido(partido);

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            deputado.UrlFoto = (subDocument.QuerySelector(".informacoes-pessoais img") as IHtmlImageElement)?.Source;

            var detalhes = subDocument.QuerySelectorAll(".informacoes-pessoais .row .col-md-9 p span");

            if (string.IsNullOrEmpty(deputado.NomeCivil))
                deputado.NomeCivil = detalhes[0].TextContent.Trim().ToTitleCase();

            if (DateOnly.TryParse(detalhes[2].TextContent.Trim(), cultureInfo, out DateOnly nascimento))
            {
                deputado.Nascimento = nascimento;
                deputado.Profissao = detalhes[3].TextContent.Trim().ToTitleCase();
                deputado.Naturalidade = detalhes[1].TextContent.Trim();
            }
            else
            {
                deputado.Nascimento = DateOnly.Parse(detalhes[1].TextContent.Trim(), cultureInfo);
                deputado.Profissao = detalhes[2].TextContent.Trim().ToTitleCase();
            }

            //var gabinete = detalhes[3].TextContent.Trim();

            var rodape = subDocument.QuerySelectorAll(".journal-content-article .shortcut a b");
            if (rodape.Length == 0)
                rodape = subDocument.QuerySelectorAll(".journal-content-article .shortcut a h5");

            deputado.Telefone = rodape[0].TextContent.Trim();
            deputado.Email = rodape[1].TextContent.Trim();
        }

        //        public override async void ImportarParlamentares()
        //        {
        //            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
        //            var context = httpClient.CreateAngleSharpContext();

        //            using (var db = new AppDb())
        //            {
        //                //var address = $"https://www.cl.df.gov.br/web/guest/deputados-2019-2022";
        //                var address = $"https://www.cl.df.gov.br/web/guest/legislaturas-anteriores/-/asset_publisher/2jS3/content/-2015-2018-s-c3-a9tima-legislatura?_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_assetEntryId=10794633&_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_redirect=https%3A%2F%2Fwww.cl.df.gov.br%2Fweb%2Fguest%2Flegislaturas-anteriores%3Fp_p_id%3Dcom_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3%26p_p_lifecycle%3D0%26p_p_state%3Dnormal%26p_p_mode%3Dview%26_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_cur%3D0%26p_r_p_resetCur%3Dfalse%26_com_liferay_asset_publisher_web_portlet_AssetPublisherPortlet_INSTANCE_2jS3_assetEntryId%3D10794633";
        //                var document = await context.OpenAsyncAutoRetry(address);
        //                if (document.StatusCode != HttpStatusCode.OK)
        //                {
        //                    Console.WriteLine($"{address} {document.StatusCode}");
        //                };

        //                var parlamentares = document.QuerySelectorAll(".journal-content-article table p");
        //                foreach (var parlamentar in parlamentares)
        //                {
        //                    if (parlamentar.ToHtml() == "<p>&nbsp;</p>") continue;

        //                    var urlPerfil = (parlamentar.QuerySelector("a") as IHtmlAnchorElement).Href;
        //                    var nome = parlamentar.QuerySelector("strong").TextContent.Trim();
        //                    var partido = parlamentar.QuerySelector("p>span").TextContent.Trim();
        //                    if (partido.Contains("("))
        //                        partido = partido.Split(new[] { '(', ')' })[1];

        //                    //Thread.Sleep(TimeSpan.FromSeconds(15));
        //                    //var subDocument = await context.OpenAsyncAutoRetry(urlPerfil);
        //                    //if (document.StatusCode != HttpStatusCode.OK)
        //                    //{
        //                    //    Console.WriteLine($"{urlPerfil} {subDocument.StatusCode}");
        //                    //    continue;
        //                    //};

        //                    //var urlImagem = (subDocument.QuerySelector(".informacoes-pessoais img") as IHtmlImageElement)?.Source;

        //                    //var detalhes = subDocument.QuerySelectorAll(".informacoes-pessoais .row .col-md-9 p span");
        //                    //var nomeCivil = detalhes[0].TextContent.Trim();
        //                    //var naturalidade = detalhes[1].TextContent.Trim();
        //                    //var nascimento = Convert.ToDateTime(detalhes[2].TextContent.Trim(), cultureInfo);
        //                    //var profissao = detalhes[3].TextContent.Trim();
        //                    ////var gabinete = detalhes[3].TextContent.Trim();

        //                    //var rodape = subDocument.QuerySelectorAll(".journal-content-article a b");
        //                    //var telefone = rodape[0].TextContent.Trim();
        //                    //var email = rodape[1].TextContent.Trim();


        //                    db.AddParameter("@partido", partido);
        //                    //db.AddParameter("@nascimento", nascimento.ToString("yyyy-MM-dd"));
        //                    //db.AddParameter("@nomeCivil", nomeCivil);
        //                    //db.AddParameter("@email", email);
        //                    //db.AddParameter("@naturalidade", naturalidade);
        //                    //db.AddParameter("@escolaridade", profissao);
        //                    //db.AddParameter("@telefone", telefone);
        //                    db.AddParameter("@nome", nome);
        //                    db.ExecuteNonQuery(@"
        //update cl_deputado set 
        //    id_partido = (SELECT id FROM partido where sigla ILIKE @partido OR nome ILIKE @partido) -- ,
        //    -- nascimento = @nascimento, 
        //    -- nome_civil = @nomeCivil, 
        //    -- email = @email, 
        //    -- naturalidade = @naturalidade,
        //    -- escolaridade = @profissao,
        //    -- telefone = @telefone
        //where id_estado = 53
        //and nome_parlamentar = @nome");

        //                    if (db.RowsAffected != 1)
        //                        Console.WriteLine(nome);

        //                }
        //            }
        //        }
    }
}
