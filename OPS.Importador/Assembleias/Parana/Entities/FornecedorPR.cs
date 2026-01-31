using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Parana
{
    public partial class ImportadorDespesasParana
    {
        private class FornecedorPR
        {
            [JsonPropertyName("codigo")]
            public int Codigo { get; set; }

            [JsonPropertyName("nome")]
            public string Nome { get; set; }

            [JsonPropertyName("documento")]
            public string Documento { get; set; }

            [JsonPropertyName("matricula")]
            public string Matricula { get; set; }
        }
    }
}
