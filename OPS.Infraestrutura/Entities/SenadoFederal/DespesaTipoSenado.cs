using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_despesa_tipo")]
    public class DespesaTipoSenado
    {
        [Key]
        [Column("id")]
        public byte Id { get; set; }

        [Column("descricao")]
        [StringLength(255)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<DespesaSenado> Despesas { get; set; } = new List<DespesaSenado>();
    }
}
