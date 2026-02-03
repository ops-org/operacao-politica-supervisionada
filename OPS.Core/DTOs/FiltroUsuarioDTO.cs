using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class FiltroUsuarioDTO
    {
        [JsonPropertyName("filter")]
        public string Filter { get; set; }

        [JsonPropertyName("sorting")]
        public string Sorting { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("uf")]
        public string Uf { get; set; }

        public FiltroUsuarioDTO()
        {
            this.Count = 1;
            this.Page = 1;
        }
    }
}