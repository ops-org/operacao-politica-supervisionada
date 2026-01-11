using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_senador_partido")]
    public class SenadorPartido
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("id_sf_senador")]
        public int IdSenador { get; set; }

        [Column("id_partido")]
        public byte IdPartido { get; set; }

        [Column("filiacao")]
        public DateTime? Filiacao { get; set; }

        [Column("desfiliacao")]
        public DateTime? Desfiliacao { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
        //public virtual Entities.Comum.Partido Partido { get; set; } = null!;
    }
}
