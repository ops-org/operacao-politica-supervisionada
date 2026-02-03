using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class GraficoBarraDTO
    {
        [JsonPropertyName("categories")]
        public List<int> Categories { get; set; }

        [JsonPropertyName("series")]
        public List<decimal> Series { get; set; }
    }
}
