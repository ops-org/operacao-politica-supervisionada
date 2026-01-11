using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Parana
{
    public partial class ImportadorDespesasParana
    {
        private class ParlamentarDespesas
        {
            [JsonPropertyName("parlamentar")]
            public Parlamentar Parlamentar { get; set; }

            [JsonPropertyName("despesasAnuais")]
            public List<DespesasAnuais> DespesasAnuais { get; set; }

            //[JsonPropertyName("tipoDespesa")]
            //public TipoDespesa TipoDespesa { get; set; }

            //[JsonPropertyName("itensDespesa")]
            //public List<ItensDespesa> ItensDespesa { get; set; }
        }
    }
}
