using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.Comum;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_mandato")]
    public class MandatoCamara
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("id_cf_deputado")]
        public int IdDeputado { get; set; }

        [Column("id_legislatura")]
        public byte? IdLegislatura { get; set; }

        [Column("id_carteira_parlamantar")]
        public int? IdCarteiraParlamantar { get; set; }

        [Column("id_estado")]
        public byte? IdEstado { get; set; }

        [Column("id_partido")]
        public byte? IdPartido { get; set; }

        [Column("condicao")]
        [StringLength(10)]
        public string? Condicao { get; set; }

        [Column("valor_total_ceap", TypeName = "decimal(26,2)")]
        public decimal? ValorTotalCeap { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
        public virtual LegislaturaCamara? Legislatura { get; set; }
        public virtual Estado? Estado { get; set; }
        public virtual Partido? Partido { get; set; }
        public virtual ICollection<DespesaCamara> Despesas { get; set; } = new List<DespesaCamara>();
    }
}
