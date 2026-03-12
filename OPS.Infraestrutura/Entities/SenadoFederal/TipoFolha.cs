using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_tipo_folha")]
    [DebuggerDisplay("TipoFolha {{Id}} - {{Descricao}}")]
    public class TipoFolha
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("descricao")]
        [StringLength(100)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<Remuneracao> Remuneracoes { get; set; } = new List<Remuneracao>();
    }
}

