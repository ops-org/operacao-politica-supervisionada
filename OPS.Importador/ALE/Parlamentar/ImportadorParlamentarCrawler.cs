using System;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;

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
            DeputadoEstadual deputado = ColetarDadosLista(parlamentar);
            if (deputado == null) continue;

            ArgumentException.ThrowIfNullOrEmpty(deputado.UrlPerfil, nameof(deputado.UrlPerfil));
            var subDocument = await context.OpenAsync(deputado.UrlPerfil);
            if (document.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError("Erro ao consultar deputado: {NomeDeputado} {StatusCode}", deputado.UrlPerfil, subDocument.StatusCode);
                continue;
            };
            ColetarDadosPerfil(deputado, subDocument);

            InsertOrUpdate(deputado);
        }

        logger.LogWarning("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
    }

    public abstract DeputadoEstadual ColetarDadosLista(IElement document);

    public abstract void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument);


    public class ImportadorParlamentarCrawlerConfig : ImportadorParlamentarConfig
    {
        public string SeletorListaParlamentares { get; set; }
    }
}