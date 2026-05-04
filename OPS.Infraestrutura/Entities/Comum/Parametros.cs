using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("parametros")]
    [DebuggerDisplay("Parametros {{Id}} - {{Descricao}}")]
    public class Parametros
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("descricao")]
        [StringLength(100)]
        public string Descricao { get; set; } = null!;

        [Column("valor")]
        [StringLength(255)]
        public string Valor { get; set; } = null!;
    }
}

