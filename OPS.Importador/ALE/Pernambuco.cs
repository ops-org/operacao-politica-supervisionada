using System;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.ALE;

/// <summary>
/// Assembleia Legislativa do Estado do Pernambuco
/// https://www.alepe.pe.gov.br/
/// </summary>
public class Pernambuco : ImportadorBase
{
    public Pernambuco(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarPernambuco(serviceProvider);
        //importadorDespesas = new ImportadorDespesasPernambuco(serviceProvider);
    }
}

public class ImportadorDespesasPernambuco : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasPernambuco(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "",
            Estado = Estado.Pernambuco,
            ChaveImportacao = ChaveDespesaTemp.CpfParcial
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        throw new NotImplementedException();
    }
}

public class ImportadorParlamentarPernambuco : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarPernambuco(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.alepe.pe.gov.br/parlamentares/",
            SeletorListaParlamentares = "#parlamentares-modo-fotos ul li",
            Estado = Estado.Pernambuco,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement item)
    {
        var nomeparlamentar = item.QuerySelector(".parlamentares-nome").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);
        deputado.UrlPerfil = (item.QuerySelector("a") as IHtmlAnchorElement).Href;

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        var cabecalho = subDocument.QuerySelector(".parlamentares-view-header");
        var nomeparlamentar = cabecalho.QuerySelector(".text .title").TextContent.Trim();

        deputado.IdPartido = BuscarIdPartido(cabecalho.QuerySelector(".text .subtitle").TextContent.Trim());

        var info1 = subDocument.QuerySelectorAll(".parlamentares-view-info dl.first dd");
        deputado.NomeCivil = info1[0].TextContent.Trim().ToTitleCase();
        deputado.Naturalidade = info1[1].TextContent.Trim();
        deputado.Email = info1[2].TextContent.Trim();
        deputado.Site = info1[3].TextContent.Trim();
        ImportacaoUtils.MapearRedeSocial(deputado, info1[4].QuerySelectorAll("a"));

        var info2 = subDocument.QuerySelectorAll(".parlamentares-view-info dl.last dd");
        //var aniversario = info2[0].TextContent.Trim(); // Falta ano
        deputado.Profissao = info2[1].TextContent.Trim().ToTitleCase();
        deputado.Telefone = info2[2].TextContent.Trim();
        //var gabinete = info2[3].TextContent.Trim();
    }
}
