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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OPS.Importador.ALE;

public class Amapa : ImportadorBase
{
    public Amapa(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarAmapa(serviceProvider);
        importadorDespesas = new ImportadorDespesasAmapa(serviceProvider);
    }
}

public class ImportadorDespesasAmapa : ImportadorDespesasRestApiMensal
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
    private readonly List<DeputadoEstadual> deputados;

    public ImportadorDespesasAmapa(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://www.al.ap.gov.br/",
            Estado = Estado.Amapa,
            ChaveImportacao = ChaveDespesaTemp.Matricula
        };

        // TODO: Filtrar legislatura atual
        deputados = connection.GetList<DeputadoEstadual>(new { id_estado = config.Estado.GetHashCode() }).ToList();
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var address = $"{config.BaseAddress}transparencia/gabinete_ceap_json.php?ano_verbaB={ano}&mes_verbaB={mes:00}";
        var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();
        var gabinetes = document.QuerySelectorAll("option").ToList();

        foreach (var item in gabinetes)
        {
            var gabinete = item as IHtmlOptionElement;
            if (string.IsNullOrEmpty(gabinete.Value)) continue;

            var deputado = deputados.Find(x => gabinete.Value.Contains(x.Gabinete.ToString()));
            if (deputado == null)
            {
                logger.LogError($"Deputado {gabinete.Value}: {gabinete.Text} não existe ou não possui gabinete relacionado!");
            }

            address = $"{config.BaseAddress}transparencia/pagina.php?pg=ceap&acao=buscar&ano_verbaB={ano}&mes_verbaB={mes:00}&idgabineteB={gabinete.Value}";
            document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();

            if (document.QuerySelector(".ls-alert-warning")?.TextContent == "Nenhum: resultado foi encontrado!") continue;

            var tabelaDespesas = document.QuerySelector(".ls-table");
            var linhasDespesas = tabelaDespesas.QuerySelectorAll("tbody tr");

            foreach (var linha in linhasDespesas)
            {
                var primeiraColuna = linha.QuerySelectorAll("td")[0];
                if (primeiraColuna.TextContent == "TOTAL") continue;

                var linkDetalhes = (primeiraColuna.QuerySelector("a") as IHtmlAnchorElement);
                var despesaTemp = new CamaraEstadualDespesaTemp()
                {
                    Nome = gabinete.Text.Split("-")[0].Trim().ToTitleCase(),
                    Cpf = deputado?.Matricula?.ToString(),
                    Ano = (short)ano,
                    Mes =  (short)mes,
                    TipoDespesa = linkDetalhes.Text.Split(" - ")[1].Trim(),
                };

                var subDocument = context.OpenAsyncAutoRetry(linkDetalhes.Href).GetAwaiter().GetResult();
                var linhasDespesasDetalhes = subDocument.QuerySelectorAll(".ls-table tbody tr");
                foreach (var detalhes in linhasDespesasDetalhes)
                {
                    var colunas = detalhes.QuerySelectorAll("td");
                    if (colunas[0].TextContent == "TOTAL") continue;

                    var empresaParts = colunas[0].TextContent.Split(" - ");
                    despesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(empresaParts[0].Trim());
                    despesaTemp.Empresa = empresaParts[1].Trim();

                    despesaTemp.Documento = colunas[2].TextContent.Trim();
                    despesaTemp.Observacao = (colunas[2].QuerySelector("a") as IHtmlAnchorElement).Href;
                    despesaTemp.Valor = Convert.ToDecimal(colunas[3].TextContent, cultureInfo);
                    despesaTemp.DataEmissao = new DateTime(ano, mes, 1);

                    InserirDespesaTemp(despesaTemp);
                }
            }
        }
    }
}

public class ImportadorParlamentarAmapa : ImportadorParlamentarCrawler
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorParlamentarAmapa(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "http://www.al.ap.gov.br/pagina.php?pg=exibir_legislatura",
            SeletorListaParlamentares = ".container .ls-box:not(.ls-box-filter)",
            Estado = Estado.Amapa,
        });
    }

    ///// <summary>
    ///// Importação manual para deputados especificos (inativos e que não aparecem na lista principal)
    ///// </summary>
    ///// <returns></returns>
    //public override async Task Importar()
    //{
    //    var angleSharpConfig = Configuration.Default.WithDefaultLoader();
    //    var context = BrowsingContext.New(angleSharpConfig);

    //    var document = await context.OpenAsync(config.BaseAddress);
    //    if (document.StatusCode != HttpStatusCode.OK)
    //    {
    //        Console.WriteLine($"{config.BaseAddress} {document.StatusCode}");
    //    };

    //    var parlamentares = new[] { 29, 30, 36, 39, 60, 63, 76, 77, 80, 82, 90 };
    //    foreach (var matricula in parlamentares)
    //    {
    //        DeputadoEstadual deputado = new()
    //        {
    //            Matricula = (uint)matricula,
    //            IdEstado = (ushort)Estado.Amapa.GetHashCode(),
    //            UrlPerfil = $"http://www.al.ap.gov.br/pagina.php?pg=exibir_parlamentar&iddeputado={matricula}"
    //        };

    //        var subDocument = await context.OpenAsync(deputado.UrlPerfil);
    //        if (document.StatusCode != HttpStatusCode.OK)
    //        {
    //            logger.LogError("Erro ao consultar deputado: {NomeDeputado} {StatusCode}", deputado.UrlPerfil, subDocument.StatusCode);
    //            continue;
    //        };

    //        deputado.NomeParlamentar = subDocument
    //           .QuerySelector("h3.ls-title-3")
    //           .TextContent
    //           .Replace("Deputada", "")
    //           .Replace("Deputado", "")
    //           .Trim()
    //           .ToTitleCase();

    //        deputado.UrlFoto = (subDocument.QuerySelector("img.foto-deputado") as IHtmlImageElement)?.Source;

    //        ColetarDadosPerfil(deputado, subDocument);

    //        InsertOrUpdate(deputado);
    //    }
    //}

    public override DeputadoEstadual ColetarDadosLista(IElement parlamentar)
    {
        // http://www.al.ap.gov.br/pagina.php?pg=exibir_parlamentar&iddeputado=78
        var urlPerfil = (parlamentar.QuerySelector("a") as IHtmlAnchorElement).Href;
        var matricula = (uint)Convert.ToInt32(urlPerfil.Split("iddeputado=")[1]);

        var deputado = GetDeputadoByMatriculaOrNew(matricula);

        deputado.NomeParlamentar = parlamentar
            .QuerySelector("p.ls-title-5")
            .TextContent
            .Replace("Deputada", "")
            .Replace("Deputado", "")
            .Trim()
            .ToTitleCase();

        deputado.UrlPerfil = urlPerfil;
        deputado.UrlFoto = (parlamentar.QuerySelector("img.foto-deputado") as IHtmlImageElement)?.Source;

        return deputado;
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {     
        List<string> perfil = subDocument
            .QuerySelectorAll(".ls-box")[2] // Perfil
            .QuerySelectorAll("p")
            .Select(x => x.TextContent.Trim())
            .ToList();

        for (int i = 0; i < perfil.Count; i+=2)
        {
            switch (perfil[i])
            {
                case "Nome Civil:": deputado.NomeCivil = perfil[i+1].ToTitleCase(); break;
                case "E-mail:": deputado.Email = perfil[i+1]; break;
                case "Aniversário:": deputado.Nascimento = DateOnly.Parse(perfil[i+1], cultureInfo); break;
                case "Profissão:": deputado.Profissao = perfil[i+1].ToTitleCase().NullIfEmpty(); break;
                case "Partido:": deputado.IdPartido = BuscarIdPartido(perfil[i+1]); break;
                case "Legislatura(s):": return;
                default: throw new Exception($"Verificar alterações na pagina! {deputado.UrlPerfil}");
            }
        }
    }
}
