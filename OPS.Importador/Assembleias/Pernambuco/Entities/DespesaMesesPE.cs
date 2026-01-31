using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Pernambuco.Entities
{
    public class DespesaMesesPE
    {
        [JsonPropertyName("mes")]
        public string Mes { get; set; }
    }
}
