using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Estados.Para.Entities
{
    public class MeasureDescriptor
    {
        [JsonPropertyName("SummaryType")]
        public string SummaryType { get; set; }

        [JsonPropertyName("FilterString")]
        public object FilterString { get; set; }

        [JsonPropertyName("Calculation")]
        public object Calculation { get; set; }

        [JsonPropertyName("WindowDefinition")]
        public object WindowDefinition { get; set; }

        [JsonPropertyName("Expression")]
        public object Expression { get; set; }

        [JsonPropertyName("DataItemId")]
        public object DataItemId { get; set; }

        [JsonPropertyName("ID")]
        public string ID { get; set; }

        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("DataMember")]
        public string DataMember { get; set; }

        [JsonPropertyName("Format")]
        public Format Format { get; set; }

        [JsonPropertyName("FinalDataType")]
        public string FinalDataType { get; set; }

        [JsonPropertyName("DataType")]
        public string DataType { get; set; }
    }
}

