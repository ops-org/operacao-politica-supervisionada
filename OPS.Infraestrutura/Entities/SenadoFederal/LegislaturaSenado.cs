using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_legislatura")]
    [DebuggerDisplay("LegislaturaSenado {{Id}}")]
    public class LegislaturaSenado
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("inicio")]
        public DateTime Inicio { get; set; }

        [Column("final")]
        public DateTime Final { get; set; }

        // Navigation properties
        public virtual ICollection<MandatoLegislatura> MandatoLegislaturas { get; set; } = new List<MandatoLegislatura>();
    }
}

