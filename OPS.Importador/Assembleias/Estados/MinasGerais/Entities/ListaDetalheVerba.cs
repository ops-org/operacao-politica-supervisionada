using System;
using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.MinasGerais.Entities
{
    public class ListaDetalheVerba
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("idDeputado")]
        public int IdDeputado { get; set; }

        [JsonPropertyName("dataReferencia")]
        public DateTime DataReferencia { get; set; }

        [JsonPropertyName("valorReembolsado")]
        public double ValorReembolsado { get; set; }

        [JsonPropertyName("dataEmissao")]
        public DateTime DataEmissao { get; set; }

        [JsonPropertyName("cpfCnpj")]
        public string CpfCnpj { get; set; }

        [JsonPropertyName("valorDespesa")]
        public double ValorDespesa { get; set; }

        [JsonPropertyName("nomeEmitente")]
        public string NomeEmitente { get; set; }

        [JsonPropertyName("descDocumento")]
        public string DescDocumento { get; set; }

        [JsonPropertyName("codTipoDespesa")]
        public int CodTipoDespesa { get; set; }

        [JsonPropertyName("descTipoDespesa")]
        public string DescTipoDespesa { get; set; }
    }
}
