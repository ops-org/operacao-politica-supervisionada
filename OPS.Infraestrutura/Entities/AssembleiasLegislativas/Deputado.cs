using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.Comum;

namespace OPS.Infraestrutura.Entities.AssembleiasLegislativas
{
    [Table("cl_deputado")]
    public class Deputado
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("nome")]
        [StringLength(255)]
        public string Nome { get; set; } = null!;

        [Column("nome_parlamentar")]
        [StringLength(255)]
        public string? NomeParlamentar { get; set; }

        [Column("id_partido")]
        public byte? IdPartido { get; set; }

        [Column("id_estado")]
        public byte? IdEstado { get; set; }

        [Column("sexo")]
        [StringLength(1)]
        public string? Sexo { get; set; }

        [Column("nascimento")]
        public DateTime? Nascimento { get; set; }

        [Column("email")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Column("site")]
        [StringLength(100)]
        public string? Site { get; set; }

        [Column("ativo")]
        public bool Ativo { get; set; }

        // Navigation properties
        public virtual Partido? Partido { get; set; }
        public virtual Estado? Estado { get; set; }

        public virtual ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();
    }
}
