using System;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core;
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
        importadorDespesas = new ImportadorDespesasAlagoas(serviceProvider);
    }
}

public class ImportadorDespesasAlagoas : ImportadorDespesasRestApiMensal
{
    public ImportadorDespesasAlagoas(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://www.al.al.leg.br/transparencia/orcamento-e-financas/viap-verba-indenizatoria-de-atividade-parlamentar",
            Estado = Estado.Alagoas,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        logger.LogWarning("Dados em PDF scaneado e de baixa qualidade!");
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

        var colunaDetalhes = colunas[1].QuerySelectorAll("p").Where(x => !string.IsNullOrEmpty(x.TextContent.Trim())).ToList();
        var colunaNome = colunaDetalhes[0].QuerySelector("a");

        var nomeparlamentar = colunaNome.TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.NomeCivil = colunaDetalhes[1].TextContent.Replace("Nome Civil:", "").Trim().ToTitleCase();
        deputado.UrlFoto = (colunaDetalhes[0].QuerySelector("img") as IHtmlImageElement)?.Source;
        deputado.UrlPerfil = (colunaNome as IHtmlAnchorElement).Href;
        deputado.IdPartido = BuscarIdPartido(colunaDetalhes[2].TextContent.Replace("Partido:", "", StringComparison.InvariantCultureIgnoreCase).Trim());
        deputado.Email = colunaDetalhes[3].TextContent.Replace("E-mail:", "", StringComparison.InvariantCultureIgnoreCase).Trim();

        if (colunaDetalhes.Count > 4)
            ImportacaoUtils.MapearRedeSocial(deputado, colunaDetalhes[4].QuerySelectorAll("a"));

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    { }
}
