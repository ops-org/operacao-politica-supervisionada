using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_mandato_legislatura")]
    [DebuggerDisplay("MandatoLegislatura {{Id}}")]
    public class MandatoLegislatura
    {
        [Key]
        [Column("id_sf_mandato")]
        public int IdMandato { get; set; }

        [Key]
        [Column("id_sf_legislatura")]
        public short IdLegislatura { get; set; }

        [Column("id_sf_senador")]
        public int IdSenador { get; set; }

        // Navigation properties
        public virtual MandatoSenado Mandato { get; set; } = null!;
        public virtual LegislaturaSenado Legislatura { get; set; } = null!;
    }
}

