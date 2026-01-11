using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_deputado_auxilio_moradia")]
    public class DeputadoAuxilioMoradia
    {
        [Key]
        [Column("id_cf_deputado")]
        public int IdDeputado { get; set; }

        [Column("ano")]
        public short? Ano { get; set; }

        [Column("mes")]
        public short? Mes { get; set; }

        [Column("valor", TypeName = "decimal(10,2)")]
        public decimal? Valor { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
