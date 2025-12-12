using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.Comum;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_senador_profissao")]
    public class SenadorProfissao
    {
        [Key]
        [Column("id_sf_senador")]
        public uint IdSenador { get; set; }

        [Key]
        [Column("id_profissao")]
        public uint IdProfissao { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
        public virtual Profissao Profissao { get; set; } = null!;
    }
}
