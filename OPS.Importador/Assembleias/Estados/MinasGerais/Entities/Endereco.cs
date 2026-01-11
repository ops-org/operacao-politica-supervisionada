using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.MinasGerais.Entities
{
    public class Endereco
    {
        [JsonPropertyName("logradouro")]
        public string Logradouro { get; set; }

        [JsonPropertyName("numero")]
        public string Numero { get; set; }

        [JsonPropertyName("complemento")]
        public string Complemento { get; set; }

        [JsonPropertyName("bairro")]
        public string Bairro { get; set; }

        [JsonPropertyName("cep")]
        public string Cep { get; set; }

        [JsonPropertyName("municipio")]
        public Municipio Municipio { get; set; }

        [JsonPropertyName("descTipo")]
        public string DescTipo { get; set; }

        [JsonPropertyName("telefones")]
        public List<Telefone> Telefones { get; set; }
    }
}
