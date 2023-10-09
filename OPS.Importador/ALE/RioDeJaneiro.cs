using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AngleSharp;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

/// <summary>
/// Assembleia Legislativa do Estado do Rio de Janeiro
/// 
/// </summary>
public class RioDeJaneiro : ImportadorBase
{
    public RioDeJaneiro(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarRioDeJaneiro(serviceProvider);
        //importadorDespesas = new ImportadorDespesasRioDeJaneiro(serviceProvider);
    }
}

public class ImportadorDespesasRioDeJaneiro : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasRioDeJaneiro(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "",
            Estado = Estado.RioDeJaneiro,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        throw new NotImplementedException();
    }
}

public class ImportadorParlamentarRioDeJaneiro : ImportadorParlamentarRestApi
{

    public ImportadorParlamentarRioDeJaneiro(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarConfig()
        {
            BaseAddress = "https://docigp.alerj.rj.gov.br/",
            Estado = Estado.RioDeJaneiro,
        });
    }

    public override Task Importar()
    {
        var query = "{\"filter\":{\"text\":null,\"checkboxes\":{\"withMandate\":false,\"withoutMandate\":false,\"withPendency\":false,\"withoutPendency\":false,\"unread\":false,\"joined\":true,\"notJoined\":false,\"filler\":false},\"selects\":{\"filler\":false}},\"pagination\":{\"total\":83,\"per_page\":\"250\",\"current_page\":1,\"last_page\":9,\"from\":1,\"to\":10,\"pages\":[1,2,3,4,5]}}";
        var address = $"{config.BaseAddress}api/v1/congressmen?query=" + WebUtility.UrlEncode(query);
        DeputadosRJ objDeputadosRJ = RestApiGet<DeputadosRJ>(address);

        foreach (var parlamentar in objDeputadosRJ.Rows)
        {
            var matricula = (uint)parlamentar.Id;
            DeputadoEstadual deputado = GetDeputadoByMatriculaOrNew(matricula);

            //deputado.UrlPerfil = $"https://ww4.al.rs.gov.br/deputados/{parlamentar.IdDeputado}";
            deputado.NomeParlamentar = parlamentar.User.Name;
            deputado.IdPartido = BuscarIdPartido(parlamentar.Party.Code);
            deputado.Email = parlamentar.User.Email;
            //deputado.Telefone = parlamentar.TelefoneDeputado;
            deputado.UrlFoto = parlamentar.PhotoUrlLinkable;

            InsertOrUpdate(deputado);
        }

        return Task.CompletedTask;
    }
}

public class PartidoRJ
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class DeputadosRJ
{

    [JsonPropertyName("rows")]
    public List<DespesaMensal> Rows { get; set; }
}

public class DespesaMensal
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("has_mandate")]
    public bool HasMandate { get; set; }

    [JsonPropertyName("photo_url_linkable")]
    public string PhotoUrlLinkable { get; set; }

    [JsonPropertyName("party")]
    public PartidoRJ Party { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }
}

public class User
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }
}

public class Entry
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("cost_center_id")]
    public int CostCenterId { get; set; }

    [JsonPropertyName("congressman_budget_id")]
    public int CongressmanBudgetId { get; set; }

    [JsonPropertyName("object")]
    public string Object { get; set; }

    [JsonPropertyName("to")]
    public string To { get; set; }

    [JsonPropertyName("entry_type_id")]
    public int EntryTypeId { get; set; }

    [JsonPropertyName("document_number")]
    public string DocumentNumber { get; set; }

    [JsonPropertyName("provider_id")]
    public int ProviderId { get; set; }

    [JsonPropertyName("verified_at")]
    public DateTime VerifiedAt { get; set; }

    [JsonPropertyName("verified_by_id")]
    public int VerifiedById { get; set; }

    [JsonPropertyName("analysed_at")]
    public DateTime AnalysedAt { get; set; }

    [JsonPropertyName("analysed_by_id")]
    public int AnalysedById { get; set; }

    [JsonPropertyName("published_at")]
    public DateTime PublishedAt { get; set; }

    [JsonPropertyName("published_by_id")]
    public int PublishedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("created_by_id")]
    public int CreatedById { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("updated_by_id")]
    public int UpdatedById { get; set; }

    [JsonPropertyName("is_transport")]
    public bool IsTransport { get; set; }

    [JsonPropertyName("is_transport_or_credit")]
    public bool IsTransportOrCredit { get; set; }

    [JsonPropertyName("comments_count")]
    public int CommentsCount { get; set; }

    [JsonPropertyName("provider_is_blocked")]
    public bool ProviderIsBlocked { get; set; }

    [JsonPropertyName("provider")]
    public Provider Provider { get; set; }
}

public class Links
{
    [JsonPropertyName("pagination")]
    public Pagination Pagination { get; set; }
}

public class Pagination
{
    [JsonPropertyName("current_page")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("last_page")]
    public int LastPage { get; set; }
}

public class Provider
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("cpf_cnpj")]
    public string CpfCnpj { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("created_by_id")]
    public int CreatedById { get; set; }

    [JsonPropertyName("updated_by_id")]
    public int? UpdatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("zipcode")]
    public object Zipcode { get; set; }

    [JsonPropertyName("street")]
    public object Street { get; set; }

    [JsonPropertyName("number")]
    public object Number { get; set; }

    [JsonPropertyName("complement")]
    public object Complement { get; set; }

    [JsonPropertyName("neighborhood")]
    public object Neighborhood { get; set; }

    [JsonPropertyName("city")]
    public object City { get; set; }

    [JsonPropertyName("state")]
    public object State { get; set; }

    [JsonPropertyName("fullAddress")]
    public object FullAddress { get; set; }

    [JsonPropertyName("is_blocked")]
    public bool IsBlocked { get; set; }
}

public class DespesasMensais
{
    [JsonPropertyName("links")]
    public Links Links { get; set; }

    [JsonPropertyName("rows")]
    public List<DespesaMensalRJ> Rows { get; set; }
}

public class DespesaMensalRJ
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("entries")]
    public List<Entry> Entries { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("month")]
    public string Month { get; set; }
}
