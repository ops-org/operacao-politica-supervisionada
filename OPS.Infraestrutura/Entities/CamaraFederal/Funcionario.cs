using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_funcionario")]
    public class Funcionario
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("chave")]
        [StringLength(45)]
        public string Chave { get; set; } = null!;

        [Column("nome")]
        [StringLength(100)]
        public string Nome { get; set; } = null!;

        [Column("processado")]
        public sbyte Processado { get; set; }

        [Column("controle")]
        [StringLength(10)]
        public string? Controle { get; set; }

        // Navigation properties
        public virtual ICollection<FuncionarioContratacao> FuncionarioContratacoes { get; set; } = new List<FuncionarioContratacao>();
        public virtual ICollection<FuncionarioRemuneracao> FuncionarioRemuneracoes { get; set; } = new List<FuncionarioRemuneracao>();
    }
}
