using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Parana
{
    public partial class ImportadorDespesasParana
    {
        private class DespesasPR
        {
            [JsonPropertyName("sucesso")]
            public bool Sucesso { get; set; }

            [JsonPropertyName("erro")]
            public object Erro { get; set; }

            [JsonPropertyName("valor")]
            public object Valor { get; set; }

            [JsonPropertyName("despesas")]
            public List<ParlamentarDespesas> Despesas { get; set; }
        }
    }
}
