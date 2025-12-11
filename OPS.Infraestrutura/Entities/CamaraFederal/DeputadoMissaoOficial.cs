using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_deputado_missao_oficial")]
    public class DeputadoMissaoOficial
    {
        [Key]
        [Column("id_cf_deputado")]
        public uint IdDeputado { get; set; }

        // [Column("id_trecho_viagem")]
        // public ushort? IdTrechoViagem { get; set; }

        [Column("data_saida")]
        public DateTime? DataSaida { get; set; }

        [Column("data_chegada")]
        public DateTime? DataChegada { get; set; }

        [Column("descricao")]
        [StringLength(255)]
        public string? Descricao { get; set; }

        [Column("valor", TypeName = "decimal(10,2)")]
        public decimal? Valor { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
        public virtual TrechoViagem? TrechoViagem { get; set; }
    }
}
