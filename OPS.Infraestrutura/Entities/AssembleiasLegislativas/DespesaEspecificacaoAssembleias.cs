using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.AssembleiasLegislativas
{
    [Table("cl_despesa_especificacao")]
    public class DespesaEspecificacaoAssembleias
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("id_cl_despesa_tipo")]
        public short? IdDespesaTipo { get; set; }

        [Column("descricao")]
        [StringLength(400)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual DespesaTipoAssembleias DespesaTipo { get; set; } = null!;
        public virtual ICollection<DespesaAssembleias> Despesas { get; set; } = new List<DespesaAssembleias>();
    }
}
