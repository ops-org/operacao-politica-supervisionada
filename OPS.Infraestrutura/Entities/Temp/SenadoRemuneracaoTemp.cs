using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class SenadoRemuneracaoTemp
    {
        [Column("ano_mes")]
        public int? AnoMes { get; set; }

        [Column("vinculo")]
        public string? Vinculo { get; set; }

        [Column("categoria")]
        public string? Categoria { get; set; }

        [Column("cargo")]
        public string? Cargo { get; set; }

        [Column("referencia_cargo")]
        public string? ReferenciaCargo { get; set; }

        [Column("simbolo_funcao")]
        public string? SimboloFuncao { get; set; }

        [Column("lotacao_exercicio")]
        public string? LotacaoExercicio { get; set; }

        [Column("tipo_folha")]
        public string? TipoFolha { get; set; }

        [Column("admissao")]
        public int? Admissao { get; set; }

        [Column("remun_basica")]
        public decimal? RemunBasica { get; set; }

        [Column("vant_pessoais")]
        public decimal? VantPessoais { get; set; }

        [Column("func_comissionada")]
        public decimal? FuncComissionada { get; set; }

        [Column("grat_natalina")]
        public decimal? GratNatalina { get; set; }

        [Column("horas_extras")]
        public decimal? HorasExtras { get; set; }

        [Column("outras_eventuais")]
        public decimal? OutrasEventuais { get; set; }

        [Column("abono_permanencia")]
        public decimal? AbonoPermanencia { get; set; }

        [Column("reversao_teto_const")]
        public decimal? ReversaoTetoConst { get; set; }

        [Column("imposto_renda")]
        public decimal? ImpostoRenda { get; set; }

        [Column("previdencia")]
        public decimal? Previdencia { get; set; }

        [Column("faltas")]
        public decimal? Faltas { get; set; }

        [Column("rem_liquida")]
        public decimal? RemLiquida { get; set; }

        [Column("diarias")]
        public decimal? Diarias { get; set; }

        [Column("auxilios")]
        public decimal? Auxilios { get; set; }

        [Column("vant_indenizatorias")]
        public decimal? VantIndenizatorias { get; set; }
    }
}
