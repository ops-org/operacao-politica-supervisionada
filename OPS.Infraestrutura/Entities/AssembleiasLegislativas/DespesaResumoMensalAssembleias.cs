using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.AssembleiasLegislativas
{
    [Table("cl_despesa_resumo_mensal")]
    public class DespesaResumoMensalAssembleias
    {
        [Key]
        [Column("ano")]
        public long Ano { get; set; }

        [Key]
        [Column("mes")]
        public long Mes { get; set; }

        [Column("valor", TypeName = "decimal(10,2)")]
        public decimal? Valor { get; set; }

    }
}
