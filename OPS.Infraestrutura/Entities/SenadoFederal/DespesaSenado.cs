using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.Fornecedores;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_despesa")]
    public class DespesaSenado
    {
        [Key]
        [Column("id_sf_senador")]
        public int IdSenador { get; set; }

        [Key]
        [Column("id")]
        public ulong Id { get; set; }

        [Column("id_sf_despesa_tipo")]
        public byte? IdDespesaTipo { get; set; }

        [Column("id_fornecedor")]
        public int? IdFornecedor { get; set; }

        [Column("ano_mes", TypeName = "decimal(6,0)")]
        public int? AnoMes { get; set; }

        [Column("ano")]
        public short? Ano { get; set; }

        [Column("mes")]
        public short? Mes { get; set; }

        [Column("documento")]
        [StringLength(50)]
        public string? Documento { get; set; }

        [Column("data_emissao")]
        public DateTime? DataEmissao { get; set; }

        [Column("detalhamento")]
        public string? Detalhamento { get; set; }

        [Column("valor", TypeName = "decimal(10,2)")]
        public decimal? Valor { get; set; }

        [Column("hash")]
        public byte[]? Hash { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
        public virtual DespesaTipoSenado? DespesaTipo { get; set; }
        public virtual Fornecedor? Fornecedor { get; set; }
    }
}
