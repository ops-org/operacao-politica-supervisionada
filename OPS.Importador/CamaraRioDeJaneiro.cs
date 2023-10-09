using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using RestSharp;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace OPS.Importador
{
    /// <summary>
    /// Assembleia Legislativa da
    /// 
    /// </summary>
    public class CamaraRioDeJaneiro : ImportadorCotaParlamentarBase
    {
        public CamaraRioDeJaneiro(ILogger<CamaraRioDeJaneiro> logger, IConfiguration configuration, IDbConnection connection) :
            base("RJ", logger, configuration, connection)
        {
        }

        public override void ImportarParlamentares()
        {
            var query = "{\"filter\":{\"text\":null,\"checkboxes\":{\"withMandate\":false,\"withoutMandate\":false,\"withPendency\":false,\"withoutPendency\":false,\"unread\":false,\"joined\":true,\"notJoined\":false,\"filler\":false},\"selects\":{\"filler\":false}},\"pagination\":{\"total\":83,\"per_page\":\"250\",\"current_page\":1,\"last_page\":9,\"from\":1,\"to\":10,\"pages\":[1,2,3,4,5]}}";
            var address = "https://docigp.alerj.rj.gov.br/api/v1/congressmen?query=" + System.Net.WebUtility.UrlEncode(query);
            var restClient = new RestClient();
            restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            var request = new RestRequest(address, Method.GET);
            request.AddHeader("Accept", "application/json");

            IRestResponse resParlamentares = restClient.ExecuteWithAutoRetry(request);
            DeputadosRJ objDeputadosRJ = JsonSerializer.Deserialize<DeputadosRJ>(resParlamentares.Content);

            foreach (var parlamentar in objDeputadosRJ.Rows)
            {
                var IdPartido = connection.GetList<Core.Entity.Partido>(new { sigla = parlamentar.Party.Code }).FirstOrDefault()?.Id;
                if (IdPartido == null)
                    throw new Exception("Partido Inexistenete");

                var deputado = new DeputadoEstadual();
                deputado.Matricula = (UInt32)parlamentar.Id;
                //deputado.UrlPerfil = $"https://ww4.al.rs.gov.br/deputados/{parlamentar.IdDeputado}";
                deputado.NomeParlamentar = parlamentar.User.Name;
                deputado.IdEstado = (ushort)idEstado;
                deputado.IdPartido = IdPartido.Value;
                deputado.Email = parlamentar.User.Email;
                //deputado.Telefone = parlamentar.TelefoneDeputado;
                deputado.UrlFoto = parlamentar.PhotoUrlLinkable;

                var IdDeputado = connection.GetList<DeputadoEstadual>(new { id_estado = idEstado, matricula = parlamentar.Id }).FirstOrDefault()?.Id;
                if (IdDeputado == null)
                    connection.Insert(deputado);
                else
                {
                    deputado.Id = IdDeputado.Value;
                    connection.Update(deputado);
                }
            }
        }

        public override void ImportarArquivoDespesas(int ano)
        {
            ImportarArquivoDespesasAsync(ano).Wait();
        }

        public async Task ImportarArquivoDespesasAsync(int ano)
        {
            logger.LogInformation("Consultando ano {Ano}!", ano);
            //LimpaDespesaTemporaria();

            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

            var query = "{\"filter\":{\"text\":null,\"checkboxes\":{\"filler\":false},\"selects\":{\"filler\":false}},\"pagination\":{\"total\":18,\"per_page\":\"250\",\"current_page\":1,\"last_page\":2,\"from\":1,\"to\":10,\"pages\":[1,2]}}";
            var address = "https://docigp.alerj.rj.gov.br/api/v1/congressmen/81/budgets?query=" + System.Net.WebUtility.UrlEncode(query);
            var restClient = new RestClient();
            restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            var request = new RestRequest(address, Method.GET);
            request.AddHeader("Accept", "application/json");

            IRestResponse resParlamentares = await restClient.ExecuteAsync(request);
            DeputadosRJ objDeputadosRJ = JsonSerializer.Deserialize<DeputadosRJ>(resParlamentares.Content);

            foreach (var parlamentar in objDeputadosRJ.Rows)
            {
                var IdPartido = connection.GetList<Core.Entity.Partido>(new { sigla = parlamentar.Party.Code }).FirstOrDefault()?.Id;
                if (IdPartido == null)
                    throw new Exception("Partido Inexistenete");

                var deputado = new DeputadoEstadual();
                deputado.Matricula = (UInt32)parlamentar.Id;
                //deputado.UrlPerfil = $"https://ww4.al.rs.gov.br/deputados/{parlamentar.IdDeputado}";
                deputado.NomeParlamentar = parlamentar.User.Name;
                deputado.IdEstado = (ushort)idEstado;
                deputado.IdPartido = IdPartido.Value;
                deputado.Email = parlamentar.User.Email;
                //deputado.Telefone = parlamentar.TelefoneDeputado;
                deputado.UrlFoto = parlamentar.PhotoUrlLinkable;

                var IdDeputado = connection.GetList<DeputadoEstadual>(new { id_estado = idEstado, matricula = parlamentar.Id }).FirstOrDefault()?.Id;
                if (IdDeputado == null)
                    connection.Insert(deputado);
                else
                {
                    deputado.Id = IdDeputado.Value;
                    connection.Update(deputado);
                }
            }


            //    var objCamaraEstadualDespesaTemp = new CamaraEstadualDespesaTemp()
            //    {
            //        Ano = (short)ano,
            //        Documento = colunas[0].QuerySelector("span").TextContent.Trim() + "/" + colunas[1].TextContent.Trim(),
            //        DataEmissao = Convert.ToDateTime("01/" + colunas[2].TextContent.Trim(), cultureInfo),
            //        Nome = colunas[3].TextContent.Trim(),
            //        TipoDespesa = colunas[4].TextContent.Trim(),
            //    };

            //    var documentDetalhes = await context.OpenAsync(linkDetalhes);
            //    if (documentDetalhes.StatusCode != HttpStatusCode.OK)
            //    {
            //        logger.LogError($"{linkDetalhes} {documentDetalhes.StatusCode}");
            //    };

            //    var despesasDetalhes = documentDetalhes.QuerySelectorAll(".tabela-cab tbody tr");
            //    foreach (var detalhes in despesasDetalhes)
            //    {
            //        // CATEGORIA	Nº NOTA/RECIBO	CPF/CNPJ	NOME DO FORNECEDOR	VALOR
            //        var colunasDetalhes = detalhes.QuerySelectorAll("td");

            //        objCamaraEstadualDespesaTemp.Documento = processo + "/" + colunasDetalhes[1].TextContent.Trim();
            //        objCamaraEstadualDespesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(colunasDetalhes[2].TextContent.Trim());
            //        objCamaraEstadualDespesaTemp.Empresa = colunasDetalhes[3].TextContent.Trim();
            //        objCamaraEstadualDespesaTemp.Valor = Convert.ToDecimal(colunasDetalhes[4].TextContent.Replace("R$", "").Trim(), cultureInfo);
            //    }


            //    connection.Insert(objCamaraEstadualDespesaTemp);
            //}


            //if (document.QuerySelector(".paginate-button-next").ClassList.Contains("disabled")) break;
        }

    }

    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    //public class Checkboxes
    //{
    //    [JsonPropertyName("withMandate")]
    //    public bool WithMandate { get; set; }

    //    [JsonPropertyName("withoutMandate")]
    //    public bool WithoutMandate { get; set; }

    //    [JsonPropertyName("withPendency")]
    //    public bool WithPendency { get; set; }

    //    [JsonPropertyName("withoutPendency")]
    //    public bool WithoutPendency { get; set; }

    //    [JsonPropertyName("unread")]
    //    public bool Unread { get; set; }

    //    [JsonPropertyName("joined")]
    //    public bool Joined { get; set; }

    //    [JsonPropertyName("notJoined")]
    //    public bool NotJoined { get; set; }

    //    [JsonPropertyName("filler")]
    //    public bool Filler { get; set; }
    //}

    //public class Filter
    //{
    //    [JsonPropertyName("text")]
    //    public object Text { get; set; }

    //    [JsonPropertyName("checkboxes")]
    //    public Checkboxes Checkboxes { get; set; }

    //    [JsonPropertyName("selects")]
    //    public Selects Selects { get; set; }
    //}

    //public class Links
    //{
    //    [JsonPropertyName("pagination")]
    //    public Pagination Pagination { get; set; }
    //}

    //public class Pagination
    //{
    //    [JsonPropertyName("total")]
    //    public int Total { get; set; }

    //    [JsonPropertyName("per_page")]
    //    public string PerPage { get; set; }

    //    [JsonPropertyName("current_page")]
    //    public int CurrentPage { get; set; }

    //    [JsonPropertyName("last_page")]
    //    public int LastPage { get; set; }

    //    [JsonPropertyName("from")]
    //    public int From { get; set; }

    //    [JsonPropertyName("to")]
    //    public int To { get; set; }

    //    [JsonPropertyName("pages")]
    //    public List<int> Pages { get; set; }
    //}

    public class Partido
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        //[JsonPropertyName("created_by_id")]
        //public int CreatedById { get; set; }

        //[JsonPropertyName("updated_by_id")]
        //public int? UpdatedById { get; set; }

        //[JsonPropertyName("created_at")]
        //public DateTime CreatedAt { get; set; }

        //[JsonPropertyName("updated_at")]
        //public DateTime UpdatedAt { get; set; }
    }

    public class DeputadosRJ
    {
        //[JsonPropertyName("links")]
        //public Links Links { get; set; }

        //[JsonPropertyName("filter")]
        //public Filter Filter { get; set; }

        [JsonPropertyName("rows")]
        public List<DespesaMensal> Rows { get; set; }
    }

    public class DespesaMensal
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        //[JsonPropertyName("remote_id")]
        //public int RemoteId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        //[JsonPropertyName("nickname")]
        //public string Nickname { get; set; }

        //[JsonPropertyName("party_id")]
        //public int PartyId { get; set; }

        //[JsonPropertyName("photo_url")]
        //public string PhotoUrl { get; set; }

        //[JsonPropertyName("thumbnail_url")]
        //public string ThumbnailUrl { get; set; }

        //[JsonPropertyName("department_id")]
        //public int DepartmentId { get; set; }

        //[JsonPropertyName("created_by_id")]
        //public int CreatedById { get; set; }

        //[JsonPropertyName("updated_by_id")]
        //public object UpdatedById { get; set; }

        //[JsonPropertyName("created_at")]
        //public DateTime CreatedAt { get; set; }

        //[JsonPropertyName("updated_at")]
        //public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("has_mandate")]
        public bool HasMandate { get; set; }

        //[JsonPropertyName("has_pendency")]
        //public bool HasPendency { get; set; }

        //[JsonPropertyName("is_published")]
        //public bool IsPublished { get; set; }

        //[JsonPropertyName("thumbnail_url_linkable")]
        //public string ThumbnailUrlLinkable { get; set; }

        [JsonPropertyName("photo_url_linkable")]
        public string PhotoUrlLinkable { get; set; }

        [JsonPropertyName("party")]
        public Partido Party { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }
    }

    //public class Selects
    //{
    //    [JsonPropertyName("filler")]
    //    public bool Filler { get; set; }
    //}

    public class User
    {
        //[JsonPropertyName("id")]
        //public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

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
    }



    // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
    //public class Budget
    //{
    //    [JsonPropertyName("id")]
    //    public int Id { get; set; }

    //    [JsonPropertyName("date")]
    //    public DateTime Date { get; set; }

    //    [JsonPropertyName("federal_value")]
    //    public string FederalValue { get; set; }

    //    [JsonPropertyName("percentage")]
    //    public string Percentage { get; set; }

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
    //}

    //public class Checkboxes
    //{
    //    [JsonPropertyName("filler")]
    //    public bool Filler { get; set; }
    //}

    //public class Congressman
    //{
    //    [JsonPropertyName("id")]
    //    public int Id { get; set; }

    //    [JsonPropertyName("remote_id")]
    //    public int RemoteId { get; set; }

    //    [JsonPropertyName("name")]
    //    public string Name { get; set; }

    //    [JsonPropertyName("nickname")]
    //    public string Nickname { get; set; }

    //    [JsonPropertyName("party_id")]
    //    public int PartyId { get; set; }

    //    [JsonPropertyName("photo_url")]
    //    public string PhotoUrl { get; set; }

    //    [JsonPropertyName("thumbnail_url")]
    //    public string ThumbnailUrl { get; set; }

    //    [JsonPropertyName("department_id")]
    //    public int DepartmentId { get; set; }

    //    [JsonPropertyName("created_by_id")]
    //    public int CreatedById { get; set; }

    //    [JsonPropertyName("updated_by_id")]
    //    public object UpdatedById { get; set; }

    //    [JsonPropertyName("created_at")]
    //    public DateTime CreatedAt { get; set; }

    //    [JsonPropertyName("updated_at")]
    //    public DateTime UpdatedAt { get; set; }

    //    [JsonPropertyName("thumbnail_url_linkable")]
    //    public string ThumbnailUrlLinkable { get; set; }

    //    [JsonPropertyName("photo_url_linkable")]
    //    public string PhotoUrlLinkable { get; set; }

    //    [JsonPropertyName("party")]
    //    public Partido Party { get; set; }

    //    [JsonPropertyName("user")]
    //    public User User { get; set; }
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

    //public class Filter
    //{
    //    [JsonPropertyName("text")]
    //    public object Text { get; set; }

    //    [JsonPropertyName("checkboxes")]
    //    public Checkboxes Checkboxes { get; set; }

    //    [JsonPropertyName("selects")]
    //    public Selects Selects { get; set; }
    //}

    public class Links
    {
        [JsonPropertyName("pagination")]
        public Pagination Pagination { get; set; }
    }

    public class Pagination
    {
        //[JsonPropertyName("total")]
        //public int Total { get; set; }

        //[JsonPropertyName("per_page")]
        //public string PerPage { get; set; }

        [JsonPropertyName("current_page")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("last_page")]
        public int LastPage { get; set; }

        //[JsonPropertyName("from")]
        //public int From { get; set; }

        //[JsonPropertyName("to")]
        //public int To { get; set; }

        //[JsonPropertyName("pages")]
        //public List<int> Pages { get; set; }
    }

    //public class Partido
    //{
    //    [JsonPropertyName("id")]
    //    public int Id { get; set; }

    //    [JsonPropertyName("code")]
    //    public string Code { get; set; }

    //    [JsonPropertyName("name")]
    //    public string Name { get; set; }

    //    [JsonPropertyName("created_by_id")]
    //    public int CreatedById { get; set; }

    //    [JsonPropertyName("updated_by_id")]
    //    public object UpdatedById { get; set; }

    //    [JsonPropertyName("created_at")]
    //    public DateTime CreatedAt { get; set; }

    //    [JsonPropertyName("updated_at")]
    //    public DateTime UpdatedAt { get; set; }
    //}

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

        //[JsonPropertyName("filter")]
        //public Filter Filter { get; set; }

        [JsonPropertyName("rows")]
        public List<DespesaMensalRJ> Rows { get; set; }
    }

    public class DespesaMensalRJ
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        //[JsonPropertyName("congressman_legislature_id")]
        //public int CongressmanLegislatureId { get; set; }

        //[JsonPropertyName("budget_id")]
        //public int BudgetId { get; set; }

        //[JsonPropertyName("percentage")]
        //public string Percentage { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

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

        //[JsonPropertyName("entries_count")]
        //public int EntriesCount { get; set; }

        //[JsonPropertyName("sum_credit")]
        //public string SumCredit { get; set; }

        //[JsonPropertyName("sum_debit")]
        //public string SumDebit { get; set; }

        //[JsonPropertyName("sum_i")]
        //public object SumI { get; set; }

        //[JsonPropertyName("sum_ii")]
        //public object SumIi { get; set; }

        //[JsonPropertyName("sum_iii")]
        //public string SumIii { get; set; }

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
        //public string SumXi { get; set; }

        //[JsonPropertyName("has_refund")]
        //public bool HasRefund { get; set; }

        //[JsonPropertyName("budget")]
        //public Budget Budget { get; set; }

        //[JsonPropertyName("congressman_legislature")]
        //public CongressmanLegislature CongressmanLegislature { get; set; }

        [JsonPropertyName("entries")]
        public List<Entry> Entries { get; set; }

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

    //public class Selects
    //{
    //    [JsonPropertyName("filler")]
    //    public bool Filler { get; set; }
    //}

    //public class User
    //{
    //    [JsonPropertyName("id")]
    //    public int Id { get; set; }

    //    [JsonPropertyName("name")]
    //    public string Name { get; set; }

    //    [JsonPropertyName("email")]
    //    public string Email { get; set; }

    //    [JsonPropertyName("email_verified_at")]
    //    public object EmailVerifiedAt { get; set; }

    //    [JsonPropertyName("per_page")]
    //    public int PerPage { get; set; }

    //    [JsonPropertyName("created_by_id")]
    //    public object CreatedById { get; set; }

    //    [JsonPropertyName("updated_by_id")]
    //    public object UpdatedById { get; set; }

    //    [JsonPropertyName("created_at")]
    //    public DateTime CreatedAt { get; set; }

    //    [JsonPropertyName("updated_at")]
    //    public DateTime UpdatedAt { get; set; }

    //    [JsonPropertyName("username")]
    //    public string Username { get; set; }

    //    [JsonPropertyName("department_id")]
    //    public int DepartmentId { get; set; }

    //    [JsonPropertyName("congressman_id")]
    //    public int CongressmanId { get; set; }

    //    [JsonPropertyName("last_login_at")]
    //    public string LastLoginAt { get; set; }
    //}



}
