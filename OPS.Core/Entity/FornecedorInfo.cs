using Dapper;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Core.Entity
{
    [Table("fornecedor_info")]
    public class FornecedorInfo
    {

        [Key, Required]
        [Column("id_fornecedor")]
        public int Id { get; set; }

        [Column("cnpj")]
        [JsonPropertyName("cnpj")]
        public string Cnpj { get; set; }

        [Column("cnpj_radical")]
        public string RadicalCnpj { get; set; }

        [Column("tipo")]
        [JsonPropertyName("descricao_identificador_matriz_filial")]
        // [JsonPropertyName("identificador_matriz_filial")] int
        public string Tipo { get; set; }

        [Column("nome")]
        [JsonPropertyName("razao_social")]
        public string RazaoSocial { get; set; }

        [Column("data_de_abertura")]
        [JsonPropertyName("data_inicio_atividade")]
        public DateTime Abertura { get; set; }

        [Column("nome_fantasia")]
        [JsonPropertyName("nome_fantasia")]
        public string NomeFantasia { get; set; }

        [Column("id_fornecedor_atividade_principal")]
        [JsonPropertyName("cnae_fiscal")]
        public int IdAtividadePrincipal { get; set; }

        [Column("id_fornecedor_natureza_juridica")]
        [JsonPropertyName("codigo_natureza_juridica")]
        public int IdNaturezaJuridica { get; set; }

        [Column("logradouro_tipo")]
        [JsonPropertyName("descricao_tipo_de_logradouro")]
        public string TipoLogradouro { get; set; }

        [Column("logradouro")]
        [JsonPropertyName("logradouro")]
        public string Logradouro { get; set; }

        [Column("numero")]
        [JsonPropertyName("numero")]
        public string Numero { get; set; }

        [Column("complemento")]
        [JsonPropertyName("complemento")]
        public string Complemento { get; set; }

        [Column("cep")]
        [JsonPropertyName("cep")]
        public string Cep { get; set; }

        [Column("bairro")]
        [JsonPropertyName("bairro")]
        public string Bairro { get; set; }

        [Column("municipio")]
        [JsonPropertyName("municipio")]
        public string Municipio { get; set; }

        [Column("estado")]
        [JsonPropertyName("uf")]
        public string UF { get; set; }

        [Column("endereco_eletronico")]
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [Column("telefone1")]
        [JsonPropertyName("ddd_telefone_1")]
        public string Telefone1 { get; set; }

        [Column("telefone2")]
        [JsonPropertyName("ddd_telefone_2")]
        public string Telefone2 { get; set; }

        [Column("ente_federativo_responsavel")]
        [JsonPropertyName("ente_federativo_responsavel")]
        public string EnteFederativoResponsavel { get; set; }

        [Column("situacao_cadastral")]
        // [JsonPropertyName("situacao_cadastral")] int
        [JsonPropertyName("descricao_situacao_cadastral")]
        public string SituacaoCadastral { get; set; }

        [Column("data_da_situacao_cadastral")]
        [JsonPropertyName("data_situacao_cadastral")]
        public DateTime? DataSituacaoCadastral { get; set; }

        [Column("motivo_situacao_cadastral")]
        //[JsonPropertyName("motivo_situacao_cadastral")] int
        [JsonPropertyName("descricao_motivo_situacao_cadastral")]
        public string MotivoSituacaoCadastral { get; set; }

        [Column("situacao_especial")]
        [JsonPropertyName("situacao_especial")]
        public string SituacaoEspecial { get; set; }

        [Column("data_situacao_especial")]
        [JsonPropertyName("data_situacao_especial")]
        public DateTime? DataSituacaoEspecial { get; set; }

        [Column("capital_social")]
        [JsonPropertyName("capital_social")]
        public decimal CapitalSocial { get; set; }

        [Column("porte")]
        [JsonPropertyName("descricao_porte")]
        // [JsonPropertyName("codigo_porte")] int
        // [JsonPropertyName("porte")] int
        public string Porte { get; set; }

        [Column("opcao_pelo_mei")]
        [JsonPropertyName("opcao_pelo_mei")]
        public bool? OpcaoPeloMEI { get; set; }

        [Column("data_opcao_pelo_mei")]
        [JsonPropertyName("data_opcao_pelo_mei")]
        public DateTime? DataOpcaoPeloMEI { get; set; }

        [Column("data_exclusao_do_mei")]
        [JsonPropertyName("data_exclusao_do_mei")]
        public DateTime? DataExclusaoMEI { get; set; }

        [Column("opcao_pelo_simples")]
        [JsonPropertyName("opcao_pelo_simples")]
        public bool? OpcaoPeloSimples { get; set; }

        [Column("data_opcao_pelo_simples")]
        [JsonPropertyName("data_opcao_pelo_simples")]
        public DateTime? DataOpcaoPeloSimples { get; set; }

        [Column("data_exclusao_do_simples")]
        [JsonPropertyName("data_exclusao_do_simples")]
        public DateTime? DataExclusaoSimples { get; set; }

        [Column("codigo_municipio")]
        [JsonPropertyName("codigo_municipio")]
        public int? CodigoMunicipio { get; set; }

        [Column("codigo_municipio_ibge")]
        [JsonPropertyName("codigo_municipio_ibge")]
        public int? CodigoMunicipioIBGE { get; set; }

        [Column("nome_cidade_no_exterior")]
        [JsonPropertyName("nome_cidade_no_exterior")]
        public string NomeCidadeExterior { get; set; }

        [Column("obtido_em")]
        public DateTime ObtidoEm { get; set; }

        [Column("nome_pais")]
        [JsonPropertyName("pais")]
        public string Pais { get; set; }

        [Column("fax")]
        [JsonPropertyName("ddd_fax")]
        public string DddFax { get; set; }

        [Column("pais")]
        [JsonPropertyName("codigo_pais")]
        public int? CodigoPais { get; set; }

        [NotMapped]
        [JsonPropertyName("qsa")]
        public List<QuadroSocietario> Qsa { get; set; }

        [NotMapped]
        [JsonPropertyName("cnaes_secundarios")]
        public List<FornecedorAtividade> CnaesSecundarios { get; set; }

        [NotMapped]
        [JsonPropertyName("cnae_fiscal_descricao")]
        public string AtividadePrincipal { get; set; }

        [NotMapped]
        [JsonPropertyName("natureza_juridica")]
        public string NaturezaJuridica { get; set; }
    }
}
