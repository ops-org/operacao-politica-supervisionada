using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Comum.Parlamentar;
using OPS.Importador.Comum.Utilities;

namespace OPS.Importador.Assembleias.Parana
{
    public class ImportadorParlamentarParana : ImportadorParlamentarCrawler
    {
        private readonly List<DeputadoEstadual> deputados;

        public ImportadorParlamentarParana(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Configure(new ImportadorParlamentarCrawlerConfig()
            {
                // https://www.assembleia.pr.leg.br/deputados/legislatura/19a-legislatura-3a-e-4a-sessoes-legislativas
                BaseAddress = "https://www.assembleia.pr.leg.br/deputados/conheca",
                SeletorListaParlamentares = ".pg-deps .dep",
                Estado = Estados.Parana,
            });

            deputados = dbContext.DeputadosEstaduais.Where(x => x.IdEstado == config.Estado.GetHashCode()).ToList();
        }

        public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
        {
            var nomeParlamentar = parlamentar.QuerySelector(".nome-deps span").TextContent.Trim().ReduceWhitespace();
            var nomeParlamentarLimpo = Utils.RemoveAccents(nomeParlamentar);
            var urlPerfil = (parlamentar.QuerySelector("a") as IHtmlAnchorElement).Href;

            var deputado = deputados.Find(x =>
                Utils.RemoveAccents(x.NomeImportacao ?? "").Equals(nomeParlamentarLimpo, StringComparison.InvariantCultureIgnoreCase) ||
                Utils.RemoveAccents(x.NomeParlamentar).Equals(nomeParlamentarLimpo, StringComparison.InvariantCultureIgnoreCase) ||
                Utils.RemoveAccents(x.NomeCivil ?? "").Equals(nomeParlamentarLimpo, StringComparison.InvariantCultureIgnoreCase) ||
                x.UrlPerfil == urlPerfil
            );

            if (deputado == null)
            {
                deputado = new DeputadoEstadual()
                {
                    NomeParlamentar = nomeParlamentar,
                    IdEstado = (byte)config.Estado.GetHashCode()
                };
            }

            if (string.IsNullOrEmpty(deputado.NomeCivil))
                deputado.NomeCivil = deputado.NomeParlamentar;

            deputado.IdPartido = BuscarIdPartido(parlamentar.QuerySelector(".nome-deps .partido").TextContent.Trim());
            deputado.UrlPerfil = urlPerfil;
            deputado.UrlFoto = (parlamentar.QuerySelector(".foto-deps img") as IHtmlImageElement)?.Source;

            return deputado;
        }

        public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
        {
            deputado.Telefone = subDocument.QuerySelector(".tel span")?.TextContent;

            var redesSociais = subDocument.QuerySelectorAll(".compartilhamento-redes a,.link-site");
            if (redesSociais.Length > 0)
                ImportacaoUtils.MapearRedeSocial(deputado, redesSociais);
        }
    }
}
