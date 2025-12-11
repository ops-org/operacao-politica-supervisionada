using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_deputado_remuneracao")]
    public class DeputadoRemuneracao
    {
        [Key]
        [Column("id_cf_deputado")]
        public uint IdDeputado { get; set; }

        [Column("referencia")]
        public uint Referencia { get; set; }

        [Column("remuneracao_fixa", TypeName = "decimal(10,2)")]
        public decimal? RemuneracaoFixa { get; set; }

        [Column("verba_parlamentar", TypeName = "decimal(10,2)")]
        public decimal? VerbaParlamentar { get; set; }

        [Column("valor_total", TypeName = "decimal(10,2)")]
        public decimal? ValorTotal { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
