using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_legislatura")]
    public class Legislatura
    {
        [Key]
        [Column("id")]
        public byte Id { get; set; }

        [Column("inicio")]
        public DateTime Inicio { get; set; }

        [Column("final")]
        public DateTime Final { get; set; }

        // Navigation properties
        public virtual ICollection<MandatoLegislatura> MandatoLegislaturas { get; set; } = new List<MandatoLegislatura>();
    }
}
