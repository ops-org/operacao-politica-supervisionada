using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_sessao")]
    public class Sessao
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_legislatura")]
        public short IdLegislatura { get; set; }

        [Column("data")]
        public DateTime Data { get; set; }

        [Column("inicio")]
        public DateTime Inicio { get; set; }

        [Column("tipo")]
        public short Tipo { get; set; }

        [Column("numero")]
        [StringLength(45)]
        public string? Numero { get; set; }

        [Column("presencas")]
        public short Presencas { get; set; }

        [Column("ausencias")]
        public short Ausencias { get; set; }

        [Column("ausencias_justificadas")]
        public short AusenciasJustificadas { get; set; }

        [Column("checksum")]
        [StringLength(100)]
        public string? Checksum { get; set; }

        // Navigation properties
        public virtual LegislaturaCamara Legislatura { get; set; } = null!;
        public virtual ICollection<SessaoPresenca> SessaoPresencas { get; set; } = new List<SessaoPresenca>();
    }
}
