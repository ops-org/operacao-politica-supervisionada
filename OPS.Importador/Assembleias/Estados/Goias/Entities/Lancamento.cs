using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Goias.Entities
{
    public class Lancamento
    {
        [JsonPropertyName("fornecedor")]
        public FornecedorGoias Fornecedor { get; set; }
    }
}
