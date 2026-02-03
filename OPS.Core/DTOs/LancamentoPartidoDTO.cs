using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class LancamentoPartidoDTO
    {
        [JsonPropertyName("id_partido")]
        public int IdPartido { get; set; }

        [JsonPropertyName("nome_partido")]
        public string NomePartido { get; set; }

        [JsonPropertyName("total_notas")]
        public string TotalNotas { get; set; }

        [JsonPropertyName("valor_total")]
        public string ValorTotal { get; set; }
    }
}
