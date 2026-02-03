using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class ImportacaoInfoDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("sigla")]
        public string Sigla { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("info")]
        public string Info { get; set; }

        [JsonPropertyName("ultima_despesa")]
        public string UltimaDespesa { get; set; }

        [JsonPropertyName("ultima_importacao")]
        public string UltimaImportacao { get; set; }
    }
}
