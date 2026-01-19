using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Parana
{
    public partial class ImportadorDespesasParana
    {
        private class DespesasMensais
        {
            [JsonPropertyName("mes")]
            public int Mes { get; set; }

            [JsonPropertyName("despesas")]
            public List<Despesas> Despesas { get; set; }

            [JsonPropertyName("verba")]
            public double Verba { get; set; }

            [JsonPropertyName("saldo")]
            public double Saldo { get; set; }

            [JsonPropertyName("total")]
            public double Total { get; set; }
        }
    }
}
