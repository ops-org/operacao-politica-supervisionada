using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Goias.Entities
{
    public class DeputadoGoias
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("partido")]
        public String Partido { get; set; }

        [JsonPropertyName("foto")]
        public string Foto { get; set; }

        [JsonPropertyName("twitter")]
        public string Twitter { get; set; }

        [JsonPropertyName("facebook")]
        public string Facebook { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("sala")]
        public string Sala { get; set; }
    }
}
