using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_funcionario_cargo")]
    [DebuggerDisplay("FuncionarioCargo {Id} - {Nome}")]
    public class FuncionarioCargo
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("nome")]
        [StringLength(100)]
        public string? Nome { get; set; }

        // Navigation properties
        public virtual ICollection<FuncionarioContratacao> FuncionarioContratacoes { get; set; } = new List<FuncionarioContratacao>();
    }
}
