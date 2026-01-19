using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Parana
{
    public partial class ImportadorDespesasParana
    {
        private class ItensDespesa
        {
            [JsonPropertyName("codigo")]
            public int Codigo { get; set; }

            [JsonPropertyName("exercicio")]
            public int? Exercicio { get; set; }

            [JsonPropertyName("mes")]
            public int Mes { get; set; }

            [JsonPropertyName("tipoDespesa")]
            public TipoDespesa TipoDespesa { get; set; }

            [JsonPropertyName("valor")]
            public double Valor { get; set; }

            [JsonPropertyName("situacao")]
            public string Situacao { get; set; }

            [JsonPropertyName("data")]
            public DateTime Data { get; set; }

            [JsonPropertyName("descricao")]
            public string Descricao { get; set; }

            [JsonPropertyName("fornecedor")]
            public FornecedorPR Fornecedor { get; set; }

            [JsonPropertyName("transporte")]
            public Transporte Transporte { get; set; }

            [JsonPropertyName("diaria")]
            public Diaria Diaria { get; set; }

            [JsonPropertyName("numero")]
            public int Numero { get; set; }

            [JsonPropertyName("valorDevolucao")]
            public double ValorDevolucao { get; set; }

            [JsonPropertyName("anexos")]
            public object Anexos { get; set; }

            [JsonPropertyName("numeroDocumento")]
            public string NumeroDocumento { get; set; }
        }
    }
}
