using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_funcionario_remuneracao")]
    public class FuncionarioRemuneracao
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_cf_funcionario")]
        public uint IdFuncionario { get; set; }

        [Column("id_cf_funcionario_contratacao")]
        public uint? IdFuncionarioContratacao { get; set; }

        [Column("id_cf_deputado")]
        public uint? IdDeputado { get; set; }

        [Column("referencia")]
        public DateTime Referencia { get; set; }

        [Column("tipo")]
        public byte? Tipo { get; set; }

        [Column("remuneracao_fixa", TypeName = "decimal(10,2)")]
        public decimal? RemuneracaoFixa { get; set; }

        [Column("vantagens_natureza_pessoal", TypeName = "decimal(10,2)")]
        public decimal? VantagensNaturezaPessoal { get; set; }

        [Column("funcao_ou_cargo_em_comissao", TypeName = "decimal(10,2)")]
        public decimal? FuncaoOuCargoEmComissao { get; set; }

        [Column("gratificacao_natalina", TypeName = "decimal(10,2)")]
        public decimal? GratificacaoNatalina { get; set; }

        [Column("ferias", TypeName = "decimal(10,2)")]
        public decimal? Ferias { get; set; }

        [Column("outras_remuneracoes", TypeName = "decimal(10,2)")]
        public decimal? OutrasRemuneracoes { get; set; }

        [Column("abono_permanencia", TypeName = "decimal(10,2)")]
        public decimal? AbonoPermanencia { get; set; }

        [Column("valor_bruto", TypeName = "decimal(10,2)")]
        public decimal? ValorBruto { get; set; }

        [Column("redutor_constitucional", TypeName = "decimal(10,2)")]
        public decimal? RedutorConstitucional { get; set; }

        [Column("contribuicao_previdenciaria", TypeName = "decimal(10,2)")]
        public decimal? ContribuicaoPrevidenciaria { get; set; }

        [Column("imposto_renda", TypeName = "decimal(10,2)")]
        public decimal? ImpostoRenda { get; set; }

        [Column("valor_liquido", TypeName = "decimal(10,2)")]
        public decimal? ValorLiquido { get; set; }

        [Column("valor_diarias", TypeName = "decimal(10,2)")]
        public decimal? ValorDiarias { get; set; }

        [Column("valor_auxilios", TypeName = "decimal(10,2)")]
        public decimal? ValorAuxilios { get; set; }

        [Column("valor_vantagens", TypeName = "decimal(10,2)")]
        public decimal? ValorVantagens { get; set; }

        [Column("valor_outros", TypeName = "decimal(10,2)")]
        public decimal? ValorOutros { get; set; }

        [Column("valor_total", TypeName = "decimal(10,2)")]
        public decimal? ValorTotal { get; set; }

        [Column("nivel")]
        [StringLength(5)]
        public string? Nivel { get; set; }

        [Column("contratacao")]
        public DateTime? Contratacao { get; set; }

        // Navigation properties
        public virtual Funcionario Funcionario { get; set; } = null!;
        public virtual FuncionarioContratacao? FuncionarioContratacao { get; set; }
        public virtual Deputado? Deputado { get; set; }
    }
}
