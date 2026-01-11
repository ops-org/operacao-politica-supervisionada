using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.Fornecedores;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_despesa")]
    public class DespesaCamara
    {
        [Key]
        [Column("id")]
        public ulong Id { get; set; }

        [Column("ano")]
        public short Ano { get; set; }

        [Column("mes")]
        public byte Mes { get; set; }

        [Column("id_cf_legislatura")]
        public byte IdLegislatura { get; set; }

        [Column("id_cf_deputado")]
        public int IdDeputado { get; set; }

        [Column("id_cf_mandato")]
        public short? IdMandato { get; set; }

        [Column("id_cf_despesa_tipo")]
        public short IdDespesaTipo { get; set; }

        [Column("id_cf_especificacao")]
        public byte? IdEspecificacao { get; set; }

        [Column("id_fornecedor")]
        public int IdFornecedor { get; set; }

        [Column("id_documento")]
        public ulong? IdDocumento { get; set; }

        [Column("id_passageiro")]
        public int? IdPassageiro { get; set; }

        [Column("id_trecho_viagem")]
        public short? IdTrechoViagem { get; set; }

        [Column("data_emissao")]
        public DateTime? DataEmissao { get; set; }

        [Column("valor_documento", TypeName = "decimal(10,2)")]
        public decimal? ValorDocumento { get; set; }

        [Column("valor_glosa", TypeName = "decimal(10,2)")]
        public decimal ValorGlosa { get; set; }

        [Column("valor_liquido", TypeName = "decimal(10,2)")]
        public decimal ValorLiquido { get; set; }

        [Column("valor_restituicao", TypeName = "decimal(10,2)")]
        public decimal? ValorRestituicao { get; set; }

        [Column("tipo_documento")]
        public byte TipoDocumento { get; set; }

        [Column("tipo_link")]
        public byte TipoLink { get; set; }

        [Column("numero_documento")]
        [StringLength(100)]
        public string? NumeroDocumento { get; set; }

        [Column("hash")]
        public byte[]? Hash { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
        public virtual LegislaturaCamara Legislatura { get; set; } = null!;
        public virtual MandatoCamara? Mandato { get; set; }
        public virtual DespesaTipoCamara DespesaTipo { get; set; } = null!;
        public virtual EspecificacaoTipo? EspecificacaoTipo { get; set; }
        public virtual Fornecedor Fornecedor { get; set; } = null!;
        public virtual TrechoViagem? TrechoViagem { get; set; }
    }
}
