using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_senador")]
    public class Senador
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("codigo")]
        public uint? Codigo { get; set; }

        [Column("nome")]
        [StringLength(255)]
        public string? Nome { get; set; }

        [Column("nome_completo")]
        [StringLength(255)]
        public string? NomeCompleto { get; set; }

        [Column("sexo")]
        [StringLength(1)]
        public string? Sexo { get; set; }

        [Column("nascimento")]
        public DateTime? Nascimento { get; set; }

        [Column("naturalidade")]
        [StringLength(50)]
        public string? Naturalidade { get; set; }

        [Column("id_estado_naturalidade")]
        public byte? IdEstadoNaturalidade { get; set; }

        [Column("profissao")]
        [StringLength(100)]
        public string? Profissao { get; set; }

        [Column("id_partido")]
        public byte IdPartido { get; set; }

        [Column("id_estado")]
        public byte? IdEstado { get; set; }

        [Column("email")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Column("site")]
        [StringLength(100)]
        public string? Site { get; set; }

        [Column("ativo")]
        [StringLength(1)]
        public string? Ativo { get; set; }

        [Column("nome_importacao")]
        [StringLength(255)]
        public string? NomeImportacao { get; set; }

        [Column("valor_total_ceaps", TypeName = "decimal(16,2)")]
        public decimal ValorTotalCeaps { get; set; }

        [Column("valor_total_remuneracao", TypeName = "decimal(16,2)")]
        public decimal ValorTotalRemuneracao { get; set; }

        [Column("hash")]
        public byte[]? Hash { get; set; }

        // Navigation properties
        public virtual ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();
        public virtual ICollection<Mandato> Mandatos { get; set; } = new List<Mandato>();
        public virtual ICollection<MandatoExercicio> MandatoExercicios { get; set; } = new List<MandatoExercicio>();
        public virtual ICollection<Lotacao> Lotacoes { get; set; } = new List<Lotacao>();
        public virtual ICollection<Secretario> Secretarios { get; set; } = new List<Secretario>();
        public virtual ICollection<SecretarioCompleto> SecretariosCompletos { get; set; } = new List<SecretarioCompleto>();
        public virtual SenadorCampeaoGasto? CampeaoGasto { get; set; }
        public virtual ICollection<SenadorHistoricoAcademico> HistoricoAcademico { get; set; } = new List<SenadorHistoricoAcademico>();
        public virtual ICollection<SenadorProfissao> Profissoes { get; set; } = new List<SenadorProfissao>();
        public virtual ICollection<SenadorPartido> Partidos { get; set; } = new List<SenadorPartido>();
    }
}
