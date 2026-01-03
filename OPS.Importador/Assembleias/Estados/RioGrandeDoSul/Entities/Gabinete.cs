using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul.Entities
{
    public class Gabinete
    {

        [JsonPropertyName("codProponente")]
        public int Codigo { get; set; }

        [JsonPropertyName("nomeCota")]
        public string Parlamentar { get; set; }

    }
}
