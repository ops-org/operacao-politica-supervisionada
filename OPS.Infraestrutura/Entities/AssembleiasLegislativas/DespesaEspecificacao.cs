using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.AssembleiasLegislativas
{
    [Table("cl_despesa_especificacao")]
    public class DespesaEspecificacao
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_cl_despesa_tipo")]
        public byte IdDespesaTipo { get; set; }

        [Column("descricao")]
        [StringLength(400)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual DespesaTipo DespesaTipo { get; set; } = null!;
        public virtual ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();
    }
}
