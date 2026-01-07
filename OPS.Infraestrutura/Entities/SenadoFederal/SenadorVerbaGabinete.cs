using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("cf_senador_verba_gabinete")]
    public class SenadorVerbaGabinete
    {
        [Key]
        [Column("id_cf_senador")]
        public uint IdSenador { get; set; }

        [Column("referencia")]
        public uint Referencia { get; set; }

        [Column("valor", TypeName = "decimal(10,2)")]
        public decimal? Valor { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
    }
}
