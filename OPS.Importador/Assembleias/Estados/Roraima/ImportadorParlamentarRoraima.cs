using System.Globalization;
using System.Net;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Estados.Roraima.Entities;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias.Estados.Roraima;

public class ImportadorParlamentarRoraima : ImportadorParlamentarRestApi
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorParlamentarRoraima(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarConfig()
        {
            BaseAddress = $"https://sapl.al.rr.leg.br/api/",
            Estado = Estado.Roraima,
        });
    }

    public override Task Importar()
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        var context = httpClient.CreateAngleSharpContext();

        // 9 - 9ª(2023 - 2026)
        // 1 - 8ª(2019 - 2022)
        // 2 - 7ª(2015 - 2018)
        // 3 - 6ª(2011 - 2014)
        // 6 - 5ª(2007 - 2010)
        // 4 - 4ª(2003 - 2006)
        // 5 - 3ª(1999 - 2002)
        // 7 - 2ª(1995 - 1998)
        // 8 - 1ª(1991 - 1994)
        var legislatura = 9;
        var address = $"{config.BaseAddress}parlamentares/legislatura/{legislatura}/parlamentares/?get_all=true";
        var deputadosRR = RestApiGet<List<DeputadosRoraima>>(address);

        foreach (var parlamentar in deputadosRR)
        {
            DeputadoEstadual deputado = GetDeputadoByMatriculaOrNew((int)parlamentar.Id);

            deputado.UrlPerfil = $"https://sapl.al.rr.leg.br/parlamentar/{parlamentar.Id}";
            deputado.NomeParlamentar = parlamentar.NomeParlamentar;
            deputado.IdPartido = BuscarIdPartido(parlamentar.Partido);
            deputado.UrlFoto = parlamentar.Fotografia;

            ImportarPerfil(deputado, context).Wait();

            InsertOrUpdate(deputado);
        }

        logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
        return Task.CompletedTask;
    }

    public async Task ImportarPerfil(DeputadoEstadual deputado, IBrowsingContext context)
    {
        var document = await context.OpenAsyncAutoRetry(deputado.UrlPerfil);
        if (document.StatusCode != HttpStatusCode.OK)
        {
            Console.WriteLine($"{config.BaseAddress} {document.StatusCode}");
        }
        ;

        ColetarDadosPerfil(deputado, document);
    }

    public void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument document)
    {
        //deputado.UrlFoto = (document.QuerySelector("#div_fotografia img") as IHtmlImageElement)?.Source;
        if (string.IsNullOrEmpty(deputado.NomeCivil))
            deputado.NomeCivil = document.QuerySelector("#div_nome").TextContent.Split(":").Last().Trim().ToTitleCase();

        var perfil = document
           .QuerySelectorAll("#div_data_nascimento") // pois é, alguem esqueceu de renomear
           .Select(x => new { Key = x.TextContent.Split(":")[0].Trim(), Value = x.TextContent.Split(":")[1].Trim() })
           .Where(x => !string.IsNullOrEmpty(x.Value) && x.Value != "Não informado")
           .ToList();

        //deputado.NomeCivil = perfil.FirstOrDefault(x => x.Key == "Nome" || x.Key == "Nome Completo")?.Value ?? deputado.NomeCivil;
        deputado.Telefone = perfil.FirstOrDefault(x => x.Key == "Telefone")?.Value ?? deputado.Telefone;
        deputado.Email = perfil.FirstOrDefault(x => x.Key.ToLower() == "e-mail")?.Value ?? deputado.Email;
        //deputado.Naturalidade = perfil.FirstOrDefault(x => x.Key == "Naturalidade")?.Value;
        //deputado.Profissao = perfil.FirstOrDefault(x => x.Key == "Formação")?.Value;

        var dataNascimento = perfil.FirstOrDefault(x => x.Key == "Data de Nascimento")?.Value;
        if (!string.IsNullOrEmpty(dataNascimento))
            deputado.Nascimento = DateOnly.Parse(dataNascimento, cultureInfo);

        var gabinete = perfil.FirstOrDefault(x => x.Key == "Número do Gabinete")?.Value;
        if (!string.IsNullOrEmpty(gabinete))
            deputado.Gabinete = Convert.ToInt16(gabinete);
    }
}