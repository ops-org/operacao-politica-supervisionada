using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_lotacao")]
    public class Lotacao
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("id_senador")]
        public int? IdSenador { get; set; }

        [Column("descricao")]
        [StringLength(100)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual Senador? Senador { get; set; }
        public virtual ICollection<Remuneracao> Remuneracoes { get; set; } = new List<Remuneracao>();
    }
}
