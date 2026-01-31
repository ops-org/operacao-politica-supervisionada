using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_deputado_cota_parlamentar")]
    public class DeputadoCotaParlamentar
    {
        [Key]
        [Column("id_cf_deputado")]
        public int IdDeputado { get; set; }

        [Key]
        [Column("ano")]
        public short Ano { get; set; }

        [Key]
        [Column("mes")]
        public short Mes { get; set; }

        [Column("valor", TypeName = "decimal(10,2)")]
        public decimal Valor { get; set; }

        [Column("percentual", TypeName = "decimal(10,2)")]
        public decimal? Percentual { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
