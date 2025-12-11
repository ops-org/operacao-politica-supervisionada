using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.AssembleiasLegislativas
{
    [Table("cl_deputado_campeao_gasto")]
    public class DeputadoCampeaoGasto
    {
        [Key]
        [Column("id_cl_deputado")]
        public uint IdDeputado { get; set; }

        [Column("nome_parlamentar")]
        [StringLength(100)]
        public string? NomeParlamentar { get; set; }

        [Column("valor_total", TypeName = "decimal(10,2)")]
        public decimal? ValorTotal { get; set; }

        [Column("sigla_partido")]
        [StringLength(20)]
        public string? SiglaPartido { get; set; }

        [Column("sigla_estado")]
        [StringLength(2)]
        public string? SiglaEstado { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
