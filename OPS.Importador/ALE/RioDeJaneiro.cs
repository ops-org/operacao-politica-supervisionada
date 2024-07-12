using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AngleSharp;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core;
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
        importadorDespesas = new ImportadorDespesasRioDeJaneiro(serviceProvider);
    }
}

public class ImportadorDespesasRioDeJaneiro : ImportadorDespesasRestApiAnual
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    /// <summary>
    /// https://docigp.alerj.rj.gov.br/transparencia#/
    /// </summary>
    /// <param name="serviceProvider"></param>
    public ImportadorDespesasRioDeJaneiro(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://docigp.alerj.rj.gov.br/api/v1/",
            Estado = Estado.RioDeJaneiro,
            ChaveImportacao = ChaveDespesaTemp.Matricula
        };
    }

    // https://docigp.alerj.rj.gov.br/api/v1/congressmen?query={"filter":{"text":null,"checkboxes":{"withMandate":false,"withoutMandate":false,"withPendency":false,"withoutPendency":false,"unread":false,"joined":true,"notJoined":false,"filler":false},"selects":{"legislatureId":2,"filler":false}},"pagination":{"total":86,"per_page":250,"current_page":1,"last_page":9,"from":1,"to":10,"pages":[1,2,3,4,5]}}
    // https://docigp.alerj.rj.gov.br/api/v1/congressmen/95/legislatures/2/budgets?query={"filter":{"text":null,"checkboxes":{"filler":false},"selects":{"filler":false}},"pagination":{"per_page":250,"current_page":1},"order":{}}
    // https://docigp.alerj.rj.gov.br/api/v1/congressmen/95/legislatures/2/budgets/4321/entries?query={"filter":{"text":null,"checkboxes":{"filler":false},"selects":{"filler":false}},"pagination":{"total":6,"per_page":250,"current_page":1,"last_page":1,"from":1,"to":6,"pages":[1]}}

    // https://docigp.alerj.rj.gov.br/api/v1/cost-centers?query={"filter":{"text":null,"checkboxes":{"filler":false},"selects":{"filler":false}},"pagination":{"per_page":5,"current_page":1},"order":{}}
    // https://docigp.alerj.rj.gov.br/api/v1/entry-types?query={"filter":{"text":null,"checkboxes":{"filler":false},"selects":{"filler":false}},"pagination":{"total":7,"per_page":10000,"current_page":1,"last_page":1,"from":1,"to":7,"pages":[1]}}
    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        // TODO: Criar importação por legislatura
        if (ano != 2023)
        {
            logger.LogWarning($"Importação já realizada para o ano de {ano}");
            return;
        }

        int legislatura = 2; // 2023-2027

        var query = "{\"filter\":{\"text\":null,\"checkboxes\":{\"withMandate\":false,\"withoutMandate\":false,\"withPendency\":false,\"withoutPendency\":false,\"unread\":false,\"joined\":true,\"notJoined\":false,\"filler\":false},\"selects\":{\"filler\":false}},\"pagination\":{\"total\":83,\"per_page\":\"250\",\"current_page\":1,\"last_page\":9,\"from\":1,\"to\":10,\"pages\":[1,2,3,4,5]}}";
        var address = $"{config.BaseAddress}congressmen?query=" + WebUtility.UrlEncode(query);
        Congressman objDeputadosRJ = RestApiGet<Congressman>(address);

        foreach (CongressmanDetails deputado in objDeputadosRJ.Data)
        {
            query = "{\"filter\":{\"text\":null,\"checkboxes\":{\"filler\":false},\"selects\":{\"filler\":false}},\"pagination\":{\"per_page\":250,\"current_page\":1},\"order\":{}}";
            address = $"{config.BaseAddress}congressmen/{deputado.Id}/legislatures/{legislatura}/budgets?query=" + WebUtility.UrlEncode(query);
            Budgets orcamentoMensal = RestApiGet<Budgets>(address);

            if (orcamentoMensal.Links.Pagination.LastPage > 1)
                throw new NotImplementedException();

            foreach (BudgetDetails orcamento in orcamentoMensal.Data)
            {
                query = "{\"filter\":{\"text\":null,\"checkboxes\":{\"filler\":false},\"selects\":{\"filler\":false}},\"pagination\":{\"total\":6,\"per_page\":250,\"current_page\":1,\"last_page\":1,\"from\":1,\"to\":6,\"pages\":[1]}}";
                address = $"{config.BaseAddress}congressmen/{deputado.Id}/legislatures/{legislatura}/budgets/{orcamento.Id}/entries?query=" + WebUtility.UrlEncode(query);
                Entries despesas = RestApiGet<Entries>(address);

                if (despesas.Links.Pagination.LastPage > 1)
                    throw new NotImplementedException();

                foreach (EntryDetails despesa in despesas.Data)
                {
                    if (despesa.CostCenterCode == "4") continue; // 4 - Devolução de saldo
                    if (Convert.ToDecimal(despesa.Value) > 1)
                        throw new NotImplementedException();

                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Nome = deputado.Nickname.ToTitleCase(),
                        Cpf = deputado.Id.ToString(),
                        Ano = (short)orcamento.Year,
                        Mes = Convert.ToInt16(orcamento.Month),
                        TipoDespesa = $"{despesa.CostCenterCode} - {despesa.CostCenterName}",
                        Valor = (decimal)despesa.ValueAbs,
                        DataEmissao = Convert.ToDateTime(despesa.Date, cultureInfo),
                        CnpjCpf = Utils.RemoveCaracteresNaoNumericos(despesa.CpfCnpj),
                        Empresa = despesa.Name,
                        Documento = despesa.Id.ToString(),
                        Observacao = $"Objeto: {despesa.Object};"
                    };

                    if (despesa.DocumentsCount > 1)
                        despesaTemp.Observacao += $" Documentos: {despesa.DocumentsCount};";
                    if (!string.IsNullOrEmpty(despesa.DocumentNumber))
                        despesaTemp.Observacao += $" Num. Doc.: {despesa.DocumentNumber};";

                    InserirDespesaTemp(despesaTemp);
                }
            }
        }
    }

    public override string SqlCarregarHashes(int ano)
    {
        return $"select d.id, d.hash from cl_despesa d join cl_deputado p on d.id_cl_deputado = p.id where p.id_estado = {idEstado} and d.ano_mes between {ano}01 and {ano + 4}12";
    }

    public override int ContarRegistrosBaseDeDadosFinal(int ano)
    {
        return connection.ExecuteScalar<int>(@$"
select count(1) 
from cl_despesa d 
join cl_deputado p on p.id = d.id_cl_deputado 
where p.id_estado = {idEstado}
and d.ano_mes between {ano}01 and {ano + 4}12");
    }

}

public class ImportadorParlamentarRioDeJaneiro : ImportadorParlamentarRestApi
{

    public ImportadorParlamentarRioDeJaneiro(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarConfig()
        {
            BaseAddress = "https://docigp.alerj.rj.gov.br/api/v1/",
            Estado = Estado.RioDeJaneiro,
        });
    }

    public override Task Importar()
    {
        logger.LogWarning("Parlamentares do(a) {idEstado}:{CasaLegislativa}", config.Estado.GetHashCode(), config.Estado.ToString());
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        var query = "{\"filter\":{\"text\":null,\"checkboxes\":{\"withMandate\":false,\"withoutMandate\":false,\"withPendency\":false,\"withoutPendency\":false,\"unread\":false,\"joined\":true,\"notJoined\":false,\"filler\":false},\"selects\":{\"filler\":false}},\"pagination\":{\"total\":83,\"per_page\":\"250\",\"current_page\":1,\"last_page\":9,\"from\":1,\"to\":10,\"pages\":[1,2,3,4,5]}}";
        var address = $"{config.BaseAddress}congressmen?query=" + WebUtility.UrlEncode(query);
        Congressman objDeputadosRJ = RestApiGet<Congressman>(address);

        foreach (var parlamentar in objDeputadosRJ.Data)
        {
            var matricula = (uint)parlamentar.Id;
            DeputadoEstadual deputado = GetDeputadoByMatriculaOrNew(matricula);

            deputado.UrlPerfil = $"https://www.alerj.rj.gov.br/Deputados/PerfilDeputado/{parlamentar.RemoteId}?Legislatura=20";
            deputado.NomeParlamentar = parlamentar.Nickname.ToTitleCase();
            deputado.NomeCivil = parlamentar.Name.ToTitleCase();
            deputado.IdPartido = BuscarIdPartido(parlamentar.Party.Code);
            deputado.Email = parlamentar.User.Email;
            //deputado.Telefone = parlamentar.TelefoneDeputado;
            deputado.UrlFoto = parlamentar.PhotoUrl;

            InsertOrUpdate(deputado);
        }

        logger.LogWarning("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
        return Task.CompletedTask;
    }
}


public class CongressmanDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// https://www.alerj.rj.gov.br/Deputados/PerfilDeputado/{RemoteId}?Legislatura=20
    /// </summary>
    [JsonPropertyName("remote_id")]
    public int RemoteId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    //[JsonPropertyName("party_id")]
    //public int PartyId { get; set; }

    [JsonPropertyName("photo_url")]
    public string PhotoUrl { get; set; }

    //[JsonPropertyName("thumbnail_url")]
    //public string ThumbnailUrl { get; set; }

    //[JsonPropertyName("department_id")]
    //public int DepartmentId { get; set; }

    //[JsonPropertyName("created_by_id")]
    //public int CreatedById { get; set; }

    //[JsonPropertyName("updated_by_id")]
    //public int? UpdatedById { get; set; }

    //[JsonPropertyName("created_at")]
    //public DateTime CreatedAt { get; set; }

    //[JsonPropertyName("updated_at")]
    //public DateTime UpdatedAt { get; set; }

    //[JsonPropertyName("has_mandate")]
    //public bool HasMandate { get; set; }

    //[JsonPropertyName("has_pendency")]
    //public bool HasPendency { get; set; }

    //[JsonPropertyName("is_published")]
    //public bool IsPublished { get; set; }

    //[JsonPropertyName("thumbnail_url_linkable")]
    //public string ThumbnailUrlLinkable { get; set; }

    //[JsonPropertyName("photo_url_linkable")]
    //public string PhotoUrlLinkable { get; set; }

    [JsonPropertyName("party")]
    public Party Party { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }
}

//public class Link
//{
//    [JsonPropertyName("url")]
//    public string Url { get; set; }

//    [JsonPropertyName("label")]
//    public string Label { get; set; }

//    [JsonPropertyName("active")]
//    public bool Active { get; set; }
//}

public class Party
{
    //[JsonPropertyName("id")]
    //public int Id { get; set; }

    [JsonPropertyName("code")]
    public string Code { get; set; }

    //[JsonPropertyName("name")]
    //public string Name { get; set; }

    //[JsonPropertyName("created_by_id")]
    //public int CreatedById { get; set; }

    //[JsonPropertyName("updated_by_id")]
    //public int? UpdatedById { get; set; }

    //[JsonPropertyName("created_at")]
    //public DateTime CreatedAt { get; set; }

    //[JsonPropertyName("updated_at")]
    //public DateTime UpdatedAt { get; set; }
}

public class Congressman
{
    //[JsonPropertyName("current_page")]
    //public int CurrentPage { get; set; }

    [JsonPropertyName("rows")]
    public List<CongressmanDetails> Data { get; set; }

    //[JsonPropertyName("first_page_url")]
    //public string FirstPageUrl { get; set; }

    //[JsonPropertyName("from")]
    //public int From { get; set; }

    //[JsonPropertyName("last_page")]
    //public int LastPage { get; set; }

    //[JsonPropertyName("last_page_url")]
    //public string LastPageUrl { get; set; }

    //[JsonPropertyName("links")]
    //public List<Link> Links { get; set; }

    //[JsonPropertyName("next_page_url")]
    //public object NextPageUrl { get; set; }

    ////[JsonPropertyName("path")]
    ////public string Path { get; set; }

    ////[JsonPropertyName("per_page")]
    ////public int PerPage { get; set; }

    ////[JsonPropertyName("prev_page_url")]
    ////public object PrevPageUrl { get; set; }

    ////[JsonPropertyName("to")]
    ////public int To { get; set; }

    //[JsonPropertyName("total")]
    //public int Total { get; set; }
}

public class User
{
    //[JsonPropertyName("id")]
    //public int Id { get; set; }

    //[JsonPropertyName("name")]
    //public string Name { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    //[JsonPropertyName("email_verified_at")]
    //public object EmailVerifiedAt { get; set; }

    //[JsonPropertyName("per_page")]
    //public int PerPage { get; set; }

    //[JsonPropertyName("created_by_id")]
    //public object CreatedById { get; set; }

    //[JsonPropertyName("updated_by_id")]
    //public object UpdatedById { get; set; }

    //[JsonPropertyName("created_at")]
    //public DateTime CreatedAt { get; set; }

    //[JsonPropertyName("updated_at")]
    //public DateTime UpdatedAt { get; set; }

    //[JsonPropertyName("username")]
    //public string Username { get; set; }

    //[JsonPropertyName("department_id")]
    //public int DepartmentId { get; set; }

    //[JsonPropertyName("congressman_id")]
    //public int CongressmanId { get; set; }

    //[JsonPropertyName("last_login_at")]
    //public string LastLoginAt { get; set; }

    //[JsonPropertyName("disabled_at")]
    //public object DisabledAt { get; set; }
}


//public class Budget
//{
//    [JsonPropertyName("id")]
//    public int Id { get; set; }

//    [JsonPropertyName("date")]
//    public DateTime Date { get; set; }

//    [JsonPropertyName("value")]
//    public string Value { get; set; }

//    [JsonPropertyName("created_by_id")]
//    public int CreatedById { get; set; }

//    [JsonPropertyName("updated_by_id")]
//    public int UpdatedById { get; set; }

//    [JsonPropertyName("created_at")]
//    public DateTime CreatedAt { get; set; }

//    [JsonPropertyName("updated_at")]
//    public DateTime UpdatedAt { get; set; }

//    [JsonPropertyName("federal_budget_id")]
//    public int FederalBudgetId { get; set; }
//}

//public class CongressmanLegislature
//{
//    [JsonPropertyName("id")]
//    public int Id { get; set; }

//    [JsonPropertyName("congressman_id")]
//    public int CongressmanId { get; set; }

//    [JsonPropertyName("legislature_id")]
//    public int LegislatureId { get; set; }

//    [JsonPropertyName("started_at")]
//    public DateTime StartedAt { get; set; }

//    [JsonPropertyName("ended_at")]
//    public object EndedAt { get; set; }

//    [JsonPropertyName("created_by_id")]
//    public int CreatedById { get; set; }

//    [JsonPropertyName("updated_by_id")]
//    public object UpdatedById { get; set; }

//    [JsonPropertyName("created_at")]
//    public DateTime CreatedAt { get; set; }

//    [JsonPropertyName("updated_at")]
//    public DateTime UpdatedAt { get; set; }

//    [JsonPropertyName("congressman")]
//    public Congressman Congressman { get; set; }
//}

public class BudgetDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    //[JsonPropertyName("congressman_legislature_id")]
    //public int CongressmanLegislatureId { get; set; }

    //[JsonPropertyName("budget_id")]
    //public int BudgetId { get; set; }

    //[JsonPropertyName("percentage")]
    //public string Percentage { get; set; }

    //[JsonPropertyName("value")]
    //public string Value { get; set; }

    //[JsonPropertyName("analysed_by_id")]
    //public int AnalysedById { get; set; }

    //[JsonPropertyName("analysed_at")]
    //public string AnalysedAt { get; set; }

    //[JsonPropertyName("published_by_id")]
    //public int PublishedById { get; set; }

    //[JsonPropertyName("published_at")]
    //public string PublishedAt { get; set; }

    //[JsonPropertyName("created_by_id")]
    //public int CreatedById { get; set; }

    //[JsonPropertyName("updated_by_id")]
    //public int UpdatedById { get; set; }

    //[JsonPropertyName("transport_from_previous_entry_id")]
    //public int? TransportFromPreviousEntryId { get; set; }

    //[JsonPropertyName("transport_to_next_entry_id")]
    //public int TransportToNextEntryId { get; set; }

    //[JsonPropertyName("created_at")]
    //public DateTime CreatedAt { get; set; }

    //[JsonPropertyName("updated_at")]
    //public DateTime UpdatedAt { get; set; }

    //[JsonPropertyName("closed_by_id")]
    //public int ClosedById { get; set; }

    //[JsonPropertyName("closed_at")]
    //public string ClosedAt { get; set; }

    //[JsonPropertyName("missing_analysis")]
    //public bool MissingAnalysis { get; set; }

    //[JsonPropertyName("missing_verification")]
    //public bool MissingVerification { get; set; }

    //[JsonPropertyName("has_deposit")]
    //public bool HasDeposit { get; set; }

    [JsonPropertyName("entries_count")]
    public int EntriesCount { get; set; }

    //[JsonPropertyName("sum_credit")]
    //public string SumCredit { get; set; }

    //[JsonPropertyName("sum_debit")]
    //public string SumDebit { get; set; }

    //[JsonPropertyName("has_comments_pendent")]
    //public bool HasCommentsPendent { get; set; }

    //[JsonPropertyName("has_comments_pendent_others_months")]
    //public bool HasCommentsPendentOthersMonths { get; set; }

    //[JsonPropertyName("has_comments_pendent_unanalysed")]
    //public bool HasCommentsPendentUnanalysed { get; set; }

    //[JsonPropertyName("sum_i")]
    //public string SumI { get; set; }

    //[JsonPropertyName("sum_ii")]
    //public object SumIi { get; set; }

    //[JsonPropertyName("sum_iii")]
    //public object SumIii { get; set; }

    //[JsonPropertyName("sum_iv")]
    //public string SumIv { get; set; }

    //[JsonPropertyName("sum_v")]
    //public object SumV { get; set; }

    //[JsonPropertyName("sum_vi_a")]
    //public string SumViA { get; set; }

    //[JsonPropertyName("sum_vi_b")]
    //public string SumViB { get; set; }

    //[JsonPropertyName("sum_vii")]
    //public string SumVii { get; set; }

    //[JsonPropertyName("sum_viii")]
    //public string SumViii { get; set; }

    //[JsonPropertyName("sum_ix")]
    //public object SumIx { get; set; }

    //[JsonPropertyName("sum_x")]
    //public string SumX { get; set; }

    //[JsonPropertyName("sum_xi")]
    //public object SumXi { get; set; }

    //[JsonPropertyName("has_refund")]
    //public bool HasRefund { get; set; }

    //[JsonPropertyName("pendency_comments_date")]
    //public List<object> PendencyCommentsDate { get; set; }

    //[JsonPropertyName("budget")]
    //public Budget Budget { get; set; }

    //[JsonPropertyName("congressman_legislature")]
    //public CongressmanLegislature CongressmanLegislature { get; set; }

    //[JsonPropertyName("entries")]
    //public List<Entry> Entries { get; set; }

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("month")]
    public string Month { get; set; }

    //[JsonPropertyName("state_value_formatted")]
    //public string StateValueFormatted { get; set; }

    //[JsonPropertyName("value_formatted")]
    //public string ValueFormatted { get; set; }

    //[JsonPropertyName("sum_debit_formatted")]
    //public string SumDebitFormatted { get; set; }

    //[JsonPropertyName("sum_credit_formatted")]
    //public string SumCreditFormatted { get; set; }

    //[JsonPropertyName("balance")]
    //public int Balance { get; set; }

    //[JsonPropertyName("balance_formatted")]
    //public string BalanceFormatted { get; set; }

    //[JsonPropertyName("percentage_formatted")]
    //public string PercentageFormatted { get; set; }

    //[JsonPropertyName("pendencies")]
    //public List<object> Pendencies { get; set; }
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

    //[JsonPropertyName("to")]
    //public string To { get; set; }

    //[JsonPropertyName("entry_type_id")]
    //public int EntryTypeId { get; set; }

    //[JsonPropertyName("document_number")]
    //public object DocumentNumber { get; set; }

    //[JsonPropertyName("provider_id")]
    //public int ProviderId { get; set; }

    //[JsonPropertyName("verified_at")]
    //public DateTime VerifiedAt { get; set; }

    //[JsonPropertyName("verified_by_id")]
    //public int VerifiedById { get; set; }

    //[JsonPropertyName("analysed_at")]
    //public DateTime AnalysedAt { get; set; }

    //[JsonPropertyName("analysed_by_id")]
    //public int AnalysedById { get; set; }

    //[JsonPropertyName("published_at")]
    //public DateTime PublishedAt { get; set; }

    //[JsonPropertyName("published_by_id")]
    //public int PublishedById { get; set; }

    //[JsonPropertyName("created_at")]
    //public DateTime CreatedAt { get; set; }

    //[JsonPropertyName("created_by_id")]
    //public int CreatedById { get; set; }

    //[JsonPropertyName("updated_at")]
    //public DateTime UpdatedAt { get; set; }

    //[JsonPropertyName("updated_by_id")]
    //public int UpdatedById { get; set; }

    //[JsonPropertyName("is_transport")]
    //public bool IsTransport { get; set; }

    //[JsonPropertyName("is_transport_or_credit")]
    //public bool IsTransportOrCredit { get; set; }

    //[JsonPropertyName("comments_count")]
    //public int CommentsCount { get; set; }

    //[JsonPropertyName("provider_is_blocked")]
    //public bool ProviderIsBlocked { get; set; }

    //[JsonPropertyName("provider")]
    //public Provider Provider { get; set; }
}

//public class Provider
//{
//    //[JsonPropertyName("id")]
//    //public int Id { get; set; }

//    [JsonPropertyName("cpf_cnpj")]
//    public string CpfCnpj { get; set; }

//    [JsonPropertyName("type")]
//    public string Type { get; set; }

//    [JsonPropertyName("name")]
//    public string Name { get; set; }

//    //[JsonPropertyName("created_by_id")]
//    //public int CreatedById { get; set; }

//    //[JsonPropertyName("updated_by_id")]
//    //public int? UpdatedById { get; set; }

//    //[JsonPropertyName("created_at")]
//    //public DateTime CreatedAt { get; set; }

//    //[JsonPropertyName("updated_at")]
//    //public DateTime UpdatedAt { get; set; }

//    //[JsonPropertyName("zipcode")]
//    //public object Zipcode { get; set; }

//    //[JsonPropertyName("street")]
//    //public object Street { get; set; }

//    //[JsonPropertyName("number")]
//    //public object Number { get; set; }

//    //[JsonPropertyName("complement")]
//    //public object Complement { get; set; }

//    //[JsonPropertyName("neighborhood")]
//    //public object Neighborhood { get; set; }

//    //[JsonPropertyName("city")]
//    //public object City { get; set; }

//    //[JsonPropertyName("state")]
//    //public object State { get; set; }

//    //[JsonPropertyName("fullAddress")]
//    //public object FullAddress { get; set; }

//    //[JsonPropertyName("is_blocked")]
//    //public bool IsBlocked { get; set; }
//}

public class Budgets
{
    //[JsonPropertyName("current_page")]
    //public int CurrentPage { get; set; }

    [JsonPropertyName("rows")]
    public List<BudgetDetails> Data { get; set; }

    //[JsonPropertyName("first_page_url")]
    //public string FirstPageUrl { get; set; }

    //[JsonPropertyName("from")]
    //public int From { get; set; }

    //[JsonPropertyName("last_page")]
    //public int LastPage { get; set; }

    //[JsonPropertyName("last_page_url")]
    //public string LastPageUrl { get; set; }

    [JsonPropertyName("links")]
    public Link Links { get; set; }

    //[JsonPropertyName("next_page_url")]
    //public object NextPageUrl { get; set; }

    ////[JsonPropertyName("path")]
    ////public string Path { get; set; }

    ////[JsonPropertyName("per_page")]
    ////public int PerPage { get; set; }

    ////[JsonPropertyName("prev_page_url")]
    ////public object PrevPageUrl { get; set; }

    ////[JsonPropertyName("to")]
    ////public int To { get; set; }

    //[JsonPropertyName("total")]
    //public int Total { get; set; }
}

// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class Link
{
    [JsonPropertyName("pagination")]
    public Pagination Pagination { get; set; }
}

public class Pagination
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    //[JsonPropertyName("per_page")]
    //public int PerPage { get; set; }

    //[JsonPropertyName("current_page")]
    //public int CurrentPage { get; set; }

    [JsonPropertyName("last_page")]
    public int LastPage { get; set; }

    //[JsonPropertyName("from")]
    //public int From { get; set; }

    //[JsonPropertyName("to")]
    //public int To { get; set; }

    //[JsonPropertyName("pages")]
    //public List<int> Pages { get; set; }
}

public class EntryDetails
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }

    //[JsonPropertyName("cost_center_id")]
    //public int CostCenterId { get; set; }

    //[JsonPropertyName("congressman_budget_id")]
    //public int CongressmanBudgetId { get; set; }

    [JsonPropertyName("object")]
    public string Object { get; set; }

    //[JsonPropertyName("to")]
    //public string To { get; set; }

    //[JsonPropertyName("entry_type_id")]
    //public int EntryTypeId { get; set; }

    [JsonPropertyName("document_number")]
    public string DocumentNumber { get; set; }

    //[JsonPropertyName("provider_id")]
    //public int ProviderId { get; set; }

    //[JsonPropertyName("verified_at")]
    //public DateTime VerifiedAt { get; set; }

    //[JsonPropertyName("verified_by_id")]
    //public int VerifiedById { get; set; }

    //[JsonPropertyName("analysed_at")]
    //public DateTime AnalysedAt { get; set; }

    //[JsonPropertyName("analysed_by_id")]
    //public int AnalysedById { get; set; }

    //[JsonPropertyName("published_at")]
    //public DateTime PublishedAt { get; set; }

    //[JsonPropertyName("published_by_id")]
    //public int PublishedById { get; set; }

    //[JsonPropertyName("created_at")]
    //public DateTime CreatedAt { get; set; }

    //[JsonPropertyName("created_by_id")]
    //public int CreatedById { get; set; }

    //[JsonPropertyName("updated_at")]
    //public DateTime UpdatedAt { get; set; }

    //[JsonPropertyName("updated_by_id")]
    //public int UpdatedById { get; set; }

    [JsonPropertyName("cost_center_name")]
    public string CostCenterName { get; set; }

    [JsonPropertyName("cost_center_code")]
    public string CostCenterCode { get; set; }

    //[JsonPropertyName("provider_name")]
    //public string ProviderName { get; set; }

    //[JsonPropertyName("provider_cpf_cnpj")]
    //public string ProviderCpfCnpj { get; set; }

    //[JsonPropertyName("provider_type")]
    //public string ProviderType { get; set; }

    [JsonPropertyName("entry_type_name")]
    public string EntryTypeName { get; set; }

    [JsonPropertyName("documents_count")]
    public int DocumentsCount { get; set; }

    //[JsonPropertyName("missing_verification")]
    //public bool MissingVerification { get; set; }

    //[JsonPropertyName("missing_analysis")]
    //public bool MissingAnalysis { get; set; }

    //[JsonPropertyName("comments_pendent")]
    //public int CommentsPendent { get; set; }

    //[JsonPropertyName("comments_pendent_unanalysed")]
    //public bool CommentsPendentUnanalysed { get; set; }

    //[JsonPropertyName("is_transport")]
    //public bool IsTransport { get; set; }

    //[JsonPropertyName("is_transport_or_credit")]
    //public bool IsTransportOrCredit { get; set; }

    //[JsonPropertyName("comments_count")]
    //public int CommentsCount { get; set; }

    //[JsonPropertyName("provider_is_blocked")]
    //public bool ProviderIsBlocked { get; set; }

    //[JsonPropertyName("provider")]
    //public Provider Provider { get; set; }

    //[JsonPropertyName("date_formatted")]
    //public string DateFormatted { get; set; }

    //[JsonPropertyName("value_formatted")]
    //public string ValueFormatted { get; set; }

    [JsonPropertyName("value_abs")]
    public double ValueAbs { get; set; }

    //[JsonPropertyName("cost_center_name_formatted")]
    //public string CostCenterNameFormatted { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("cpf_cnpj")]
    public string CpfCnpj { get; set; }

    //[JsonPropertyName("pendencies")]
    //public List<object> Pendencies { get; set; }
}

public class Entries
{
    //[JsonPropertyName("current_page")]
    //public int CurrentPage { get; set; }

    [JsonPropertyName("rows")]
    public List<EntryDetails> Data { get; set; }

    //[JsonPropertyName("first_page_url")]
    //public string FirstPageUrl { get; set; }

    //[JsonPropertyName("from")]
    //public int From { get; set; }

    //[JsonPropertyName("last_page")]
    //public int LastPage { get; set; }

    //[JsonPropertyName("last_page_url")]
    //public string LastPageUrl { get; set; }

    [JsonPropertyName("links")]
    public Link Links { get; set; }

    [JsonPropertyName("next_page_url")]
    public object NextPageUrl { get; set; }

    //[JsonPropertyName("path")]
    //public string Path { get; set; }

    //[JsonPropertyName("per_page")]
    //public int PerPage { get; set; }

    //[JsonPropertyName("prev_page_url")]
    //public object PrevPageUrl { get; set; }

    //[JsonPropertyName("to")]
    //public int To { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}
