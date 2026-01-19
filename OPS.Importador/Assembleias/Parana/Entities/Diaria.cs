using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Parana
{
    public partial class ImportadorDespesasParana
    {
        private class Diaria
        {
            [JsonPropertyName("codigo")]
            public int Codigo { get; set; }

            [JsonPropertyName("descricao")]
            public string Descricao { get; set; }

            [JsonPropertyName("numeroDiarias")]
            public double NumeroDiarias { get; set; }

            [JsonPropertyName("regiao")]
            public string Regiao { get; set; }
        }
    }
}
