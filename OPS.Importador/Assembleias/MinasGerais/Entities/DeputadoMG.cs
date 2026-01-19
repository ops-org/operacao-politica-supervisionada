using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.MinasGerais.Entities
{
    public class DeputadoMG
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("partido")]
        public string Partido { get; set; }

        [JsonPropertyName("tagLocalizacao")]
        public int TagLocalizacao { get; set; }
    }
}
