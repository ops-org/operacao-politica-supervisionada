using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_sessao")]
    public class Sessao
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_legislatura")]
        public byte IdLegislatura { get; set; }

        [Column("data")]
        public DateTime Data { get; set; }

        [Column("inicio")]
        public DateTime Inicio { get; set; }

        [Column("tipo")]
        public byte Tipo { get; set; }

        [Column("numero")]
        [StringLength(45)]
        public string? Numero { get; set; }

        [Column("presencas")]
        public ushort Presencas { get; set; }

        [Column("ausencias")]
        public ushort Ausencias { get; set; }

        [Column("ausencias_justificadas")]
        public ushort AusenciasJustificadas { get; set; }

        [Column("checksum")]
        [StringLength(100)]
        public string? Checksum { get; set; }

        // Navigation properties
        public virtual LegislaturaCamara Legislatura { get; set; } = null!;
        public virtual ICollection<SessaoPresenca> SessaoPresencas { get; set; } = new List<SessaoPresenca>();
    }
}
