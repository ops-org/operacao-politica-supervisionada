using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE
{
    [Table("tse_eleicao")]
    public class Eleicao
    {
        [Column("id")]
        public int? Id { get; set; }

        [Column("descricao")]
        [StringLength(50)]
        public string? Descricao { get; set; }

        [Column("data")]
        public DateTime? Data { get; set; }

        [Column("turno")]
        [StringLength(50)]
        public string? Turno { get; set; }

        [Column("tipo")]
        [StringLength(50)]
        public string? Tipo { get; set; }

        [Column("abrangencia")]
        [StringLength(50)]
        public string? Abrangencia { get; set; }
    }
}
