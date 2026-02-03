using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class ParlamentarNotaDTO
    {
        [JsonPropertyName("id_despesa")]
        public string IdDespesa { get; set; }

        [JsonPropertyName("id_fornecedor")]
        public string IdFornecedor { get; set; }

        [JsonPropertyName("cnpj_cpf")]
        public string CnpjCpf { get; set; }

        [JsonPropertyName("nome_fornecedor")]
        public string NomeFornecedor { get; set; }

        [JsonPropertyName("valor_liquido")]
        public string ValorLiquido { get; set; }
    }
}
