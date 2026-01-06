using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_senador_missao_oficial")]
    public class DeputadoMissaoOficial
    {
        [Key]
        [Column("id_sf_senador")]
        public uint IdSenador { get; set; }

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
        [StringLength(10)]
        public decimal? Passagens { get; set; }

        [Column("diarias")]
        [StringLength(10)]
        public decimal? Diarias { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
    }
}
