using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_deputado_missao_oficial")]
    public class DeputadoMissaoOficial
    {
        [Key]
        [Column("id_cf_deputado")]
        public int IdDeputado { get; set; }

        [Column("periodo")]
        [StringLength(50)]
        public string? Periodo { get; set; }

        [Column("assunto")]
        [StringLength(4000)]
        public string? Assunto { get; set; }

        [Column("destino")]
        [StringLength(255)]
        public string? Destino { get; set; }

        [Column("passagens")]
        public decimal? Passagens { get; set; }

        [Column("diarias")]
        public decimal? Diarias { get; set; }

        [Column("relatorio")]
        [StringLength(255)]
        public string? Relatorio { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
        //public virtual TrechoViagem? TrechoViagem { get; set; }
    }
}
