using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class FornecedorDetalheDTO
    {
        [JsonPropertyName("fornecedor")]
        public FornecedorInfoDTO Fornecedor { get; set; }

        [JsonPropertyName("quadro_societario")]
        public IEnumerable<QuadroSocietarioDTO> QuadroSocietario { get; set; }
    }
}
