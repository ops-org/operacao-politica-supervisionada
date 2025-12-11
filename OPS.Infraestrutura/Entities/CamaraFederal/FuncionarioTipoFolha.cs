using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_funcionario_tipo_folha")]
    public class FuncionarioTipoFolha
    {
        [Key]
        [Column("id")]
        public byte Id { get; set; }

        [Column("descricao")]
        [StringLength(50)]
        public string? Descricao { get; set; }

        // Navigation properties
        public virtual ICollection<FuncionarioRemuneracao> FuncionarioRemuneracoes { get; set; } = new List<FuncionarioRemuneracao>();
    }
}
