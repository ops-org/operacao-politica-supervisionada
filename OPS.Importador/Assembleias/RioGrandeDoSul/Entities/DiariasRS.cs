using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.RioGrandeDoSul.Entities
{
    public class DiariasRS
    {
        [JsonPropertyName("lista")]
        public List<ListaDiarias> Lista { get; set; }
    }
}
