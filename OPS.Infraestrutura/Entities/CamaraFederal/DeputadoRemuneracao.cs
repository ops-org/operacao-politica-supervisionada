using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_deputado_remuneracao")]
    public class DeputadoRemuneracao
    {
        [Key, Required]
        [Column("id_cf_deputado")]
        public int IdDeputado { get; set; }

        [Key, Required]
        [Column("ano")]
        public short Ano { get; set; }

        [Key, Required]
        [Column("mes")]
        public short Mes { get; set; }

        [Column("valor")]
        public decimal Valor { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
