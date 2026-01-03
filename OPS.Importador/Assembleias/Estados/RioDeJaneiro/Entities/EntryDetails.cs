using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioDeJaneiro.Entities
{
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
}
