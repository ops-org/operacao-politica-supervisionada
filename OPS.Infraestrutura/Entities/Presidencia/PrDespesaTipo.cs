using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Presidencia
{
    [Table("pr_despesa_tipo")]
    public class PrDespesaTipo
    {
        [Key]
        [Column("id")]
        public byte Id { get; set; }

        [Column("descricao")]
        [StringLength(100)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<PrDespesa> Despesas { get; set; } = new List<PrDespesa>();
    }
}
