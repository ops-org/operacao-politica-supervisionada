using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.RioGrandeDoSul.Entities
{
    public class DeputadosRS
    {
        [JsonPropertyName("lista")]
        public List<DeputadoRS> Lista { get; set; }
    }
}
