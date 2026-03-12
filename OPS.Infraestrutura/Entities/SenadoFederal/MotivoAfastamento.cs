using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_motivo_afastamento")]
    [DebuggerDisplay("MotivoAfastamento {{Id}} - {{Descricao}}")]
    public class MotivoAfastamento
    {
        [Key]
        [Column("id")]
        [StringLength(5)]
        public string Id { get; set; } = null!;

        [Column("descricao")]
        [StringLength(100)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<MandatoExercicio> MandatoExercicios { get; set; } = new List<MandatoExercicio>();
    }
}

