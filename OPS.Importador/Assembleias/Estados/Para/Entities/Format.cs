using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Estados.Para.Entities
{
    public class Format
    {
        [JsonPropertyName("DataType")]
        public string DataType { get; set; }

        [JsonPropertyName("NumericFormat")]
        public object NumericFormat { get; set; }

        [JsonPropertyName("DateTimeFormat")]
        public DateTimeFormat DateTimeFormat { get; set; }
    }
}

