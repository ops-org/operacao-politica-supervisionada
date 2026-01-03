using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.MinasGerais.Entities
{
    public class Telefone
    {
        [JsonPropertyName("ddd")]
        public string Ddd { get; set; }

        [JsonPropertyName("numero")]
        public string Numero { get; set; }

        [JsonPropertyName("fax")]
        public string Fax { get; set; }
    }
}
