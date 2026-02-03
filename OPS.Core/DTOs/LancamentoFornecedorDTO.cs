using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class LancamentoFornecedorDTO
    {
        [JsonPropertyName("id_fornecedor")]
        public int IdFornecedor { get; set; }

        [JsonPropertyName("cnpj_cpf")]
        public string CnpjCpf { get; set; }

        [JsonPropertyName("nome_fornecedor")]
        public string NomeFornecedor { get; set; }

        [JsonPropertyName("total_notas")]
        public string TotalNotas { get; set; }

        [JsonPropertyName("valor_total")]
        public string ValorTotal { get; set; }
    }
}
