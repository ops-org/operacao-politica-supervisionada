using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_deputado_verba_gabinete")]
    public class DeputadoVerbaGabinete
    {
        [Key]
        [Column("id_sf_senador")]
        public int IdSenador { get; set; }

        [Column("ano")]
        public short? Ano { get; set; }

        [Column("mes")]
        public short? Mes { get; set; }

        [Column("valor", TypeName = "decimal(10,2)")]
        public decimal? Valor { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
    }
}
