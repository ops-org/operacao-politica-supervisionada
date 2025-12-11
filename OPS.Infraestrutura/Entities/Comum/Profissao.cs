using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.SenadoFederal;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("profissao")]
    public class Profissao
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("descricao")]
        [StringLength(100)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<SenadorProfissao> SenadorProfissoes { get; set; } = new List<SenadorProfissao>();
    }
}
