using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class FuncionarioPesquisaDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
