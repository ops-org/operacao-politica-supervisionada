using System;
using System.Collections.Generic;
using System.Globalization;
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

public class Maranhao : ImportadorBase
{
    public Maranhao(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        //importadorParlamentar = new ImportadorParlamentarMaranhao(serviceProvider);
        //importadorDespesas = new ImportadorDespesasMaranhao(serviceProvider);
    }
}

public class ImportadorDespesasMaranhao : ImportadorDespesasRestApiMensal
{
    public ImportadorDespesasMaranhao(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://sistemas.al.ma.leg.br/transparencia/",
            Estado = Estado.Maranhao,
            ChaveImportacao = ChaveDespesaTemp.Matricula
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var address = $"{config.BaseAddress}lista-parlamentar.html";
        var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();

        IHtmlFormElement form = document.QuerySelector<IHtmlFormElement>("form.form-horizontal");
        //form.Action = $"/transparencia/lista-parlamentar.html?dswid={dswid}";

        var dcForm = new Dictionary<string, string>();
        dcForm.Add("in_competencia_input", $"{mes:00}/{ano:0000}");
        //dcForm.Add("javax.faces.ViewState", (document.QuerySelectorAll("input").First(x => x.Id == "j_id1:javax.faces.ViewState:0") as IHtmlInputElement).Value);
        var subDocument = form.SubmitAsync(dcForm).GetAwaiter().GetResult();

        var teste = subDocument.QuerySelector("#tabela");

        //foreach (var linha in linhasDespesas)
        //{
        //    var primeiraColuna = linha.QuerySelectorAll("td")[0];
        //    if (primeiraColuna.TextContent == "TOTAL") continue;

        //    var linkDetalhes = (primeiraColuna.QuerySelector("a") as IHtmlAnchorElement);
        //    var despesaTemp = new CamaraEstadualDespesaTemp()
        //    {
        //        Nome = gabinete.Text.ToTitleCase(),
        //        Cpf = deputado?.Matricula?.ToString(),
        //        Ano = (short)ano,
        //        Mes = (short)mes,
        //        TipoDespesa = linkDetalhes.Text.Split(" - ")[1].Trim(),
        //    };

        //    var subDocument = context.OpenAsyncAutoRetry(linkDetalhes.Href).GetAwaiter().GetResult();
        //    var linhasDespesasDetalhes = subDocument.QuerySelectorAll(".ls-table tbody tr");
        //    foreach (var detalhes in linhasDespesasDetalhes)
        //    {
        //        var colunas = detalhes.QuerySelectorAll("td");
        //        if (colunas[0].TextContent == "TOTAL") continue;

        //        var empresaParts = colunas[0].TextContent.Split(" - ");
        //        despesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(empresaParts[0].Trim());
        //        despesaTemp.Empresa = empresaParts[1].Trim();

        //        despesaTemp.Documento = colunas[2].TextContent.Trim();
        //        despesaTemp.Observacao = (colunas[2].QuerySelector("a") as IHtmlAnchorElement).Href;
        //        despesaTemp.Valor = Convert.ToDecimal(colunas[3].TextContent, cultureInfo);
        //        despesaTemp.DataEmissao = new DateTime(ano, mes, 1);

        //        InserirDespesaTemp(despesaTemp);
        //    }
        //}
    }
}

public class ImportadorParlamentarMaranhao : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarMaranhao(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.al.ma.leg.br/sitealema/deputados/",
            SeletorListaParlamentares = "section.section .news-card",
            Estado = Estado.Maranhao,
        });
    }


    public override DeputadoEstadual ColetarDadosLista(IElement item)
    {
        var nomeparlamentar = item.QuerySelector(".news-card-title").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);
        deputado.NomeCivil = nomeparlamentar;

        deputado.UrlPerfil = (item.QuerySelector(".news-card-title a") as IHtmlAnchorElement).Href;
        // deputado.UrlFoto = (item.QuerySelector("img") as IHtmlImageElement)?.Source;

        deputado.IdPartido = BuscarIdPartido(item.QuerySelector(".news-card-chapeu").TextContent.Trim());

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        deputado.UrlFoto = (subDocument.QuerySelector(".deputado img.img-full") as IHtmlImageElement)?.Source;

        var perfil = subDocument.QuerySelectorAll(".deputado>p")
            .Where(xx => xx.TextContent.Contains(":"))
            .Select(x => new { Key = x.TextContent.Split(':')[0].Trim(), Value = x.TextContent.Replace("Ramal:", "Ramal").Split(':')[1].Trim() });

        if (!string.IsNullOrEmpty(perfil.First(x => x.Key == "Aniversário").Value))
            deputado.Nascimento = DateOnly.Parse(perfil.First(x => x.Key == "Aniversário").Value);

        deputado.Profissao = perfil.First(x => x.Key == "Profissão").Value.ToTitleCase().NullIfEmpty();
        deputado.Site = perfil.First(x => x.Key == "Site").Value.NullIfEmpty();
        deputado.Email = perfil.First(x => x.Key == "E-mail").Value.NullIfEmpty() ?? deputado.Email;
        deputado.Telefone = perfil.First(x => x.Key == "Telefone").Value;

        // ImportacaoUtils.MapearRedeSocial(deputado, subDocument.QuerySelectorAll(".deputado ul a")); // Todos são as redes sociaos da AL
    }
}
