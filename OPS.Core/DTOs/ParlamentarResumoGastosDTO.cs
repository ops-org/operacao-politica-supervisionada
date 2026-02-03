using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class ParlamentarResumoGastosDTO
    {
        [JsonPropertyName("deputados_federais")]
        public IEnumerable<ParlamentarResumoDTO> DeputadosFederais { get; set; }

        [JsonPropertyName("deputados_estaduais")]
        public IEnumerable<ParlamentarResumoDTO> DeputadosEstaduais { get; set; }

        [JsonPropertyName("senadores")]
        public IEnumerable<ParlamentarResumoDTO> Senadores { get; set; }
    }
}
