using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using OPS.Importador.Utilities;
using static OPS.Importador.ALE.ImportadorDespesasPernambuco;

namespace OPS.Importador.ALE;

public class EspiritoSanto : ImportadorBase
{
    public EspiritoSanto(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarEspiritoSanto(serviceProvider);
        importadorDespesas = new ImportadorDespesasEspiritoSanto(serviceProvider);
    }
}

public class ImportadorDespesasEspiritoSanto : ImportadorDespesasRestApiMensal
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
    private readonly List<DeputadoEstadual> deputados;

    public ImportadorDespesasEspiritoSanto(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://www.al.es.gov.br",
            Estado = Estado.EspiritoSanto,
            ChaveImportacao = ChaveDespesaTemp.Gabinete
        };

        // TODO: Filtrar legislatura atual
        deputados = connection.GetList<DeputadoEstadual>(new { id_estado = config.Estado.GetHashCode() }).ToList();
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var address = $"{config.BaseAddress}/Transparencia/CotasParlamentares";
        var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();
        var gabinetes = document.QuerySelectorAll("#cboSetor option").ToList();

        foreach (var item in gabinetes)
        {
            var gabinete = item as IHtmlOptionElement;
            if (gabinete.Value == "0") continue;

            var deputado = deputados.Find(x => x.Gabinete.ToString() == gabinete.Value);
            if (deputado == null)
            {
                deputado = deputados.Find(x => gabinete.Text.Replace("Gab. Dep. ", "").Contains(x.NomeParlamentar.ToString()));

                if (deputado != null)
                {
                    deputado.Gabinete = Convert.ToUInt32(gabinete.Value);
                    connection.Update(deputado);
                }
                else
                    logger.LogError($"Deputado {gabinete.Value}: {gabinete.Text} não existe ou não possui gabinete relacionado!");
            }

            address = $"{config.BaseAddress}/Transparencia/Api/CotasParlamentaresNovoTable/{mes}/{mes}/{ano}/{gabinete.Value}/false";
            document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();

            if (document.QuerySelector(".ls-alert-warning")?.TextContent == "Nenhum: resultado foi encontrado!") continue;

            var rows = document.QuerySelectorAll("#tabelaMobile table tr");
            for (var i = 0; i < rows.Count() - 1; i++)
            {
                if (rows[i].QuerySelectorAll("td")[0].TextContent.Trim() != "Produto:") continue;
                if (rows[i + 2].QuerySelectorAll("td")[1].TextContent.Trim() == "-") continue;

                var despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Nome = gabinete.Text.Replace("Gab. Dep. ", ""),
                    Cpf = gabinete.Value,
                    Ano = (short)ano,
                    Mes = (short)mes,
                    DataEmissao = new DateTime(ano, mes, 1),
                    Valor = Convert.ToDecimal(rows[i + 2].QuerySelectorAll("td")[1].TextContent.Replace("R$ ", "").Trim(), cultureInfo),
                    Observacao = "Quantidade: " + rows[i + 1].QuerySelectorAll("td")[1].TextContent.Trim(),
                    TipoDespesa = rows[i].QuerySelectorAll("td")[1].TextContent.Trim(),
                };

                if (despesaTemp.Valor > 0)
                    InserirDespesaTemp(despesaTemp);
            }
        }
    }
}

public class ImportadorParlamentarEspiritoSanto : ImportadorParlamentarCrawler
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorParlamentarEspiritoSanto(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.al.es.gov.br/Deputado/Lista",
            SeletorListaParlamentares = "#divListaDeputados>div",
            Estado = Estado.EspiritoSanto,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement item)
    {
        var nomeparlamentar = item.QuerySelector(".nomeDeputadoLista").TextContent.Replace("(licenciado)", "").Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlPerfil = (item.QuerySelector("a.linkLimpo") as IHtmlAnchorElement).Href;
        deputado.UrlFoto = (item.QuerySelector("#divImagemDeputado img") as IHtmlImageElement)?.Source;

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        var dados = subDocument
            .QuerySelectorAll(".fonte-dados-deputado>div")
            .Select(x => new { Key = x.QuerySelector("label").TextContent.Trim(), Value = x.QuerySelector("span").TextContent.Trim() });

        if (!string.IsNullOrEmpty(dados.First(x => x.Key == "Partido:").Value))
            deputado.IdPartido = BuscarIdPartido(dados.First(x => x.Key == "Partido:").Value);

        deputado.NomeCivil = dados.FirstOrDefault(x => x.Key == "Nome Civil:")?.Value;
        deputado.Naturalidade = dados.FirstOrDefault(x => x.Key == "Naturalidade:")?.Value;
        deputado.Nascimento = DateOnly.Parse(dados.First(x => x.Key == "Data de Nascimento:").Value, cultureInfo);
        deputado.Telefone = dados.FirstOrDefault(x => x.Key == "Telefone:")?.Value;
        deputado.Email = dados.FirstOrDefault(x => x.Key == "E-mail:")?.Value;
    }
}
