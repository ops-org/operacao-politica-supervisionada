using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class LancamentoParlamentarDTO
    {
        [JsonPropertyName("id_parlamentar")]
        public int IdParlamentar { get; set; }

        [JsonPropertyName("nome_parlamentar")]
        public string NomeParlamentar { get; set; }

        [JsonPropertyName("sigla_estado")]
        public string SiglaEstado { get; set; }

        [JsonPropertyName("sigla_partido")]
        public string SiglaPartido { get; set; }

        [JsonPropertyName("total_notas")]
        public int TotalNotas { get; set; }

        [JsonPropertyName("valor_total")]
        public string ValorTotal { get; set; }
    }
}
