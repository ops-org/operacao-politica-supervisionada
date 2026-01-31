using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_funcao")]
    public class Funcao
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("descricao")]
        [StringLength(5)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<Remuneracao> Remuneracoes { get; set; } = new List<Remuneracao>();
    }
}
