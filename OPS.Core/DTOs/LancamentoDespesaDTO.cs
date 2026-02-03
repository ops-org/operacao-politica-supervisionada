using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class LancamentoDespesaDTO
    {
        [JsonPropertyName("id_despesa_tipo")]
        public int IdDespesaTipo { get; set; }

        [JsonPropertyName("descricao")]
        public string Descricao { get; set; }

        [JsonPropertyName("total_notas")]
        public string TotalNotas { get; set; }

        [JsonPropertyName("valor_total")]
        public string ValorTotal { get; set; }
    }
}
