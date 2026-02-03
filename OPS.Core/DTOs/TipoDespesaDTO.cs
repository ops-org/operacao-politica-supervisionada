using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class TipoDespesaDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
