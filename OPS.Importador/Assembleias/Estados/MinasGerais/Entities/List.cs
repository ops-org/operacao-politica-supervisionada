using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.MinasGerais.Entities
{
    public class List
    {
        [JsonPropertyName("idDeputado")]
        public int IdDeputado { get; set; }

        [JsonPropertyName("dataReferencia")]
        public DateTime DataReferencia { get; set; }

        [JsonPropertyName("codTipoDespesa")]
        public int CodTipoDespesa { get; set; }

        [JsonPropertyName("valor")]
        public double Valor { get; set; }

        [JsonPropertyName("descTipoDespesa")]
        public string DescTipoDespesa { get; set; }

        [JsonPropertyName("listaDetalheVerba")]
        public List<ListaDetalheVerba> ListaDetalheVerba { get; set; }
    }
}
