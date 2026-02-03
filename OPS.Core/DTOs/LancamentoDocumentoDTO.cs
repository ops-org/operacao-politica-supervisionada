using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class LancamentoDocumentoDTO
    {
        [JsonPropertyName("id_despesa")]
        public object IdDespesa { get; set; }

        [JsonPropertyName("data_emissao")]
        public string DataEmissao { get; set; }

        [JsonPropertyName("id_fornecedor")]
        public int IdFornecedor { get; set; }

        [JsonPropertyName("cnpj_cpf")]
        public string CnpjCpf { get; set; }

        [JsonPropertyName("nome_fornecedor")]
        public string NomeFornecedor { get; set; }

        [JsonPropertyName("id_parlamentar")]
        public int IdParlamentar { get; set; }

        [JsonPropertyName("nome_parlamentar")]
        public string NomeParlamentar { get; set; }

        [JsonPropertyName("sigla_estado")]
        public string SiglaEstado { get; set; }

        [JsonPropertyName("sigla_partido")]
        public string SiglaPartido { get; set; }

        [JsonPropertyName("despesa_tipo")]
        public string DespesaTipo { get; set; }

        [JsonPropertyName("despesa_especificacao")]
        public string DespesaEspecificacao { get; set; }

        [JsonPropertyName("favorecido")]
        public string Favorecido { get; set; }

        [JsonPropertyName("valor_total")]
        public string ValorTotal { get; set; }
    }
}
