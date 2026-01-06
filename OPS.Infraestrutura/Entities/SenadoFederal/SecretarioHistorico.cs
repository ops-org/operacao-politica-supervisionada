using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_secretario_historico")]
    public class SecretarioHistorico
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_sf_senador")]
        public uint IdSenador { get; set; }

        [Column("ano_mes")]
        public int? AnoMes { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
    }
}
