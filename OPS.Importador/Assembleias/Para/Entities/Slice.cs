using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Para.Entities
{
    public class Slice
    {
        [JsonPropertyName("KeyIds")]
        public List<string> KeyIds { get; set; }

        //[JsonPropertyName("ValueIds")]
        //public ValueIds ValueIds { get; set; }

        [JsonPropertyName("Data")]
        public Dictionary<string, Dictionary<string, decimal?>> Data { get; set; }
    }
}
