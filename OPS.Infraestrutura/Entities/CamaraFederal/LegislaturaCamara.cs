using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_legislatura")]
    public class LegislaturaCamara
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("ano")]
        public short? Ano { get; set; }

        [Column("inicio")]
        public int? Inicio { get; set; }

        [Column("final")]
        public int? Final { get; set; }

        // Navigation properties
        public virtual ICollection<DespesaCamara> Despesas { get; set; } = new List<DespesaCamara>();
        public virtual ICollection<MandatoCamara> Mandatos { get; set; } = new List<MandatoCamara>();
        public virtual ICollection<Sessao> Sessoes { get; set; } = new List<Sessao>();
    }
}
