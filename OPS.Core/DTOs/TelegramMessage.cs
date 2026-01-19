using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class TelegramMessage
    {
        [JsonPropertyName("parse_mode")]
        public string ParseMode { get; set; }

        [JsonPropertyName("chat_id")]
        public string ChatId { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
