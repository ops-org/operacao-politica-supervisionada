using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.Comum;

namespace OPS.Infraestrutura.Entities.AssembleiasLegislativas
{
    [Table("cl_deputado")]
    public class Deputado
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("matricula")]
        public int? Matricula { get; set; }

        [Column("gabinete")]
        public int? Gabinete { get; set; }

        [Column("id_partido")]
        public byte IdPartido { get; set; }

        [Column("id_estado")]
        public byte IdEstado { get; set; }

        [Column("cpf")]
        [StringLength(11)]
        public string? Cpf { get; set; }

        [Column("cpf_parcial")]
        [StringLength(6)]
        public string? CpfParcial { get; set; }

        [Column("nome_parlamentar")]
        [StringLength(255)]
        public string NomeParlamentar { get; set; } = null!;

        [Column("nome_civil")]
        [StringLength(255)]
        public string? NomeCivil { get; set; }

        [Column("nome_importacao")]
        [StringLength(255)]
        public string? NomeImportacao { get; set; }

        [Column("nascimento")]
        public DateTime? Nascimento { get; set; }

        [Column("sexo")]
        [StringLength(2)]
        public string? Sexo { get; set; }

        [Column("email")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Column("naturalidade")]
        [StringLength(100)]
        public string? Naturalidade { get; set; }

        [Column("escolaridade")]
        [StringLength(100)]
        public string? Escolaridade { get; set; }

        [Column("profissao")]
        [StringLength(150)]
        public string? Profissao { get; set; }

        [Column("telefone")]
        [StringLength(100)]
        public string? Telefone { get; set; }

        [Column("site")]
        [StringLength(500)]
        public string? Site { get; set; }

        [Column("perfil")]
        [StringLength(100)]
        public string? Perfil { get; set; }

        [Column("foto")]
        [StringLength(200)]
        public string? Foto { get; set; }

        [Column("valor_total_ceap", TypeName = "decimal(12,2)")]
        public decimal ValorTotalCeap { get; set; }

        [Column("valor_total_remuneracao", TypeName = "decimal(12,2)")]
        public decimal ValorTotalRemuneracao { get; set; }

        //[Column("ativo")]
        //public bool Ativo { get; set; }

        // Navigation properties
        public virtual Partido? Partido { get; set; }
        public virtual Estado? Estado { get; set; }

        public virtual ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();
    }
}
