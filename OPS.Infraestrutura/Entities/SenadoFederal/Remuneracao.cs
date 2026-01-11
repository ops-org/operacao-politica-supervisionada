using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_remuneracao")]
    public class Remuneracao
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_vinculo")]
        public byte IdVinculo { get; set; }

        [Column("id_categoria")]
        public byte IdCategoria { get; set; }

        [Column("id_cargo")]
        public byte? IdCargo { get; set; }

        [Column("id_referencia_cargo")]
        public byte? IdReferenciaCargo { get; set; }

        [Column("id_simbolo_funcao")]
        public byte? IdSimboloFuncao { get; set; }

        [Column("id_lotacao")]
        public short IdLotacao { get; set; }

        [Column("id_tipo_folha")]
        public byte IdTipoFolha { get; set; }

        [Column("ano_mes")]
        public int AnoMes { get; set; }

        [Column("admissao")]
        public short Admissao { get; set; }

        [Column("remun_basica", TypeName = "decimal(10,2)")]
        public decimal? RemunBasica { get; set; }

        [Column("vant_pessoais", TypeName = "decimal(10,2)")]
        public decimal? VantPessoais { get; set; }

        [Column("func_comissionada", TypeName = "decimal(10,2)")]
        public decimal? FuncComissionada { get; set; }

        [Column("grat_natalina", TypeName = "decimal(10,2)")]
        public decimal? GratNatalina { get; set; }

        [Column("horas_extras", TypeName = "decimal(10,2)")]
        public decimal? HorasExtras { get; set; }

        [Column("outras_eventuais", TypeName = "decimal(10,2)")]
        public decimal? OutrasEventuais { get; set; }

        [Column("abono_permanencia", TypeName = "decimal(10,2)")]
        public decimal? AbonoPermanencia { get; set; }

        [Column("reversao_teto_const", TypeName = "decimal(10,2)")]
        public decimal? ReversaoTetoConst { get; set; }

        [Column("imposto_renda", TypeName = "decimal(10,2)")]
        public decimal? ImpostoRenda { get; set; }

        [Column("previdencia", TypeName = "decimal(10,2)")]
        public decimal? Previdencia { get; set; }

        [Column("faltas", TypeName = "decimal(10,2)")]
        public decimal? Faltas { get; set; }

        [Column("rem_liquida", TypeName = "decimal(10,2)")]
        public decimal? RemLiquida { get; set; }

        [Column("diarias", TypeName = "decimal(10,2)")]
        public decimal? Diarias { get; set; }

        [Column("auxilios", TypeName = "decimal(10,2)")]
        public decimal? Auxilios { get; set; }

        [Column("vant_indenizatorias", TypeName = "decimal(10,2)")]
        public decimal? VantIndenizatorias { get; set; }

        [Column("custo_total", TypeName = "decimal(10,2)")]
        public decimal? CustoTotal { get; set; }

        // Navigation properties
        public virtual Vinculo Vinculo { get; set; } = null!;
        public virtual Categoria Categoria { get; set; } = null!;
        public virtual Cargo? Cargo { get; set; }
        public virtual ReferenciaCargo? ReferenciaCargo { get; set; }
        public virtual Funcao? Funcao { get; set; }
        public virtual Lotacao Lotacao { get; set; } = null!;
        public virtual TipoFolha TipoFolha { get; set; } = null!;
    }
}
