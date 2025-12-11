using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.AssembleiasLegislativas
{
    [Table("cl_despesa_resumo_mensal")]
    public class DespesaResumoMensal
    {
        [Key]
        [Column("id_cl_deputado")]
        public uint IdDeputado { get; set; }

        [Key]
        [Column("ano")]
        public ushort Ano { get; set; }

        [Key]
        [Column("mes")]
        public ushort Mes { get; set; }

        [Column("valor_total", TypeName = "decimal(10,2)")]
        public decimal? ValorTotal { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
