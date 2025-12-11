using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_legislatura")]
    public class Legislatura
    {
        [Key]
        [Column("id")]
        public byte Id { get; set; }

        [Column("ano")]
        public ushort? Ano { get; set; }

        [Column("inicio")]
        public uint? Inicio { get; set; }

        [Column("final")]
        public uint? Final { get; set; }

        // Navigation properties
        public virtual ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();
        public virtual ICollection<Mandato> Mandatos { get; set; } = new List<Mandato>();
        public virtual ICollection<Sessao> Sessoes { get; set; } = new List<Sessao>();
    }
}
