using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.RioGrandeDoSul.Entities
{
    public class ParlamentaresListarGabinete
    {

        [JsonPropertyName("lista")]
        public List<Gabinete> Lista { get; set; }

    }
}
