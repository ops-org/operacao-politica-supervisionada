using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("partido_historico")]
    public class PartidoHistorico
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("legenda")]
        public short? Legenda { get; set; }

        [Column("sigla")]
        [StringLength(20)]
        public string? Sigla { get; set; }

        [Column("nome")]
        [StringLength(100)]
        public string? Nome { get; set; }

        [Column("sede")]
        [StringLength(2)]
        public string? Sede { get; set; }

        [Column("fundacao")]
        public DateOnly? Fundacao { get; set; }

        [Column("registro_solicitacao")]
        public DateOnly? RegistroSolicitacao { get; set; }

        [Column("registro_provisorio")]
        public DateOnly? RegistroProvisorio { get; set; }

        [Column("registro_definitivo")]
        public DateOnly? RegistroDefinitivo { get; set; }

        [Column("extincao")]
        public DateOnly? Extincao { get; set; }

        [Column("motivo")]
        [StringLength(500)]
        public string? Motivo { get; set; }
    }
}
