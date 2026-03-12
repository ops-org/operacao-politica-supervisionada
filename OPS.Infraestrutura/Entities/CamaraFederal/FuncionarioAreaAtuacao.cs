using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_funcionario_area_atuacao")]
    [DebuggerDisplay("FuncionarioAreaAtuacao {{Id}} - {{Nome}}")]
    public class FuncionarioAreaAtuacao
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("nome")]
        [StringLength(50)]
        public string? Nome { get; set; }

        // Navigation properties
        public virtual ICollection<FuncionarioContratacao> FuncionarioContratacoes { get; set; } = new List<FuncionarioContratacao>();
    }
}

