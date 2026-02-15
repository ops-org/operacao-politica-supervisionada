using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("municipio")]
    public class Municipio
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("id_estado")]
        public short IdEstado { get; set; }

        [Column("nome")]
        [StringLength(30)]
        public string Nome { get; set; } = null!;

    }
}
