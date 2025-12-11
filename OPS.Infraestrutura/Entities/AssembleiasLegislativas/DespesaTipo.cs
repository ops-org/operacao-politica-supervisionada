using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.AssembleiasLegislativas
{
    [Table("cl_despesa_tipo")]
    public class DespesaTipo
    {
        [Key]
        [Column("id")]
        public byte Id { get; set; }

        [Column("descricao")]
        [StringLength(100)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();
        public virtual ICollection<DespesaEspecificacao> DespesaEspecificacoes { get; set; } = new List<DespesaEspecificacao>();
    }
}
