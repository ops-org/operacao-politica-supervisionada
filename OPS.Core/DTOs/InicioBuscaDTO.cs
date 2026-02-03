using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class InicioBuscaDTO
    {
        [JsonPropertyName("deputado_federal")]
        public List<ParlamentarListaDTO> DeputadoFederal { get; set; }

        [JsonPropertyName("deputado_estadual")]
        public List<ParlamentarListaDTO> DeputadoEstadual { get; set; }

        [JsonPropertyName("senador")]
        public List<ParlamentarListaDTO> Senador { get; set; }

        [JsonPropertyName("fornecedor")]
        public List<FornecedorListaDTO> Fornecedor { get; set; }
    }
}
