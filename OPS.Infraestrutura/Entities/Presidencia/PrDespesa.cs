using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Presidencia
{
    [Table("pr_despesa")]
    public class PrDespesa
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_pr_despesa_tipo")]
        public byte IdDespesaTipo { get; set; }

        [Column("descricao")]
        [StringLength(255)]
        public string? Descricao { get; set; }

        [Column("valor", TypeName = "decimal(10,2)")]
        public decimal? Valor { get; set; }

        // Navigation properties
        public virtual PrDespesaTipo DespesaTipo { get; set; } = null!;
    }
}
