using System.Text.Json.Serialization;

namespace OPS.Importador.Fornecedores.ReceitaWS
{
    #region ReceitaWS DTO

    public class FornecedorReceitaWs
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("ultima_atualizacao")]
        public DateTime UltimaAtualizacao { get; set; }

        [JsonPropertyName("cnpj")]
        public string Cnpj { get; set; }

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; }

        [JsonPropertyName("porte")]
        public string Porte { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("fantasia")]
        public string Fantasia { get; set; }

        [JsonPropertyName("abertura")]
        public string Abertura { get; set; }

        [JsonPropertyName("atividade_principal")]
        public List<AtividadePrincipal> AtividadePrincipal { get; set; }

        [JsonPropertyName("atividades_secundarias")]
        public List<AtividadesSecundaria> AtividadesSecundarias { get; set; }

        [JsonPropertyName("natureza_juridica")]
        public string NaturezaJuridica { get; set; }

        [JsonPropertyName("logradouro")]
        public string Logradouro { get; set; }

        [JsonPropertyName("numero")]
        public string Numero { get; set; }

        [JsonPropertyName("complemento")]
        public string Complemento { get; set; }

        [JsonPropertyName("cep")]
        public string Cep { get; set; }

        [JsonPropertyName("bairro")]
        public string Bairro { get; set; }

        [JsonPropertyName("municipio")]
        public string Municipio { get; set; }

        [JsonPropertyName("uf")]
        public string Uf { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("telefone")]
        public string Telefone { get; set; }

        [JsonPropertyName("efr")]
        public string Efr { get; set; }

        [JsonPropertyName("situacao")]
        public string Situacao { get; set; }

        [JsonPropertyName("data_situacao")]
        public string DataSituacao { get; set; }

        [JsonPropertyName("motivo_situacao")]
        public string MotivoSituacao { get; set; }

        [JsonPropertyName("situacao_especial")]
        public string SituacaoEspecial { get; set; }

        [JsonPropertyName("data_situacao_especial")]
        public string DataSituacaoEspecial { get; set; }

        [JsonPropertyName("capital_social")]
        public decimal CapitalSocial { get; set; }

        [JsonPropertyName("qsa")]
        public List<Qsa> Qsa { get; set; }

        [JsonPropertyName("simples")]
        public Simples Simples { get; set; }

        [JsonPropertyName("simei")]
        public Simei Simei { get; set; }

        [JsonPropertyName("billing")]
        public Billing Billing { get; set; }
    }


    #endregion ReceitaWS DTO
}