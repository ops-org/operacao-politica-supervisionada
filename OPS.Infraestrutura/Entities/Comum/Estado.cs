using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("estado")]
    public class Estado
    {
        [Key]
        [Column("id")]
        public byte Id { get; set; }

        [Column("sigla")]
        [StringLength(2)]
        public string Sigla { get; set; } = null!;

        [Column("nome")]
        [StringLength(30)]
        public string Nome { get; set; } = null!;

        [Column("regiao")]
        [StringLength(30)]
        public string Regiao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<CamaraFederal.Deputado> DeputadosFedarais { get; set; } = new List<CamaraFederal.Deputado>();
        public virtual ICollection<AssembleiasLegislativas.Deputado> DeputadosEstaduais { get; set; } = new List<AssembleiasLegislativas.Deputado>();
        //public virtual ICollection<CamaraFederal.Mandato> MandatosCamaraFederal { get; set; } = new List<CamaraFederal.Mandato>();
        //public virtual ICollection<SenadoFederal.Mandato> MandatosSenadoFederal { get; set; } = new List<SenadoFederal.Mandato>();
    }
}
