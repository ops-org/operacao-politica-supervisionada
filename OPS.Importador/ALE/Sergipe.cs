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
using Tabula.Detectors;
using Tabula.Extractors;
using Tabula;
using UglyToad.PdfPig;
using System.IO;
using OfficeOpenXml.Drawing.Chart;

namespace OPS.Importador.ALE;

public class Sergipe : ImportadorBase
{
    public Sergipe(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarSergipe(serviceProvider);
        importadorDespesas = new ImportadorDespesasSergipe(serviceProvider);
    }
}

public class ImportadorDespesasSergipe : ImportadorDespesasRestApiAnual
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasSergipe(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://al.se.leg.br/portal-da-transparencia/despesas/ressarcimento-dos-deputados/",
            Estado = Estado.Sergipe,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        var today = DateTime.Today;
        var document = context.OpenAsyncAutoRetry(config.BaseAddress).GetAwaiter().GetResult();

        var anoSelecionado = document
            .QuerySelectorAll(".elementor-widget-wrap .elementor-widget-heading")
            .First(x => x.QuerySelector(".elementor-heading-title").TextContent == ano.ToString())
            .NextElementSibling;

        var meses = anoSelecionado.QuerySelectorAll(".elementor-tabs-wrapper .elementor-tab-title");
        foreach (var item in meses)
        {
            var mesExtenso = item.TextContent;
            var competencia = new DateTime(ano, ResolveMes(mesExtenso), 1).AddMonths(-1);
            if (competencia.AddMonths(1) > today) continue;

            using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = competencia.Month }))
            {
                var urlPdf = anoSelecionado.QuerySelector("#" + item.Attributes["aria-controls"].Value + " .wpdm-download-link")?.Attributes["data-downloadurl"]?.Value;
                if (string.IsNullOrEmpty(urlPdf))
                {
                    var aviso = anoSelecionado.QuerySelector("#" + item.Attributes["aria-controls"].Value + " div[id^=content_wpdm_package_]").TextContent.Trim();
                    logger.LogWarning("Despesas indisponiveis para {Mes:00}/{Ano}. Detalhes: {Detalhes}", competencia.Month, competencia.Year, aviso);
                    continue;
                }

                // Coletar dados apenas a partir de 2023
                if (competencia.Year == 2022) continue;

                using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = competencia.Month, ["Url"] = urlPdf, ["Arquivo"] = $"CLSE-{ano}-{competencia.Month}.pdf" }))
                {
                    ImportarDespesasArquivo(competencia.Year, competencia.Month, urlPdf);
                }
            }
        }
    }

    private int ResolveMes(string mes) => mes switch
    {
        "Janeiro" => 1,
        "Fevereiro" => 2,
        "Março" => 3,
        "Abril" => 4,
        "Maio" => 5,
        "Junho" => 6,
        "Julho" => 7,
        "Agosto" => 8,
        "Setembro" => 9,
        "Outubro" => 10,
        "Novembro" => 11,
        "Dezembro" => 12,
        _ => throw new ArgumentOutOfRangeException(nameof(mes), $"Mês invalido: {mes}"),
    };

    private void ImportarDespesasArquivo(int ano, int mes, string urlPdf)
    {
        var filename = $"{tempPath}/CLSE-{ano}-{mes}.pdf";
        BaixarArquivo(urlPdf, filename);

        using (PdfDocument document = PdfDocument.Open(filename, new ParsingOptions() { ClipPaths = true }))
        {
            ObjectExtractor oe = new ObjectExtractor(document);

            // detect canditate table zones
            SimpleNurminenDetectionAlgorithm detector = new SimpleNurminenDetectionAlgorithm();
            IExtractionAlgorithm ea = new BasicExtractionAlgorithm();
            //IExtractionAlgorithm ea = new SpreadsheetExtractionAlgorithm();

            decimal valorTotalDeputado = 0;
            string nomeParlamentar = "";
            DateTime competencia = DateTime.MinValue;
            var totalValidado = true;
            var despesasIncluidas = 0;
            for (var p = 1; p <= document.NumberOfPages; p++)
            {
                PageArea page = oe.Extract(p);
                var regions = detector.Detect(page);
                List<Table> tables = ea.Extract(page); // .GetArea(regions[0].BoundingBox) // take first candidate area

                foreach (var table in tables)
                {
                    foreach (var row in table.Rows)
                    {
                        var numColunas = row.Count;
                        var itemDescicaoTemp = row[0].GetText();
                        var valorTemp = row[numColunas == 2 ? 1 : 2].GetText(); // Pode haver 3 colunas, quando isso ocorre o valor estão na última.

                        if (itemDescicaoTemp.StartsWith("Deputado:"))
                        {
                            nomeParlamentar = itemDescicaoTemp.Split(":")[1].Trim();
                            competencia = Convert.ToDateTime(valorTemp.Split(":")[1].Trim(), cultureInfo);
                            valorTotalDeputado = 0;

                            if (competencia.Year == 2022) break;

                            if (!totalValidado)
                                logger.LogError("Valor total não validado!");
                            else
                                totalValidado = false;

                            continue;
                        }

                        if (valorTemp.StartsWith("Total:"))
                        {
                            totalValidado = true;
                            var valorTotalArquivo = Convert.ToDecimal(valorTemp.Split(":")[1].Replace("R$", "").Trim(), cultureInfo);
                            if (valorTotalDeputado != valorTotalArquivo)
                                logger.LogError("Valor Divergente! Esperado: {ValorTotalArquivo}; Encontrado: {ValorTotalDeputado}; Diferenca: {Diferenca}",
                                    valorTotalArquivo, valorTotalDeputado, valorTotalArquivo - valorTotalDeputado);

                            continue;
                        }

                        if (string.IsNullOrEmpty(itemDescicaoTemp) ||
                            string.IsNullOrEmpty(valorTemp) ||
                            valorTemp.StartsWith("Valor") ||
                            valorTemp.StartsWith("Página")) continue;

                        CamaraEstadualDespesaTemp despesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Nome = nomeParlamentar,
                            Ano = (short)competencia.Year,
                            Mes = (short)competencia.Month,
                            DataEmissao = competencia,
                            Valor = Convert.ToDecimal(valorTemp.Replace("R$", "").Trim(), cultureInfo),
                        };

                        if (itemDescicaoTemp.Contains("-"))
                            despesaTemp.TipoDespesa = itemDescicaoTemp.Split("-")[1].Trim();
                        else
                            despesaTemp.TipoDespesa = Core.Utils.RemoveCaracteresNumericos(itemDescicaoTemp).Trim();

                        //logger.LogWarning($"Inserindo Item {itemDescicaoTemp.Split("Art")[0]} com valor: {despesaTemp.Valor}!");
                        valorTotalDeputado += despesaTemp.Valor;
                        despesasIncluidas++;

                        InserirDespesaTemp(despesaTemp);
                    }
                }
            }

            if (!totalValidado)
                logger.LogError("Valor total não validado!");
        }
    }

    public override void DefinirCompetencias(int ano)
    {
        competenciaInicial = $"{ano - 1}12";
        competenciaFinal = $"{ano}11";
    }
}

public class ImportadorParlamentarSergipe : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarSergipe(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://aleselegis.al.se.leg.br/spl/parlamentares.aspx", //?leg=19
            SeletorListaParlamentares = "#ContentPlaceHolder1_parlamentares_lista .custom-user-profile",
            Estado = Estado.Sergipe,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement document)
    {
        var nomeparlamentar = document.QuerySelector("a.kt-widget__username").TextContent.Trim().ToTitleCase();
        var deputado = GetDeputadoByNameOrNew(nomeparlamentar);

        deputado.UrlPerfil = (document.QuerySelector("a.kt-widget__username") as IHtmlAnchorElement).Href;
        deputado.UrlFoto = (document.QuerySelector("img.kt-widget__img") as IHtmlImageElement)?.Source;
        deputado.Matricula = Convert.ToUInt32(deputado.UrlPerfil.Split(@"=").Last());
        deputado.IdPartido = BuscarIdPartido(document.QuerySelector("span.kt-widget__username").TextContent.Replace("(", "").Replace(")", "").Trim());

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument document)
    {

        var perfil = document
            .QuerySelectorAll("#dados_parlamentar .kt-widget__info")
            .Select(x => new { Key = x.QuerySelector(".kt-widget__label").TextContent.Trim(), Value = x.QuerySelector(".kt-widget__data").TextContent.Trim() })
            .ToList();

        if (string.IsNullOrEmpty(deputado.NomeCivil))
            deputado.NomeCivil = perfil.FirstOrDefault(x => x.Key == "Nome civil:")?.Value;

        deputado.Telefone = perfil.FirstOrDefault(x => x.Key == "Telefone(s):")?.Value;
        //deputado.Celular = perfil.FirstOrDefault(x => x.Key == "Celular:")?.Value;
        deputado.Email = perfil.FirstOrDefault(x => x.Key == "E-mail:")?.Value;

        ImportacaoUtils.MapearRedeSocial(deputado, document.QuerySelectorAll("#dados_parlamentar_links a"));
        deputado.Site = null;
    }
}
