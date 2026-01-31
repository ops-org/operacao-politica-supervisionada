using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.AssembleiasLegislativas
{
    [Table("cl_despesa_tipo")]
    public class DespesaTipoAssembleias
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("descricao")]
        [StringLength(150)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<DespesaAssembleias> Despesas { get; set; } = new List<DespesaAssembleias>();
        public virtual ICollection<DespesaEspecificacaoAssembleias> DespesaEspecificacoes { get; set; } = new List<DespesaEspecificacaoAssembleias>();
    }
}
