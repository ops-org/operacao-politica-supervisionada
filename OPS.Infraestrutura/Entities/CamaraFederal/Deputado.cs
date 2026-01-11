using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.Comum;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_deputado")]
    public class Deputado
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_deputado")]
        public int? IdDeputado { get; set; }

        [Column("id_partido")]
        public byte IdPartido { get; set; }

        [Column("id_estado")]
        public byte? IdEstado { get; set; }

        [Column("id_cf_gabinete")]
        public short? IdGabinete { get; set; }

        [Column("cpf")]
        [StringLength(15)]
        public string? Cpf { get; set; }

        [Column("nome_parlamentar")]
        [StringLength(100)]
        public string? NomeParlamentar { get; set; }

        [Column("nome_civil")]
        [StringLength(100)]
        public string? NomeCivil { get; set; }

        [Column("nome_importacao_presenca")]
        [StringLength(100)]
        public string? NomeImportacaoPresenca { get; set; }

        [Column("sexo")]
        [StringLength(2)]
        public string? Sexo { get; set; }

        [Column("email")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Column("nascimento")]
        public DateTime? Nascimento { get; set; }

        [Column("falecimento")]
        public DateTime? Falecimento { get; set; }

        [Column("id_estado_nascimento")]
        public byte? IdEstadoNascimento { get; set; }

        [Column("municipio")]
        [StringLength(500)]
        public string? Municipio { get; set; }

        [Column("website")]
        [StringLength(255)]
        public string? Website { get; set; }

        [Column("profissao")]
        [StringLength(255)]
        public string? Profissao { get; set; }

        [Column("escolaridade")]
        [StringLength(100)]
        public string? Escolaridade { get; set; }

        [Column("condicao")]
        [StringLength(50)]
        public string? Condicao { get; set; }

        [Column("situacao")]
        [StringLength(20)]
        public string? Situacao { get; set; }

        [Column("passaporte_diplomatico")]
        public bool PassaporteDiplomatico { get; set; }

        [Column("processado")]
        public sbyte Processado { get; set; }

        [Column("valor_total_ceap", TypeName = "decimal(16,2)")]
        public decimal ValorTotalCeap { get; set; }

        [Column("secretarios_ativos")]
        public byte? SecretariosAtivos { get; set; }

        [Column("valor_mensal_secretarios", TypeName = "decimal(16,2)")]
        public decimal ValorMensalSecretarios { get; set; }

        [Column("valor_total_remuneracao", TypeName = "decimal(16,2)")]
        public decimal ValorTotalRemuneracao { get; set; }

        [Column("valor_total_salario", TypeName = "decimal(16,2)")]
        public decimal ValorTotalSalario { get; set; }

        [Column("valor_total_auxilio_moradia", TypeName = "decimal(16,2)")]
        public decimal ValorTotalAuxilioMoradia { get; set; }

        // Navigation properties
        public virtual Partido? Partido { get; set; }
        public virtual Estado? Estado { get; set; }
        public virtual Estado? EstadoNascimento { get; set; }
        public virtual Gabinete? Gabinete { get; set; }
        public virtual ICollection<DespesaCamara> Despesas { get; set; } = new List<DespesaCamara>();
        public virtual ICollection<MandatoCamara> Mandatos { get; set; } = new List<MandatoCamara>();
        public virtual ICollection<SessaoPresenca> SessaoPresencas { get; set; } = new List<SessaoPresenca>();
        public virtual ICollection<SecretarioCamara> Secretarios { get; set; } = new List<SecretarioCamara>();
        public virtual ICollection<FuncionarioContratacao> FuncionarioContratacoes { get; set; } = new List<FuncionarioContratacao>();
        public virtual ICollection<FuncionarioRemuneracao> FuncionarioRemuneracoes { get; set; } = new List<FuncionarioRemuneracao>();
    }
}
