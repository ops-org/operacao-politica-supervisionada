using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("pessoa")]
    public class Pessoa
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("cpf")]
        [StringLength(15)]
        public string? Cpf { get; set; }

        [Column("nome")]
        [StringLength(100)]
        public string? Nome { get; set; }
    }
}
