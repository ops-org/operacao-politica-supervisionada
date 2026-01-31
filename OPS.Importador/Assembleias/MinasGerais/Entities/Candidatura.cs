using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.MinasGerais.Entities
{
    public class Candidatura
    {
        [JsonPropertyName("legislatura")]
        public int Legislatura { get; set; }

        [JsonPropertyName("numeroCandidato")]
        public int NumeroCandidato { get; set; }
    }
}
