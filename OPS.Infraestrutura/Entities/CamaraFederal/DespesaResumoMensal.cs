using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_despesa_resumo_mensal")]
    [DebuggerDisplay("ResumoMensal {IdDeputado} - {Ano}/{Mes} - R$ {ValorTotal}")]
    public class DespesaResumoMensal
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

        [Column("valor_total", TypeName = "decimal(10,2)")]
        public decimal? ValorTotal { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
