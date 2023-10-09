using System;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.ALE;

public class Alagoas : ImportadorBase
{
    public Alagoas(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarAlagoas(serviceProvider);
        //importadorDespesas = new ImportadorDespesasAlagoas(serviceProvider);
    }
}

public class ImportadorDespesasAlagoas : ImportadorDespesasRestApiMensal
{
    public ImportadorDespesasAlagoas(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "http://app.al.ac.leg.br/financa/despesaVI",
            Estado = Estado.Alagoas,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        logger.LogWarning("API Sem dados detalhados!");
    }
}

public class ImportadorParlamentarAlagoas : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarAlagoas(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.al.al.leg.br/processo-legislativo/20-a-legislatura",
            SeletorListaParlamentares = "#content-core table tbody tr",
            Estado = Estado.Alagoas,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement document)
    {
        var colunas = document.QuerySelectorAll("td");
        if (colunas.Length < 3 || !colunas[0].HasChildNodes || string.IsNullOrEmpty(colunas[1].TextContent.Trim())) return null;

        var colunaDetalhes = colunas[1].QuerySelectorAll("p");
        var colunaNome = colunaDetalhes[0].QuerySelector("a");

        var nomeparlamentar = colunaNome.TextContent.Trim();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        var elementoNome = colunaDetalhes[1].QuerySelector("em,i");
        if (elementoNome != null)
            deputado.NomeParlamentar = elementoNome.TextContent.Trim();
        else
            deputado.NomeParlamentar = colunaDetalhes[1].TextContent.Replace("Nome Civil:", "").Trim();

        deputado.UrlFoto = (colunaDetalhes[0].QuerySelector("img") as IHtmlImageElement)?.Source;
        deputado.UrlPerfil = (colunaNome as IHtmlAnchorElement).Href;
        deputado.IdPartido = BuscarIdPartido(colunaDetalhes[2].TextContent.Replace("Partido:", "", StringComparison.InvariantCultureIgnoreCase).Trim());
        deputado.Email = colunaDetalhes[3].TextContent.Replace("E-mail:", "", StringComparison.InvariantCultureIgnoreCase).Trim();

        ImportacaoUtils.MapearRedeSocial(deputado, colunaDetalhes[4].QuerySelectorAll("a"));

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    { }
}
