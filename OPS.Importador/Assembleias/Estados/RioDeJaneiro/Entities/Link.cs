using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioDeJaneiro.Entities
{
    public class Link
    {
        [JsonPropertyName("pagination")]
        public Pagination Pagination { get; set; }
    }
}
