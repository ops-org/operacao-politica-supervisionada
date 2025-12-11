using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("partido_historico")]
    public class PartidoHistorico
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_partido")]
        public byte IdPartido { get; set; }

        [Column("sigla")]
        [StringLength(20)]
        public string Sigla { get; set; } = null!;

        [Column("nome")]
        [StringLength(255)]
        public string Nome { get; set; } = null!;

        [Column("data_inicio")]
        public DateTime? DataInicio { get; set; }

        [Column("data_fim")]
        public DateTime? DataFim { get; set; }

        // Navigation properties
        public virtual Partido Partido { get; set; } = null!;
    }
}
