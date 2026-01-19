using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.RioGrandeDoSul.Entities
{
    public class ParlamentaresListarMes
    {

        [JsonPropertyName("lista")]
        public List<ListaMes> Lista { get; set; }

    }
}
