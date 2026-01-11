using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.MinasGerais.Entities
{
    public class Filiaco
    {
        [JsonPropertyName("dataInicio")]
        public DateTime DataInicio { get; set; }

        [JsonPropertyName("dataTermino")]
        public DateTime DataTermino { get; set; }

        [JsonPropertyName("partido")]
        public Partido Partido { get; set; }
    }
}
