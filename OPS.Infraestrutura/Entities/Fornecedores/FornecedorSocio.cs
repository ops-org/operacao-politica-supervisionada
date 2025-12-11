using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("fornecedor_socio")]
    public class FornecedorSocio
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_fornecedor")]
        public uint IdFornecedor { get; set; }

        [Column("nome")]
        [StringLength(255)]
        public string? Nome { get; set; }

        [Column("cnpj_cpf")]
        [StringLength(15)]
        public string? CnpjCpf { get; set; }

        [Column("pais_origem")]
        [StringLength(255)]
        public string? PaisOrigem { get; set; }

        [Column("data_entrada_sociedade")]
        public DateTime? DataEntradaSociedade { get; set; }

        [Column("id_fornecedor_faixa_etaria")]
        public byte? IdFornecedorFaixaEtaria { get; set; }

        [Column("id_fornecedor_socio_qualificacao")]
        public uint? IdFornecedorSocioQualificacao { get; set; }

        [Column("nome_representante")]
        [StringLength(255)]
        public string? NomeRepresentante { get; set; }

        [Column("cpf_representante")]
        [StringLength(15)]
        public string? CpfRepresentante { get; set; }

        [Column("id_fornecedor_socio_representante_qualificacao")]
        public uint? IdFornecedorSocioRepresentanteQualificacao { get; set; }

        // Navigation properties
        public virtual Fornecedor Fornecedor { get; set; } = null!;
    }
}
