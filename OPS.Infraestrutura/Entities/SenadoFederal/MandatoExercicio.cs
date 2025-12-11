using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_mandato_exercicio")]
    public class MandatoExercicio
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_sf_senador")]
        public uint IdSenador { get; set; }

        [Column("id_sf_mandato")]
        public uint IdMandato { get; set; }

        [Column("id_sf_motivo_afastamento")]
        [StringLength(5)]
        public string? IdMotivoAfastamento { get; set; }

        [Column("inicio")]
        public DateTime? Inicio { get; set; }

        [Column("final")]
        public DateTime? Final { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
        public virtual Mandato Mandato { get; set; } = null!;
        public virtual MotivoAfastamento? MotivoAfastamento { get; set; }
    }
}
