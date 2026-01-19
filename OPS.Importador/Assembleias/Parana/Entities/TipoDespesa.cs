using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Parana
{
    public partial class ImportadorDespesasParana
    {
        private class TipoDespesa
        {
            [JsonPropertyName("codigo")]
            public int Codigo { get; set; }

            [JsonPropertyName("descricao")]
            public string Descricao { get; set; }

            [JsonPropertyName("categoria")]
            public string Categoria { get; set; }
        }
    }
}
