using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.RioGrandeDoSul.Entities
{
    public class ListaMes
    {

        [JsonPropertyName("mes")]
        public int Mes { get; set; }

    }
}
