using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("partido")]
    public class Partido
    {
        [Key]
        [Column("id")]
        public byte Id { get; set; }

        [Column("legenda")]
        public byte? Legenda { get; set; }

        [Column("sigla")]
        [StringLength(20)]
        public string Sigla { get; set; } = null!;

        [Column("nome")]
        [StringLength(100)]
        public string? Nome { get; set; }

        [Column("imagem")]
        [StringLength(100)]
        public string? Imagem { get; set; }

        // Navigation properties
        public virtual ICollection<AssembleiasLegislativas.Deputado> DeputadosEstaduais { get; set; } = new List<AssembleiasLegislativas.Deputado>();
        public virtual ICollection<CamaraFederal.Deputado> DeputadosFederais { get; set; } = new List<CamaraFederal.Deputado>();
        public virtual ICollection<PartidoHistorico> PartidoHistoricos { get; set; } = new List<PartidoHistorico>();
        public virtual ICollection<SenadoFederal.MandatoSenado> Mandatos { get; set; } = new List<SenadoFederal.MandatoSenado>();
        public virtual ICollection<SenadoFederal.SenadorPartido> SenadorPartidos { get; set; } = new List<SenadoFederal.SenadorPartido>();
    }
}
