using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Parana
{
    public partial class ImportadorDespesasParana
    {
        private class Despesas
        {
            [JsonPropertyName("itensDespesa")]
            public List<ItensDespesa> ItensDespesa { get; set; }
        }
    }
}
