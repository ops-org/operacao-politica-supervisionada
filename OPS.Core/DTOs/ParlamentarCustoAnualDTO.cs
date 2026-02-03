using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class ParlamentarCustoAnualDTO
    {
        [JsonPropertyName("ano")]
        public int Ano { get; set; }

        [JsonPropertyName("cota_parlamentar")]
        public decimal CotaParlamentar { get; set; }

        [JsonPropertyName("verba_gabinete")]
        public decimal VerbaGabinete { get; set; }

        [JsonPropertyName("salario_patronal")]
        public decimal SalarioPatronal { get; set; }

        [JsonPropertyName("auxilio_moradia")]
        public decimal AuxilioMoradia { get; set; }

        [JsonPropertyName("auxilio_saude")]
        public decimal AuxilioSaude { get; set; }

        [JsonPropertyName("diarias")]
        public decimal Diarias { get; set; }
    }
}