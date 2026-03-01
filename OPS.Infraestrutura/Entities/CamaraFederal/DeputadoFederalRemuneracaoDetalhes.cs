using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_deputado_remuneracao_detalhes")]
    public class DeputadoFederalRemuneracaoDetalhes
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_cf_deputado")]
        public int? IdDeputado { get; set; }

        [Column("referencia")]
        public DateTime Referencia { get; set; }

        [Column("tipo")]
        public short Tipo { get; set; }

        [Column("remuneracao_fixa")]
        public decimal RemuneracaoFixa { get; set; }

        [Column("vantagens_natureza_pessoal")]
        public decimal VantagensNaturezaPessoal { get; set; }

        [Column("funcao_ou_cargo_em_comissao")]
        public decimal FuncaoOuCargoEmComissao { get; set; }

        [Column("gratificacao_natalina")]
        public decimal GratificacaoNatalina { get; set; }

        [Column("ferias")]
        public decimal Ferias { get; set; }

        [Column("outras_remuneracoes")]
        public decimal OutrasRemuneracoes { get; set; }

        [Column("abono_permanencia")]
        public decimal AbonoPermanencia { get; set; }

        [Column("valor_bruto")]
        public decimal? ValorBruto { get; set; }

        [Column("redutor_constitucional")]
        public decimal RedutorConstitucional { get; set; }

        [Column("contribuicao_previdenciaria")]
        public decimal ContribuicaoPrevidenciaria { get; set; }

        [Column("imposto_renda")]
        public decimal ImpostoRenda { get; set; }

        [Column("valor_liquido")]
        public decimal ValorLiquido { get; set; }

        [Column("valor_diarias")]
        public decimal ValorDiarias { get; set; }

        [Column("valor_auxilios")]
        public decimal ValorAuxilios { get; set; }

        [Column("valor_vantagens")]
        public decimal ValorVantagens { get; set; }

        [Column("valor_outros")]
        public decimal? ValorOutros { get; set; }

        [Column("valor_total")]
        public decimal? ValorTotal { get; set; }

        [Column("contratacao")]
        public DateOnly? Contratacao { get; set; }

        // Navigation property
        public virtual Deputado? Deputado { get; set; }
    }

}
