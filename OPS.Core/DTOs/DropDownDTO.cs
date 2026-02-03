using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class DropDownDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("help_text")]
        public string HelpText { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("tokens")]
        public string[] Tokens { get; set; }
    }
}