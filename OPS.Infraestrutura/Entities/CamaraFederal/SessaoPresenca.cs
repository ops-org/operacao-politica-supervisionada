using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_sessao_presenca")]
    public class SessaoPresenca
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("id_cf_sessao")]
        public int IdSessao { get; set; }

        [Column("id_cf_deputado")]
        public int IdDeputado { get; set; }

        [Column("presente")]
        public bool Presente { get; set; }

        [Column("justificativa")]
        [StringLength(100)]
        public string? Justificativa { get; set; }

        [Column("presenca_externa")]
        public bool PresencaExterna { get; set; }

        // Navigation properties
        public virtual Sessao Sessao { get; set; } = null!;
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
