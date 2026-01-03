using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul.Entities
{
    public class ListaMes
    {

        [JsonPropertyName("mes")]
        public int Mes { get; set; }

    }
}
