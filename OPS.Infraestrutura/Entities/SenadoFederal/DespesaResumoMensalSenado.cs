using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_despesa_resumo_mensal")]
    public class DespesaResumoMensalSenado
    {
        [Key]
        [Column("id_sf_senador")]
        public uint IdSenador { get; set; }

        [Key]
        [Column("ano")]
        public ushort Ano { get; set; }

        [Key]
        [Column("mes")]
        public ushort Mes { get; set; }

        [Column("valor_total", TypeName = "decimal(10,2)")]
        public decimal? ValorTotal { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
    }
}
