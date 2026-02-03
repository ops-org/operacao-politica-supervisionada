using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class FiltroSecretarioDTO
    {
        //public string sorting { get; set; }
        //public int count { get; set; }
        //public int page { get; set; }

        [JsonPropertyName("draw")]
        public int Draw { get; set; }

        [JsonPropertyName("start")]
        public int Start { get; set; }

        [JsonPropertyName("length")]
        public int Length { get; set; }

        [JsonPropertyName("order")]
        public Dictionary<string, object> Order { get; set; }

        [JsonPropertyName("nome_parlamentar")]
        public string NomeParlamentar { get; set; }

        [JsonPropertyName("ativo")]
        public bool Ativo { get; set; }

        public FiltroSecretarioDTO()
        {
            this.Start = 0;
            this.Length = 500;
        }
    }
}
