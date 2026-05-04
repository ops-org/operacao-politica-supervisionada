using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using OPS.Infraestrutura.Entities.Comum;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_senador_profissao")]
    [DebuggerDisplay("SenadorProfissao {{Id}} - {{Nome}}")]
    public class SenadorProfissao
    {
        [Key]
        [Column("id_sf_senador")]
        public int IdSenador { get; set; }

        [Key]
        [Column("id_profissao")]
        public int IdProfissao { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
        public virtual Profissao Profissao { get; set; } = null!;
    }
}

