using System;
using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class QuadroSocietarioDTO
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("cnpj_cpf")]
        public string CnpjCpf { get; set; }

        [JsonPropertyName("pais_origem")]
        public string PaisOrigem { get; set; }

        [JsonPropertyName("data_entrada_sociedade")]
        public string DataEntradaSociedade { get; set; }

        [JsonPropertyName("faixa_etaria")]
        public string FaixaEtaria { get; set; }

        [JsonPropertyName("qualificacao")]
        public string Qualificacao { get; set; }

        [JsonPropertyName("nome_representante")]
        public string NomeRepresentante { get; set; }

        [JsonPropertyName("cpf_representante")]
        public string CpfRepresentante { get; set; }

        [JsonPropertyName("qualificacao_representante")]
        public string QualificacaoRepresentante { get; set; }
    }
}
