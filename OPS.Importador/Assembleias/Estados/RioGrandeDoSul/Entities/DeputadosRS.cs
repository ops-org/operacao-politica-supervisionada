using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul.Entities
{
    public class DeputadosRS
    {
        [JsonPropertyName("lista")]
        public List<DeputadoRS> Lista { get; set; }
    }
}
