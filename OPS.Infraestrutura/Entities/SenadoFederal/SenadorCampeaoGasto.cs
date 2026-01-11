using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_senador_campeao_gasto")]
    public class SenadorCampeaoGasto
    {
        [Key]
        [Column("id_sf_senador")]
        public int IdSenador { get; set; }

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

        [Column("ano")]
        public short? Ano { get; set; }

        [Column("mes")]
        public short? Mes { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
    }
}
