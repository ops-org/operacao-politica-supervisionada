using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.Comum;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_mandato")]
    public class MandatoSenado
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Key]
        [Column("id_sf_senador")]
        public int IdSenador { get; set; }

        [Column("id_estado")]
        public byte? IdEstado { get; set; }

        // Partido no início do mandato
        [Column("id_partido")]
        public byte? IdPartido { get; set; }

        [Column("participacao")]
        [StringLength(50)]
        public string Participacao { get; set; } = null!;

        [Column("exerceu")]
        public bool Exerceu { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
        //public virtual Estado? Estado { get; set; }
        //public virtual Entities.Comum.Partido? Partido { get; set; }
        public virtual ICollection<MandatoExercicio> MandatoExercicios { get; set; } = new List<MandatoExercicio>();
        public virtual ICollection<MandatoLegislatura> MandatoLegislaturas { get; set; } = new List<MandatoLegislatura>();
    }
}
