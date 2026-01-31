using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Pernambuco.Entities
{
    public class DespesaDocumentosPE
    {
        [JsonPropertyName("docid")]
        public string Docid { get; set; }

        [JsonPropertyName("numero")]
        public string Numero { get; set; }

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; }

        [JsonPropertyName("ano")]
        public string Ano { get; set; }

        [JsonPropertyName("deputado")]
        public string Deputado { get; set; }

        [JsonPropertyName("mes")]
        public string Mes { get; set; }

        [JsonPropertyName("total")]
        public string Total { get; set; }
    }
}
