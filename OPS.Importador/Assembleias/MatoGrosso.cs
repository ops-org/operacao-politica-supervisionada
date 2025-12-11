using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Comum;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias;

public class MatoGrosso : ImportadorBase
{
    public MatoGrosso(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarMatoGrosso(serviceProvider);
        importadorDespesas = new ImportadorDespesasMatoGrosso(serviceProvider);
    }
}

public class ImportadorDespesasMatoGrosso : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasMatoGrosso(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://almt.eloweb.net/portaltransparencia-api/empenhos",
            Estado = Estado.MatoGrosso,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        var address = $"{config.BaseAddress}/lista?search=id.exercicio==%272025%27%20and%20id.entidade==%271%27%20and%20data%3E=%272025-01-01%27%20and%20data%3C=%272025-12-31%27&programatica.projeto=4491&programatica.elemento=339093&entidade=1&size=20&sort=data,DESC";
        EmpenhoResponse objEmpenhos = RestApiGet<EmpenhoResponse>(address);


        foreach (var empenho in objEmpenhos.Content)
        {
            var objCamaraEstadualDespesaTemp = new CamaraEstadualDespesaTemp()
            {
                Ano = (short)ano,
                Mes = (short)empenho.Data.Month,
                TipoDespesa = "Indenizações e Restituições",
                Valor = empenho.ValorPago,
                DataEmissao = empenho.Data
            };

            InserirDespesaTemp(objCamaraEstadualDespesaTemp);
        }
    }

    public class EmpenhoResponse
    {
        public List<Empenho> Content { get; set; }
    }

    public class Empenho
    {
        public int Entidade { get; set; }
        public int EmpenhoNum { get; set; }
        public int Exercicio { get; set; }
        public DateTime Data { get; set; }
        public int Fornecedor { get; set; }
        public string Nome { get; set; }
        public string Historico { get; set; }
        public decimal ValorEmpenhado { get; set; }
        public decimal ValorAnulado { get; set; }
        public decimal ValorLiquidado { get; set; }
        public decimal ValorRetido { get; set; }
        public decimal ValorPago { get; set; }
        public decimal ValorAPagar { get; set; }
    }
}

public class ImportadorParlamentarMatoGrosso : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarMatoGrosso(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.al.mt.gov.br/parlamento/deputados",
            SeletorListaParlamentares = "main .card",
            Estado = Estado.MatoGrosso,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement item)
    {
        var nomeparlamentar = item.QuerySelector(".card-body .card-title").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlPerfil = (item.QuerySelector(">a") as IHtmlAnchorElement).Href;
        deputado.IdPartido = BuscarIdPartido(item.QuerySelector(".card-body .badge").TextContent.Trim());

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        deputado.UrlFoto = (subDocument.QuerySelector("main a>img") as IHtmlImageElement)?.Source;

        var perfil = subDocument.QuerySelectorAll("ul.list-group>li")
            .Where(xx => xx.TextContent.Contains(":"))
            .Select(x => new { Key = x.TextContent.Split(':')[0].Trim(), Value = x.TextContent.Split(':')[1].Trim() });

        deputado.NomeCivil = perfil.First(x => x.Key == "Nome civil").Value.ToTitleCase().NullIfEmpty();

        ImportacaoUtils.MapearRedeSocial(deputado, subDocument.QuerySelectorAll("ul.nav a")); // Todos são as redes sociaos da AL
    }
}
