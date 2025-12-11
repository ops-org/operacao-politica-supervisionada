using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_despesa_56")]
    public class Despesa56
    {
        [Key]
        [Column("id_cf_deputado")]
        public uint IdDeputado { get; set; }

        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("data_emissao")]
        public DateTime? DataEmissao { get; set; }

        [Column("valor", TypeName = "decimal(10,2)")]
        public decimal? Valor { get; set; }

        [Column("descricao")]
        [StringLength(255)]
        public string? Descricao { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
