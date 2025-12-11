using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_sessao_presenca")]
    public class SessaoPresenca
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_cf_sessao")]
        public uint IdSessao { get; set; }

        [Column("id_cf_deputado")]
        public uint IdDeputado { get; set; }

        [Column("presente")]
        [StringLength(1)]
        public string Presente { get; set; } = null!;

        [Column("justificativa")]
        [StringLength(100)]
        public string? Justificativa { get; set; }

        [Column("presenca_externa")]
        [StringLength(1)]
        public string PresencaExterna { get; set; } = null!;

        // Navigation properties
        public virtual Sessao Sessao { get; set; } = null!;
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
