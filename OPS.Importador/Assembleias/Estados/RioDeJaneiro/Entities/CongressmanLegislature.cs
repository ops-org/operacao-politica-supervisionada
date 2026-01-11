using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Estados.RioDeJaneiro.Entities
{
    public class CongressmanLegislature
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("congressman_id")]
        public int CongressmanId { get; set; }

        [JsonPropertyName("legislature_id")]
        public int LegislatureId { get; set; }

        [JsonPropertyName("started_at")]
        public DateTime StartedAt { get; set; }

        [JsonPropertyName("ended_at")]
        public object EndedAt { get; set; }

        [JsonPropertyName("created_by_id")]
        public int CreatedById { get; set; }

        [JsonPropertyName("updated_by_id")]
        public object UpdatedById { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("congressman")]
        public Congressman Congressman { get; set; }
    }
}

