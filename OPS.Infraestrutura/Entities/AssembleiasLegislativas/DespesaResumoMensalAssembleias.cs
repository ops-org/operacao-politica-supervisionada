using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.AssembleiasLegislativas
{
    [Table("cl_despesa_resumo_mensal")]
    [DebuggerDisplay("DespesaResumoMensalAssembleias {{Id}} - R${{Valor}}")]
    public class DespesaResumoMensalAssembleias
    {
        [Key]
        [Column("ano")]
        public short Ano { get; set; }

        [Key]
        [Column("mes")]
        public short Mes { get; set; }

        [Column("valor", TypeName = "decimal(10,2)")]
        public decimal? Valor { get; set; }

    }
}

