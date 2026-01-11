using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Parana
{
    public partial class ImportadorDespesasParana
    {
        private class DespesasAnuais
        {
            [JsonPropertyName("exercicio")]
            public int Exercicio { get; set; }

            [JsonPropertyName("despesasMensais")]
            public List<DespesasMensais> DespesasMensais { get; set; }

            [JsonPropertyName("verba")]
            public object Verba { get; set; }
        }
    }
}
