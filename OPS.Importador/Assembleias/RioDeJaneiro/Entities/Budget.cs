using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.RioDeJaneiro.Entities
{
    public class Budget
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("created_by_id")]
        public int CreatedById { get; set; }

        [JsonPropertyName("updated_by_id")]
        public int UpdatedById { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("federal_budget_id")]
        public int FederalBudgetId { get; set; }
    }
}

