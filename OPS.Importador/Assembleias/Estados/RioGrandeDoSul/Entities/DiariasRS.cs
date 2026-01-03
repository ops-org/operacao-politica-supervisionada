using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul.Entities
{
    public class DiariasRS
    {
        [JsonPropertyName("lista")]
        public List<ListaDiarias> Lista { get; set; }
    }
}
