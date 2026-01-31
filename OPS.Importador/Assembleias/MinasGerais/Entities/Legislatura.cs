using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.MinasGerais.Entities
{
    public class Legislatura
    {
        [JsonPropertyName("numeroLegislatura")]
        public int NumeroLegislatura { get; set; }

        [JsonPropertyName("inicioLegislatura")]
        public DateTime InicioLegislatura { get; set; }

        [JsonPropertyName("terminoLegislatura")]
        public DateTime TerminoLegislatura { get; set; }

        [JsonPropertyName("tipoMandato")]
        public string TipoMandato { get; set; }

        [JsonPropertyName("situacoes")]
        public List<Situaco> Situacoes { get; set; }

        [JsonPropertyName("inicioExercicio")]
        public DateTime InicioExercicio { get; set; }

        [JsonPropertyName("terminoExercicio")]
        public DateTime TerminoExercicio { get; set; }

        [JsonPropertyName("deputadoAfastado")]
        public string DeputadoAfastado { get; set; }
    }
}
