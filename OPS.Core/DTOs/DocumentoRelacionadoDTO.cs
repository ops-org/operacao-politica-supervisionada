using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class DocumentoRelacionadoDTO
    {
        [JsonPropertyName("id_despesa")]
        public int IdDespesa { get; set; }

        [JsonPropertyName("id_fornecedor")]
        public int IdFornecedor { get; set; }

        [JsonPropertyName("cnpj_cpf")]
        public string CnpjCpf { get; set; }

        [JsonPropertyName("nome_fornecedor")]
        public string NomeFornecedor { get; set; }

        [JsonPropertyName("sigla_estado_fornecedor")]
        public string SiglaEstadoFornecedor { get; set; }

        [JsonPropertyName("valor_liquido")]
        public string ValorLiquido { get; set; }
    }
}
