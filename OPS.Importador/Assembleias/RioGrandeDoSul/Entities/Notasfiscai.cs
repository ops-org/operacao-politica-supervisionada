using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.RioGrandeDoSul.Entities
{
    public class Notasfiscai
    {
        [JsonPropertyName("idRequisicaoDiaria")]
        public int IdRequisicaoDiaria { get; set; }

        [JsonPropertyName("numNota")]
        public string NumNota { get; set; }

        [JsonPropertyName("cnpjEstabelecimento")]
        public string CnpjEstabelecimento { get; set; }

        [JsonPropertyName("dataEmissao")]
        public string DataEmissao { get; set; }

        [JsonPropertyName("valorTotal")]
        public string ValorTotal { get; set; }

        [JsonPropertyName("municipio")]
        public string Municipio { get; set; }

        [JsonPropertyName("nomeEstabelecimento")]
        public string NomeEstabelecimento { get; set; }
    }
}
