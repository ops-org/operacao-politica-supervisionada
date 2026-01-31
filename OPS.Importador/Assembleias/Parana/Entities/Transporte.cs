using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Parana
{
    public partial class ImportadorDespesasParana
    {
        private class Transporte
        {
            [JsonPropertyName("codigo")]
            public int Codigo { get; set; }

            [JsonPropertyName("descricao")]
            public string Descricao { get; set; }

            [JsonPropertyName("veiculo")]
            public Veiculo Veiculo { get; set; }

            [JsonPropertyName("distancia")]
            public double Distancia { get; set; }

            [JsonPropertyName("dataSaida")]
            public DateTime DataSaida { get; set; }

            [JsonPropertyName("dataChegada")]
            public DateTime DataChegada { get; set; }
        }
    }
}
