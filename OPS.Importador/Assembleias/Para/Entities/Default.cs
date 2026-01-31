using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Para.Entities
{
    public class Default
    {
        [JsonPropertyName("Level")]
        public int Level { get; set; }

        [JsonPropertyName("DateTimeGroupInterval")]
        public string DateTimeGroupInterval { get; set; }

        [JsonPropertyName("TextGroupInterval")]
        public string TextGroupInterval { get; set; }

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

