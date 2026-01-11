using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Para.Entities
{
    public class EncodeMaps
    {
        [JsonPropertyName("DataItem1")]
        public List<DateTime> DataItem1 { get; set; }

        [JsonPropertyName("DataItem0")]
        public List<string> DataItem0 { get; set; }

        [JsonPropertyName("DataItem5")]
        public List<string> DataItem5 { get; set; }

        [JsonPropertyName("DataItem2")]
        public List<string> DataItem2 { get; set; }

        [JsonPropertyName("DataItem3")]
        public List<object> DataItem3 { get; set; }
    }
}
