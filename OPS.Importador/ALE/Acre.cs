using System;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

/// <summary>
/// https://www.al.ac.leg.br/
/// </summary>
public class Acre : ImportadorBase
{
    public Acre(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        //importadorParlamentar = new ImportadorParlamentarAcre(serviceProvider);
        //importadorDespesas = new ImportadorDespesasAcre(serviceProvider);
    }
}

public class ImportadorDespesasAcre : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasAcre(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://www.al.al.leg.br/transparencia/",
            Estado = Estado.Acre,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        logger.LogWarning("Usa PDF de baixa qualidade separados por Ano/Mes e Deputado!");
    }
}

public class ImportadorParlamentarAcre : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarAcre(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://sapl.al.ac.leg.br/parlamentar/",
            SeletorListaParlamentares = ".lista-parlamentares tbody tr",
            Estado = Estado.Acre,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement document)
    {
        var colunas = document.QuerySelectorAll("th");
        var colunaNome = colunas[0].QuerySelector("a");

        var nomeparlamentar = colunaNome.TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);
        deputado.UrlPerfil = (colunaNome as IHtmlAnchorElement).Href;
        deputado.IdPartido = BuscarIdPartido(colunas[1].TextContent.Trim());

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        var detalhes = subDocument.QuerySelector(".container");
        deputado.UrlFoto = detalhes.QuerySelector("img.img-thumbnail").Attributes["src"].Value;
        deputado.NomeCivil = detalhes.QuerySelector("#div_nome").TextContent.Replace("Nome Completo:", "").Trim().ToTitleCase();
        deputado.Email = detalhes.QuerySelectorAll(".row .col-sm-8>.form-group>p")[3].TextContent.Trim();
        deputado.Telefone = detalhes.QuerySelectorAll(".row .col-sm-8>.form-group>p")[2].TextContent.Trim();
    }
}
