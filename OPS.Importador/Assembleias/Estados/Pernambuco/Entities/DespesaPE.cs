using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Pernambuco.Entities
{
    public class DespesaPE
    {
        [JsonPropertyName("rubrica")]
        public string Rubrica { get; set; }

        [JsonPropertyName("sequencial")]
        public string Sequencial { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonPropertyName("cnpj")]
        public string Cnpj { get; set; }

        [JsonPropertyName("empresa")]
        public string Empresa { get; set; }

        [JsonPropertyName("valor")]
        public string Valor { get; set; }
    }
}
