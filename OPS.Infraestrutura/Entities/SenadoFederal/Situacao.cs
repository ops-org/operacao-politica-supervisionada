using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_situacao", Schema = "senado")]
    public class Situacao
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("descricao")]
        [StringLength(50)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<SecretarioSenado> Secretarios { get; set; } = new List<SecretarioSenado>();
    }
}
