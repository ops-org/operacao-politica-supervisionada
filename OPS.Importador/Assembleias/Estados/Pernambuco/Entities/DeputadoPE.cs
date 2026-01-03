using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Pernambuco.Entities
{
    public class DeputadoPE
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("partido")]
        public string Partido { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("foto")]
        public string Foto { get; set; }

        [JsonPropertyName("to_ascii")]
        public string ToAscii { get; set; }
    }
}
