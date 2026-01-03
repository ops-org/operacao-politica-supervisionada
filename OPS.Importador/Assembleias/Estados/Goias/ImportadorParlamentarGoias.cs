using System;
using System.Data;
using System.Linq;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Entity;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.Goias;

public class ImportadorParlamentarGoias : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarGoias(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://portal.al.go.leg.br/deputados/em-exercicio",
            SeletorListaParlamentares = "#tabela-deputados table tbody tr",
            Estado = Estado.Goias,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
    {
        var colunas = parlamentar.QuerySelectorAll("td");
        var urlPerfil = (colunas[0].QuerySelector("a") as IHtmlAnchorElement).Href;
        var matricula = Convert.ToUInt32(urlPerfil.Split(@"/").Last());

        var deputado = GetDeputadoByMatriculaOrNew(matricula);

        deputado.IdPartido = BuscarIdPartido(colunas[1].TextContent.Split('(')[1].Split(')')[0].Trim());
        deputado.UrlPerfil = urlPerfil;
        deputado.NomeParlamentar = colunas[0].TextContent.Trim().ToTitleCase().ReduceWhitespace();
        deputado.Telefone = string.Join(",", colunas[2].QuerySelectorAll("a").Select(x => x.TextContent.Trim()));

        if (string.IsNullOrEmpty(deputado.NomeCivil))
            deputado.NomeCivil = deputado.NomeParlamentar;

        ImportacaoUtils.MapearRedeSocial(deputado, colunas[3].QuerySelectorAll("a"));

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        deputado.UrlFoto = (subDocument.QuerySelector(".perfil__foto img") as IHtmlImageElement)?.Source;

        //var perfilPessoal = detalhes.QuerySelectorAll(".perfil-biografico__texto p")[0];
        //deputado.Naturalidade = null;
        //deputado.Nascimento = null;
        //deputado.Escolaridade = null;
        //deputado.Profissao = null;
    }
}
