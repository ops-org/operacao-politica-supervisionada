using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

/// <summary>
/// https://www.al.ac.leg.br/
/// </summary>
public class Acre : ImportadorBase
{
    public Acre(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarAcre(serviceProvider);
        importadorDespesas = new ImportadorDespesasAcre(serviceProvider);
    }
}

/// <summary>
/// http://app.al.ac.leg.br/financa/verba-indenizatoria
/// </summary>
public class ImportadorDespesasAcre : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasAcre(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://app.al.ac.leg.br/financa/despesaVI", // TODO: Gastos totais mensais apenas
            Estado = Estado.Acre,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        logger.LogWarning("Portal sem dados detalhados!");
    }
}

public class ImportadorParlamentarAcre : ImportadorParlamentarRestApi
{

    public ImportadorParlamentarAcre(IServiceProvider serviceProvider) : base(serviceProvider)
    {

        Configure(new ImportadorParlamentarConfig()
        {
            BaseAddress = "https://sapl.al.ac.leg.br/",
            Estado = Estado.Acre,
        });
    }

    public override Task Importar()
    {
        logger.LogWarning("Parlamentares do(a) {idEstado}:{CasaLegislativa}", config.Estado.GetHashCode(), config.Estado.ToString());
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        var legislatura = 17; // 2023-2027
        var address = $"{config.BaseAddress}api/parlamentares/legislatura/{legislatura}/parlamentares/?get_all=true";
        List<DeputadoAcre> objDeputadosAcre = RestApiGet<List<DeputadoAcre>>(address);

        foreach (var parlamentar in objDeputadosAcre)
    {
            var matricula = (uint)parlamentar.Id;
            DeputadoEstadual deputado = GetDeputadoByMatriculaOrNew(matricula);

            deputado.UrlPerfil = $"https://sapl.al.ac.leg.br/parlamentar/{parlamentar.Id}";
            deputado.NomeParlamentar = parlamentar.NomeParlamentar.ToTitleCase();
            deputado.IdPartido = BuscarIdPartido(parlamentar.Partido);
            deputado.UrlFoto = parlamentar.Fotografia;

            ObterDetalhesDoPerfil(deputado).GetAwaiter().GetResult();

            InsertOrUpdate(deputado);
        }

        logger.LogWarning("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
        return Task.CompletedTask;
    }

    private async Task ObterDetalhesDoPerfil(DeputadoEstadual deputado)
    {
        var angleSharpConfig = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(angleSharpConfig);

        var document = await context.OpenAsync(deputado.UrlPerfil);
        if (document.StatusCode != HttpStatusCode.OK)
    {
            Console.WriteLine($"{config.BaseAddress} {document.StatusCode}");
        };

        var perfil = document.QuerySelector("#content");

        deputado.NomeCivil = perfil.QuerySelector("#div_nome").TextContent.Split(":")[1].Trim().ToTitleCase();

        var elementos = perfil.QuerySelectorAll(".form-group>p").Select(x => x.TextContent);
        if (elementos.Any())
        {
            deputado.Email = elementos.Where(x => x.StartsWith("E-mail")).FirstOrDefault()?.Split(':')[1].Trim();
            deputado.Telefone = elementos.Where(x => x.StartsWith("Telefone")).FirstOrDefault()?.Split(':')[1].Trim().NullIfEmpty();
        }
        else
        {
            logger.LogWarning($"Verificar possivel mudança no perfil do deputado: {deputado.UrlPerfil}");
        }
    }
    }

public class DeputadoAcre
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("nome_parlamentar")]
    public string NomeParlamentar { get; set; }

    [JsonPropertyName("fotografia_cropped")]
    public string FotografiaCropped { get; set; }

    [JsonPropertyName("fotografia")]
    public string Fotografia { get; set; }

    [JsonPropertyName("ativo")]
    public bool Ativo { get; set; }

    [JsonPropertyName("partido")]
    public string Partido { get; set; }

    [JsonPropertyName("titular")]
    public string Titular { get; set; }
}


