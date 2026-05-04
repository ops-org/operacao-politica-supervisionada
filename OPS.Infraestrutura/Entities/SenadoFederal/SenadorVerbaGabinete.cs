using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("cf_senador_verba_gabinete")]
    [DebuggerDisplay("SenadorVerbaGabinete {{Id}} - {{Nome}}")]
    public class SenadorVerbaGabinete
    {
        [Key]
        [Column("id_cf_senador")]
        public int IdSenador { get; set; }

        [Column("referencia")]
        public int Referencia { get; set; }

        [Column("valor", TypeName = "decimal(10,2)")]
        public decimal? Valor { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
    }
}

