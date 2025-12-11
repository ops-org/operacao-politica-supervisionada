using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("trecho_viagem")]
    public class TrechoViagem
    {
        [Key]
        [Column("id")]
        public ushort Id { get; set; }

        [Column("descricao")]
        [StringLength(200)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();
    }
}
