using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.MinasGerais.Entities
{
    public class ListaFechamentoVerba
    {
        [JsonPropertyName("idDeputado")]
        public int IdDeputado { get; set; }

        [JsonPropertyName("dataReferencia")]
        public string DataReferencia { get; set; }
    }
}
