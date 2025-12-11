using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_secretario_historico")]
    public class SecretarioHistorico
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_cf_deputado")]
        public uint IdDeputado { get; set; }

        [Column("nome")]
        [StringLength(100)]
        public string? Nome { get; set; }

        [Column("periodo")]
        [StringLength(100)]
        public string? Periodo { get; set; }

        [Column("cargo")]
        [StringLength(45)]
        public string? Cargo { get; set; }

        [Column("valor_bruto", TypeName = "decimal(10,2)")]
        public decimal? ValorBruto { get; set; }

        [Column("valor_liquido", TypeName = "decimal(10,2)")]
        public decimal? ValorLiquido { get; set; }

        [Column("valor_outros", TypeName = "decimal(10,2)")]
        public decimal? ValorOutros { get; set; }

        [Column("link")]
        [StringLength(255)]
        public string? Link { get; set; }

        [Column("referencia")]
        [StringLength(255)]
        public string? Referencia { get; set; }

        [Column("em_exercicio")]
        public bool? EmExercicio { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
