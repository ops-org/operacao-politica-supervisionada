using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AngleSharp;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

/// <summary>
/// Assembleia Legislativa do Estado de Rio Grande do Sul
/// </summary>
public class RioGrandeDoSul : ImportadorBase
{
    public RioGrandeDoSul(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        //importadorParlamentar = new ImportadorParlamentarRioGrandeDoSul(serviceProvider);
        //importadorDespesas = new ImportadorDespesasRioGrandeDoSul(serviceProvider);
    }
}

public class ImportadorDespesasRioGrandeDoSul : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasRioGrandeDoSul(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "",
            Estado = Estado.RioGrandeDoSul,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        throw new NotImplementedException();
    }
}

public class ImportadorParlamentarRioGrandeDoSul : ImportadorParlamentarRestApi
{

    public ImportadorParlamentarRioGrandeDoSul(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarConfig()
        {
            BaseAddress = "https://ww4.al.rs.gov.br:5000/",
            Estado = Estado.RioGrandeDoSul,
        });
    }

    public override Task Importar()
    {
        var address = $"{config.BaseAddress}listarDestaqueDeputados";
        DeputadosRS objDeputadosRS = RestApiGet<DeputadosRS>(address);

        foreach (var parlamentar in objDeputadosRS.Lista)
        {
            var matricula = (uint)parlamentar.IdDeputado;
            var deputado = GetDeputadoByMatriculaOrNew(matricula);

            deputado.UrlPerfil = $"https://ww4.al.rs.gov.br/deputados/{parlamentar.IdDeputado}";
            deputado.NomeParlamentar = parlamentar.NomeDeputado;
            deputado.IdPartido = BuscarIdPartido(parlamentar.SiglaPartido);
            deputado.Email = parlamentar.EmailDeputado;
            deputado.Telefone = parlamentar.TelefoneDeputado;
            deputado.UrlFoto = parlamentar.FotoGrandeDeputado;

            InsertOrUpdate(deputado);
        }

        return Task.CompletedTask;
    }
}


public class DeputadoRS
{
    [JsonPropertyName("codigoPro")]
    public int CodigoPro { get; set; }

    [JsonPropertyName("idDeputado")]
    public int IdDeputado { get; set; }

    [JsonPropertyName("nomeDeputado")]
    public string NomeDeputado { get; set; }

    [JsonPropertyName("emailDeputado")]
    public string EmailDeputado { get; set; }

    [JsonPropertyName("telefoneDeputado")]
    public string TelefoneDeputado { get; set; }

    [JsonPropertyName("siglaPartido")]
    public string SiglaPartido { get; set; }

    [JsonPropertyName("nomePartido")]
    public string NomePartido { get; set; }

    [JsonPropertyName("codStatus")]
    public int CodStatus { get; set; }

    [JsonPropertyName("fotoGrandeDeputado")]
    public string FotoGrandeDeputado { get; set; }
}

public class DeputadosRS
{
    [JsonPropertyName("lista")]
    public List<DeputadoRS> Lista { get; set; }
}
