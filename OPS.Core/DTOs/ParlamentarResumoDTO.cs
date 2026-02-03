using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class ParlamentarResumoDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome_parlamentar")]
        public string NomeParlamentar { get; set; }

        [JsonPropertyName("valor_total")]
        public string ValorTotal { get; set; }

        [JsonPropertyName("sigla_partido_estado")]
        public string SiglaPartidoEstado { get; set; }
    }
}
