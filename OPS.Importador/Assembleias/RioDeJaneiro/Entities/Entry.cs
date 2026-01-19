using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.RioDeJaneiro.Entities
{
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
}
