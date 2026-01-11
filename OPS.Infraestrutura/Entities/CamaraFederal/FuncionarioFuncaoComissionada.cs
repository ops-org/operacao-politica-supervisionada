using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_funcionario_funcao_comissionada")]
    public class FuncionarioFuncaoComissionada
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("nome")]
        [StringLength(255)]
        public string? Nome { get; set; }

        // Navigation properties
        public virtual ICollection<FuncionarioContratacao> FuncionarioContratacoes { get; set; } = new List<FuncionarioContratacao>();
    }
}
