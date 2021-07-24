using Newtonsoft.Json;

namespace OPS.Core.Entity
{
    public class TelegramMessage
    {
        [JsonProperty("parse_mode")]
        public string ParseMode { get; set; }

        [JsonProperty("chat_id")]
        public string ChatId { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
