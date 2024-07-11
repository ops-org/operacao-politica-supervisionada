using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using CsvHelper;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

public class Piaui : ImportadorBase
{
    public Piaui(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarPiaui(serviceProvider);
        importadorDespesas = new ImportadorDespesasPiaui(serviceProvider);
    }
}

/// <summary>
/// https://transparencia.al.pi.leg.br/grid_transp_publico_gecop/
/// Importação parcialmente manual:
/// 1. Selecionar ultima legislatura
/// 2. Fazer download do CSV
/// 3. Mover arquivo para a pasta temp
/// </summary>
public class ImportadorDespesasPiaui : ImportadorDespesasArquivo
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasPiaui(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://transparencia.al.pi.leg.br/",
            Estado = Estado.Piaui,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void Importar(int ano)
    {
        // TODO: Criar importação por legislatura
        if (ano != 2023)
            throw new BusinessException("Importação já realizada");

        logger.LogWarning("Despesas do(a) {idEstado}:{CasaLegislativa} de {Ano}", config.Estado.GetHashCode(), config.Estado.ToString(), ano);

        CarregarHashes(ano);
        LimpaDespesaTemporaria();

        var caminhoArquivo = $"{tempPath}/grid_transp_publico_gecop.csv";
        using (var reader = new StreamReader(caminhoArquivo, Encoding.GetEncoding("UTF-8")))
        {
            using (var csv = new CsvReader(reader, CultureInfo.CreateSpecificCulture("pt-BR")))
            {
                while (csv.Read())
                {
                    var despesaTemp = new CamaraEstadualDespesaTemp();
                    despesaTemp.DataEmissao = Convert.ToDateTime("01/" + csv[ColunasDespesas.Competencia.GetHashCode()], cultureInfo);
                    despesaTemp.Ano = (short)despesaTemp.DataEmissao.Year;
                    despesaTemp.Mes = (short)despesaTemp.DataEmissao.Month;
                    despesaTemp.Nome = csv[ColunasDespesas.Parlamentar.GetHashCode()].Trim().ToTitleCase();
                    despesaTemp.TipoDespesa = csv[ColunasDespesas.Subcota.GetHashCode()].Trim();
                    despesaTemp.Valor = Convert.ToDecimal(csv[ColunasDespesas.Valor.GetHashCode()], cultureInfo);

                    InserirDespesaTemp(despesaTemp);
                }
            }
        }

        ProcessarDespesas(ano);
    }

    public override string SqlCarregarHashes(int ano)
    {
        return $"select d.id, d.hash from cl_despesa d join cl_deputado p on d.id_cl_deputado = p.id where p.id_estado = {idEstado} and d.ano_mes between {ano}01 and {ano+4}12";
    }

    public override int ContarRegistrosBaseDeDadosFinal(int ano)
    {
        return connection.ExecuteScalar<int>(@$"
select count(1) 
from cl_despesa d 
join cl_deputado p on p.id = d.id_cl_deputado 
where p.id_estado = {idEstado}
and d.ano_mes between {ano}01 and {ano+4}12");
    }

    public override void ImportarDespesas(string caminhoArquivo, int ano)
    {
        throw new NotImplementedException();
    }

    private enum ColunasDespesas
    {
        Legislatura,
        Parlamentar,
        Competencia,
        Subcota,
        Valor
    }

    //public override void ImportarDespesas(IBrowsingContext context, int ano)
    //{
    //    var configuration = AngleSharp.Configuration.Default
    //            .With(new HttpClientRequester(httpClient))
    //            .WithDefaultLoader(new LoaderOptions { IsResourceLoadingEnabled = true })
    //            .WithJs()
    //            .WithDefaultCookies()
    //            .WithCulture("pt-BR");

    //    context = BrowsingContext.New(configuration);


    //    var address = $"{config.BaseAddress}grid_transp_publico_gecop/";
    //    var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();

    //    //var filtroLegislaturaAtual = (document.QuerySelectorAll(".simplemv_legis a.scGridRefinedSearchCampoFont").First(x => x.TextContent.Trim() == "20a Legislatura (2023 a 2027)") as IHtmlAnchorElement);
    //    //filtroLegislaturaAtual.DoClick();

    //    while (true)
    //    {
    //        var pagina = document.QuerySelector(".scGridToolbarNavOpen").TextContent;
    //        Console.WriteLine($"Consultando pagina {pagina}");

    //        var rows = document.QuerySelectorAll("#sc_grid_body>table>tbody");
    //        var despesaTemp = new CamaraEstadualDespesaTemp();
    //        foreach (var row in rows)
    //        {
    //            if (row.Id.EndsWith("_bot"))
    //            {
    //                var despesas = row.QuerySelectorAll("tr");
    //                foreach (var despesa in despesas)
    //                {
    //                    var colunas = despesa.QuerySelectorAll(">td");
    //                    if (colunas.Length != 8) continue; // Ignorar linha de detalhamento;
    //                    var parlamentar = colunas[ColunasDespesas.Parlamentar.GetHashCode()].TextContent.Trim();
    //                    if (parlamentar == "Competência" || colunas.Any(x=> x.TextContent.Trim() == "Docto")) continue;

    //                    despesaTemp.DataEmissao = Convert.ToDateTime("01/" + colunas[ColunasDespesas.Competencia.GetHashCode()].TextContent, cultureInfo);
    //                    despesaTemp.Ano = (short)ano;
    //                    despesaTemp.Mes = (short)despesaTemp.DataEmissao.Month;
    //                    despesaTemp.Nome = parlamentar.Trim().ToTitleCase();
    //                    despesaTemp.TipoDespesa = colunas[ColunasDespesas.Subcota.GetHashCode()].TextContent;
    //                    despesaTemp.Valor = Convert.ToDecimal(colunas[ColunasDespesas.Valor.GetHashCode()].TextContent, cultureInfo);

    //                    InserirDespesaTemp(despesaTemp);
    //                }
    //            }
    //            else
    //            {
    //                Console.WriteLine(row.QuerySelector(".scGridBlockFont td").TextContent);
    //                //throw new BusinessException("Linha invalida!");
    //            }
    //        }

    //        var proximaPagina = document.QuerySelector("#forward_bot");
    //        if (proximaPagina.ClassList.Contains("disabled")) break;

    //        (proximaPagina as IHtmlAnchorElement).DoClick();
    //    }
    //}

    //private enum ColunasDespesas
    //{
    //    Legislatura = 3,
    //    Parlamentar,
    //    Competencia,
    //    Subcota,
    //    Valor
    //}

}

public class ImportadorParlamentarPiaui : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarPiaui(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://www.al.pi.leg.br/",
            SeletorListaParlamentares = "#bloco-fotos-parlamentar>div",
            Estado = Estado.Piaui,
        });
    }

    public override async Task Importar()
    {
        logger.LogWarning("Parlamentares do(a) {idEstado}:{CasaLegislativa}", config.Estado.GetHashCode(), config.Estado.ToString());
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        var angleSharpConfig = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(angleSharpConfig);

        var document = await context.OpenAsync(config.BaseAddress);
        if (document.StatusCode != HttpStatusCode.OK)
        {
            Console.WriteLine($"{config.BaseAddress} {document.StatusCode}");
        };

        var parlamentares = document.QuerySelectorAll(config.SeletorListaParlamentares);
        foreach (var parlamentar in parlamentares)
        {
            if (parlamentar.InnerHtml == "<br>") continue;

            var deputado = new DeputadoEstadual()
            {
                IdEstado = (ushort)config.Estado.GetHashCode()
            };

            deputado.UrlPerfil = (parlamentar.QuerySelector("a") as IHtmlAnchorElement).Href;
            deputado.UrlFoto = (parlamentar.QuerySelector("img") as IHtmlImageElement)?.Source;

            ArgumentException.ThrowIfNullOrEmpty(deputado.UrlPerfil, nameof(deputado.UrlPerfil));
            var subDocument = await context.OpenAsync(deputado.UrlPerfil);
            if (document.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError("Erro ao consultar deputado: {NomeDeputado} {StatusCode}", deputado.UrlPerfil, subDocument.StatusCode);
                continue;
            };
            deputado.NomeParlamentar = subDocument.QuerySelector("h1#parent-fieldname-title").TextContent.Split("-")[0].Trim().ToTitleCase();

            var deputadoJaExistente = GetDeputadoByNameOrNew(deputado.NomeParlamentar);
            if (deputadoJaExistente.Id != 0)
            {
                deputadoJaExistente.NomeParlamentar = deputado.NomeParlamentar;
                deputadoJaExistente.UrlPerfil = deputado.UrlPerfil;
                deputadoJaExistente.UrlFoto = deputado.UrlFoto;
                deputado = deputadoJaExistente;
            }

            if (string.IsNullOrEmpty(deputado.NomeCivil))
                deputado.NomeCivil = deputado.NomeParlamentar;

            InsertOrUpdate(deputado);
        }

        logger.LogWarning("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
    }

    public override DeputadoEstadual ColetarDadosLista(IElement document)
    {
        throw new NotImplementedException();
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        throw new NotImplementedException();
    }


    //public override DeputadoEstadual ColetarDadosLista(IElement item)
    //{
    //    if (item.InnerHtml == "<br>") return null;

    //    var deputado = new DeputadoEstadual()
    //    {
    //        IdEstado = (ushort)config.Estado.GetHashCode()
    //    };

    //    deputado.UrlPerfil = (item.QuerySelector("a") as IHtmlAnchorElement).Href;
    //    deputado.UrlFoto = (item.QuerySelector("img") as IHtmlImageElement)?.Source;

    //    return deputado;
    //}

    //public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    //{
    //    deputado.NomeParlamentar = subDocument.QuerySelector("h1#parent-fieldname-title").TextContent.Split("-")[0].Trim().ToTitleCase();

    //    var deputadoJaExistente = GetDeputadoByNameOrNew(deputado.NomeParlamentar);
    //    if (deputadoJaExistente.Id != 0)
    //    {
    //        deputadoJaExistente.NomeParlamentar = deputado.NomeParlamentar;
    //        deputadoJaExistente.UrlPerfil = deputado.UrlPerfil;
    //        deputadoJaExistente.UrlFoto = deputado.UrlFoto;
    //        deputado = deputadoJaExistente;
    //    }

    //    if (string.IsNullOrEmpty(deputado.NomeCivil))
    //        deputado.NomeCivil = deputado.NomeParlamentar;
    //}
}
