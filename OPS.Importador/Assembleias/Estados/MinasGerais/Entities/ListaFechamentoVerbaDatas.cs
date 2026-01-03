using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.MinasGerais.Entities
{
    public class ListaFechamentoVerbaDatas
    {
        [JsonPropertyName("listaFechamentoVerba")]
        public List<ListaFechamentoVerba> ListaFechamentoVerba { get; set; }
    }
}
