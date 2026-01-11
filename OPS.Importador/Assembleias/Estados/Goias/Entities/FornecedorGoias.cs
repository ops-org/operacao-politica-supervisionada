using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Goias.Entities
{
    public class FornecedorGoias
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("cnpj_cpf")]
        public string CnpjCpf { get; set; }

        [JsonPropertyName("data")]
        public DateTime Data { get; set; }

        [JsonPropertyName("numero")]
        public string Numero { get; set; }

        [JsonPropertyName("valor_apresentado")]
        public string ValorApresentado { get; set; }

        [JsonPropertyName("valor_indenizado")]
        public string ValorIndenizado { get; set; }
    }
}
