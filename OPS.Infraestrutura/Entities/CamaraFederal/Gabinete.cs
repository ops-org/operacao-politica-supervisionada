using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_gabinete")]
    [DebuggerDisplay("Gabinete {Id} - {Nome} ({Predio} {Andar} {Sala})")]
    public class Gabinete
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("nome")]
        [StringLength(50)]
        public string? Nome { get; set; }

        [Column("predio")]
        [StringLength(50)]
        public string? Predio { get; set; }

        [Column("andar")]
        public short? Andar { get; set; }

        [Column("sala")]
        [StringLength(50)]
        public string? Sala { get; set; }

        [Column("telefone")]
        [StringLength(20)]
        public string? Telefone { get; set; }

        // Navigation properties
        public virtual ICollection<Deputado> Deputados { get; set; } = new List<Deputado>();
    }
}
