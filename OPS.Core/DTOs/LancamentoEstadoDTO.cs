using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class LancamentoEstadoDTO
    {
        [JsonPropertyName("id_estado")]
        public int IdEstado { get; set; }

        [JsonPropertyName("nome_estado")]
        public string NomeEstado { get; set; }

        [JsonPropertyName("total_notas")]
        public int TotalNotas { get; set; }

        [JsonPropertyName("valor_total")]
        public string ValorTotal { get; set; }
    }
}
