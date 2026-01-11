using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("fornecedor_info")]
    public class FornecedorInfo
    {
        [Key]
        [Column("id_fornecedor")]
        public int IdFornecedor { get; set; }

        [Column("cnpj")]
        [StringLength(14)]
        public string Cnpj { get; set; } = null!;

        [Column("cnpj_radical")]
        [StringLength(14)]
        public string? CnpjRadical { get; set; }

        [Column("tipo")]
        [StringLength(20)]
        public string? Tipo { get; set; }

        [Column("nome")]
        [StringLength(255)]
        public string? Nome { get; set; }

        [Column("data_de_abertura")]
        public DateTime? DataDeAbertura { get; set; }

        [Column("nome_fantasia")]
        [StringLength(255)]
        public string? NomeFantasia { get; set; }

        [Column("id_fornecedor_atividade_principal")]
        // [ForeignKey("FornecedorAtividadePrincipal")]
        public int? IdFornecedorAtividadePrincipal { get; set; }

        [Column("id_fornecedor_natureza_juridica")]
        // [ForeignKey("FornecedorNaturezaJuridica")]
        public int? IdFornecedorNaturezaJuridica { get; set; }

        [Column("logradouro_tipo")]
        [StringLength(20)]
        public string? LogradouroTipo { get; set; }

        [Column("logradouro")]
        [StringLength(100)]
        public string? Logradouro { get; set; }

        [Column("numero")]
        [StringLength(100)]
        public string? Numero { get; set; }

        [Column("complemento")]
        [StringLength(150)]
        public string? Complemento { get; set; }

        [Column("cep")]
        [StringLength(20)]
        public string? Cep { get; set; }

        [Column("bairro")]
        [StringLength(100)]
        public string? Bairro { get; set; }

        [Column("municipio")]
        [StringLength(100)]
        public string? Municipio { get; set; }

        [Column("estado")]
        [StringLength(4)]
        public string? Estado { get; set; }

        [Column("endereco_eletronico")]
        [StringLength(100)]
        public string? EnderecoEletronico { get; set; }

        [Column("telefone1")]
        [StringLength(100)]
        public string? Telefone1 { get; set; }

        [Column("telefone2")]
        [StringLength(100)]
        public string? Telefone2 { get; set; }

        [Column("fax")]
        [StringLength(100)]
        public string? Fax { get; set; }

        [Column("ente_federativo_responsavel")]
        [StringLength(100)]
        public string? EnteFederativoResponsavel { get; set; }

        [Column("situacao_cadastral")]
        [StringLength(100)]
        public string? SituacaoCadastral { get; set; }

        [Column("data_da_situacao_cadastral")]
        public DateTime? DataDaSituacaoCadastral { get; set; }

        [Column("motivo_situacao_cadastral")]
        [StringLength(100)]
        public string? MotivoSituacaoCadastral { get; set; }

        [Column("situacao_especial")]
        [StringLength(100)]
        public string? SituacaoEspecial { get; set; }

        [Column("data_situacao_especial")]
        public DateTime? DataSituacaoEspecial { get; set; }

        [Column("capital_social", TypeName = "decimal(65,2)")]
        public decimal? CapitalSocial { get; set; }

        [Column("porte")]
        [StringLength(50)]
        public string? Porte { get; set; }

        [Column("opcao_pelo_mei")]
        public bool? OpcaoPeloMei { get; set; }

        [Column("data_opcao_pelo_mei")]
        public DateTime? DataOpcaoPeloMei { get; set; }

        [Column("data_exclusao_do_mei")]
        public DateTime? DataExclusaoDoMei { get; set; }

        [Column("opcao_pelo_simples")]
        public bool? OpcaoPeloSimples { get; set; }

        [Column("data_opcao_pelo_simples")]
        public DateTime? DataOpcaoPeloSimples { get; set; }

        [Column("data_exclusao_do_simples")]
        public DateTime? DataExclusaoDoSimples { get; set; }

        [Column("codigo_municipio")]
        [StringLength(50)]
        public string? CodigoMunicipio { get; set; }

        [Column("codigo_municipio_ibge")]
        [StringLength(50)]
        public string? CodigoMunicipioIbge { get; set; }

        [Column("nome_cidade_no_exterior")]
        [StringLength(100)]
        public string? NomeCidadeNoExterior { get; set; }

        [Column("obtido_em")]
        public DateTime? ObtidoEm { get; set; }

        [Column("ip_colaborador")]
        [StringLength(15)]
        public string? IpColaborador { get; set; }

        [Column("pais")]
        [StringLength(15)]
        public string? Pais { get; set; }

        [Column("nome_pais")]
        [StringLength(100)]
        public string? NomePais { get; set; }

        // Navigation properties
        public virtual Fornecedor Fornecedor { get; set; } = null!;
        // public virtual FornecedorNaturezaJuridica? FornecedorNaturezaJuridica { get; set; }
        // public virtual FornecedorAtividade? FornecedorAtividadePrincipal { get; set; }
    }
}
