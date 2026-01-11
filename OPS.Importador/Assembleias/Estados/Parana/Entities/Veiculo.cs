using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Parana
{
    public partial class ImportadorDespesasParana
    {
        private class Veiculo
        {
            [JsonPropertyName("codigo")]
            public int Codigo { get; set; }

            [JsonPropertyName("numero")]
            public int Numero { get; set; }

            [JsonPropertyName("placa")]
            public string Placa { get; set; }

            [JsonPropertyName("modelo")]
            public string Modelo { get; set; }
        }
    }
}
