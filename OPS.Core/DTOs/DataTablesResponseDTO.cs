using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class DataTablesResponseDTO<T>
    {
        [JsonPropertyName("records_total")]
        public int recordsTotal { get; set; }

        [JsonPropertyName("records_filtered")]
        public int recordsFiltered { get; set; }

        public List<T> data { get; set; }
    }
}
