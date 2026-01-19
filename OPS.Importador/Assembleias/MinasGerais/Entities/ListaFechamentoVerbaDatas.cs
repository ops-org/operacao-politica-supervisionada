using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.MinasGerais.Entities
{
    public class ListaFechamentoVerbaDatas
    {
        [JsonPropertyName("listaFechamentoVerba")]
        public List<ListaFechamentoVerba> ListaFechamentoVerba { get; set; }
    }
}
