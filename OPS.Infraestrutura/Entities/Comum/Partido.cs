using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using OPS.Infraestrutura.Entities.SenadoFederal;

namespace OPS.Infraestrutura.Entities.Comum
{
    [DebuggerDisplay("Id = {Id}, Legenda = {Legenda}, Sigla = {Sigla}, Nome = {Nome}, Imagem = {Imagem}")]
    [Table("partido")]
    public class Partido
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("legenda")]
        public short? Legenda { get; set; }

        [Column("sigla")]
        [StringLength(20)]
        public string Sigla { get; set; } = null!;

        [Column("nome")]
        [StringLength(100)]
        public string? Nome { get; set; }

        [Column("imagem")]
        [StringLength(100)]
        public string? Imagem { get; set; }

        [Column("ativo")]
        public bool Ativo { get; set; } = true;

        [Column("sede")]
        [StringLength(2)]
        public string? Sede { get; set; }

        [Column("fundacao")]
        public DateTime? Fundacao { get; set; }

        [Column("registro_solicitacao")]
        public DateTime? RegistroSolicitacao { get; set; }

        [Column("registro_provisorio")]
        public DateTime? RegistroProvisorio { get; set; }

        [Column("registro_definitivo")]
        public DateTime? RegistroDefinitivo { get; set; }

        [Column("extincao")]
        public DateTime? Extincao { get; set; }

        [Column("motivo")]
        [StringLength(500)]
        public string? Motivo { get; set; }

        // Navigation properties
        public virtual ICollection<AssembleiasLegislativas.DeputadoEstadual> DeputadosEstaduais { get; set; } = new List<AssembleiasLegislativas.DeputadoEstadual>();
        public virtual ICollection<CamaraFederal.Deputado> DeputadosFederais { get; set; } = new List<CamaraFederal.Deputado>();
        //public virtual ICollection<PartidoHistorico> PartidoHistoricos { get; set; } = new List<PartidoHistorico>();
        //public virtual ICollection<SenadoFederal.MandatoSenado> Mandatos { get; set; } = new List<SenadoFederal.MandatoSenado>();
        //public virtual ICollection<SenadoFederal.SenadorPartido> SenadorPartidos { get; set; } = new List<SenadoFederal.SenadorPartido>();
    }
}
