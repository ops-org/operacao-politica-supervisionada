using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_secretario")]
    public class SecretarioSenado
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_sf_senador")]
        public uint IdSenador { get; set; }

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

        [Column("ano_mes")]
        public int? AnoMes { get; set; }

        [Column("admissao")]
        public int? Admissao { get; set; }

        [Column("situacao")]
        public int? Situacao { get; set; }

        [Column("id_funcao")]
        public int? IdFuncao { get; set; }

        [Column("id_categoria")]
        public int? IdCategoria { get; set; }

        [Column("id_referencia_cargo")]
        public int? IdReferenciaCargo { get; set; }

        [Column("id_especialidade")]
        public int? IdEspecialidade { get; set; }

        [Column("id_lotacao")]
        public int? IdLotacao { get; set; }

        [Column("id_tipo_folha")]
        public int? IdTipoFolha { get; set; }

        [Column("id_vinculo")]
        public int? IdVinculo { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
    }
}
