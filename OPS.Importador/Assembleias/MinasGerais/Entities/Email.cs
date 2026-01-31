using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.MinasGerais.Entities
{
    public class Email
    {
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; }

        [JsonPropertyName("endereco")]
        public string Endereco { get; set; }
    }
}
