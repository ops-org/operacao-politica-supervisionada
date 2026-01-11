using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace OPS.Importador.Fornecedores.MinhaReceita
{
    [Table("fornecedor_socio")]
    public class QuadroSocietario
    {
        //[Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_fornecedor")]
        public int IdFornecedor { get; set; }

        [Column("nome")]
        [JsonPropertyName("nome_socio")]
        public string Nome { get; set; }

        [Column("cnpj_cpf")]
        [JsonPropertyName("cnpj_cpf_do_socio")]
        public string CnpjCpf { get; set; }

        [Column("pais_origem")]
        //[JsonPropertyName("codigo_pais")]
        [JsonPropertyName("pais")]
        public string PaisOrigem { get; set; }

        [Column("data_entrada_sociedade")]
        [JsonPropertyName("data_entrada_sociedade")]
        public string DataEntradaSociedade { get; set; }

        [Column("id_fornecedor_faixa_etaria")]
        [JsonPropertyName("codigo_faixa_etaria")]
        //[JsonPropertyName("faixa_etaria")]
        public byte IdFaixaEtaria { get; set; }

        [Column("id_fornecedor_socio_qualificacao")]
        [JsonPropertyName("codigo_qualificacao_socio")]
        public int IdSocioQualificacao { get; set; }

        [Column("nome_representante")]
        [JsonPropertyName("nome_representante_legal")]
        public string NomeRepresentante { get; set; }

        [Column("cpf_representante")]
        [JsonPropertyName("cpf_representante_legal")]
        public string CpfRepresentante { get; set; }

        [Column("id_fornecedor_socio_representante_qualificacao")]
        [JsonPropertyName("codigo_qualificacao_representante_legal")]
        public int IdSocioRepresentanteQualificacao { get; set; }

        [NotMapped]
        [JsonPropertyName("codigo_pais")]
        public int? Pais { get; set; }

        [NotMapped]
        [JsonPropertyName("qualificacao_socio")]
        public string QualificacaoSocio { get; set; }

        [NotMapped]
        [JsonPropertyName("qualificacao_representante_legal")]
        public string QualificacaoRepresentante { get; set; }

    }
}
