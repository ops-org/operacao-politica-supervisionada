using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.Fornecedores;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_despesa")]
    public class Despesa
    {
        [Key]
        [Column("id")]
        public ulong Id { get; set; }

        [Column("ano")]
        public ushort Ano { get; set; }

        [Column("mes")]
        public byte Mes { get; set; }

        [Column("id_cf_legislatura")]
        public byte IdLegislatura { get; set; }

        [Column("id_cf_deputado")]
        public uint IdDeputado { get; set; }

        [Column("id_cf_mandato")]
        public ushort? IdMandato { get; set; }

        [Column("id_cf_despesa_tipo")]
        public ushort IdDespesaTipo { get; set; }

        [Column("id_cf_especificacao")]
        public byte? IdEspecificacao { get; set; }

        [Column("id_fornecedor")]
        public uint IdFornecedor { get; set; }

        [Column("id_documento")]
        public ulong? IdDocumento { get; set; }

        [Column("id_passageiro")]
        public uint? IdPassageiro { get; set; }

        [Column("id_trecho_viagem")]
        public ushort? IdTrechoViagem { get; set; }

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
        public virtual Legislatura Legislatura { get; set; } = null!;
        public virtual Mandato? Mandato { get; set; }
        public virtual DespesaTipo DespesaTipo { get; set; } = null!;
        public virtual EspecificacaoTipo? EspecificacaoTipo { get; set; }
        public virtual Fornecedor Fornecedor { get; set; } = null!;
        public virtual TrechoViagem? TrechoViagem { get; set; }
    }
}
