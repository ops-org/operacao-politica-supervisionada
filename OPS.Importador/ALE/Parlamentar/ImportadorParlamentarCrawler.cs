using System;
using System.Net;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Importador.Utilities;

namespace OPS.Importador.ALE.Parlamentar;

public abstract class ImportadorParlamentarCrawler : ImportadorParlamentarBase, IImportadorParlamentar
{
    new protected ImportadorParlamentarCrawlerConfig config;

    public ImportadorParlamentarCrawler(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public void Configure(ImportadorParlamentarCrawlerConfig config)
    {
        base.config = config;
        this.config = config;
    }

    public override async Task Importar()
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        var context = httpClient.CreateAngleSharpContext();

        var document = await context.OpenAsyncAutoRetry(config.BaseAddress);
        if (document.StatusCode != HttpStatusCode.OK)
        {
            Console.WriteLine($"{config.BaseAddress} {document.StatusCode}");
        };

        var parlamentares = document.QuerySelectorAll(config.SeletorListaParlamentares);
        foreach (var parlamentar in parlamentares)
        {
            DeputadoEstadual deputado = ColetarDadosLista(parlamentar);
            if (deputado == null) continue;

            if (config.ColetaDadosDoPerfil)
            {
                ArgumentException.ThrowIfNullOrEmpty(deputado.UrlPerfil, nameof(deputado.UrlPerfil));
                var subDocument = await context.OpenAsyncAutoRetry(deputado.UrlPerfil);
                if (document.StatusCode != HttpStatusCode.OK)
                {
                    logger.LogError("Erro ao consultar parlamentar: {NomeDeputado} {StatusCode}", deputado.UrlPerfil, subDocument.StatusCode);
                    continue;
                };
                ColetarDadosPerfil(deputado, subDocument);
            }

            InsertOrUpdate(deputado);
        }

        logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
    }

    public abstract DeputadoEstadual ColetarDadosLista(IElement document);

    public abstract void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument);

    public class ImportadorParlamentarCrawlerConfig : ImportadorParlamentarConfig
    {
        public string SeletorListaParlamentares { get; set; }

        public bool ColetaDadosDoPerfil { get; set; } = true;
    }
}