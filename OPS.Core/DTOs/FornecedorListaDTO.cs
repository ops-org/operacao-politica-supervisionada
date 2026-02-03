using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class FornecedorListaDTO
    {
        [JsonPropertyName("id_fornecedor")]
        public int IdFornecedor { get; set; }

        [JsonPropertyName("cnpj_cpf")]
        public string CnpjCpf { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; }

        [JsonPropertyName("valor_total_contratado")]
        public string ValorTotalContratado { get; set; }
    }
}
