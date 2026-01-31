using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Parana
{
    public partial class ImportadorDespesasParana
    {
        private class Parlamentar
        {
            [JsonPropertyName("codigo")]
            public int Codigo { get; set; }

            [JsonPropertyName("nome")]
            public string Nome { get; set; }

            [JsonPropertyName("nomePolitico")]
            public string NomePolitico { get; set; }

            [JsonPropertyName("partido")]
            public string Partido { get; set; }

            [JsonPropertyName("numeroGabinete")]
            public object NumeroGabinete { get; set; }

            [JsonPropertyName("foto")]
            public string Foto { get; set; }
        }
    }
}
