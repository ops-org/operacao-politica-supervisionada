using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_deputado_imovel_funcional")]
    [DebuggerDisplay("DeputadoImovelFuncional {{Id}}")]
    public class DeputadoImovelFuncional
    {
        [Key]
        [Column("id_cf_deputado")]
        public int IdDeputado { get; set; }

        [Column("uso_de")]
        public DateOnly UsoDe { get; set; }

        [Column("uso_ate")]
        public DateOnly? UsoAte { get; set; }

        [Column("total_dias")]
        public int? TotalDias { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
    }
}

