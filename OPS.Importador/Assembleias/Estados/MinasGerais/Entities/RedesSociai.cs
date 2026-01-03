using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.MinasGerais.Entities
{
    public class RedesSociai
    {
        [JsonPropertyName("redeSocial")]
        public RedeSocial RedeSocial { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
