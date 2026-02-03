using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class ParlamentarListaDTO
    {
        [JsonPropertyName("id_parlamentar")]
        public int IdParlamentar { get; set; }

        [JsonPropertyName("nome_parlamentar")]
        public string NomeParlamentar { get; set; }

        [JsonPropertyName("nome_civil")]
        public string NomeCivil { get; set; }

        [JsonPropertyName("valor_total_ceap")]
        public string ValorTotalCeap { get; set; }

        [JsonPropertyName("valor_total_remuneracao")]
        public string ValorTotalRemuneracao { get; set; }

        [JsonPropertyName("sigla_partido")]
        public string SiglaPartido { get; set; }

        [JsonPropertyName("nome_partido")]
        public string NomePartido { get; set; }

        [JsonPropertyName("sigla_estado")]
        public string SiglaEstado { get; set; }

        [JsonPropertyName("nome_estado")]
        public string NomeEstado { get; set; }

        [JsonPropertyName("ativo")]
        public bool? Ativo { get; set; }
    }
}
