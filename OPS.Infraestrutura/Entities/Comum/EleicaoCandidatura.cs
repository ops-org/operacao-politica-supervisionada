using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("eleicao_candidatura")]
    public class EleicaoCandidatura
    {
        [Key]
        [Column("numero", Order = 1)]
        public uint Numero { get; set; }

        [Key]
        [Column("cargo", Order = 2)]
        public byte Cargo { get; set; }

        [Key]
        [Column("ano", Order = 3)]
        public uint Ano { get; set; }

        [Key]
        [Column("sigla_estado", Order = 4)]
        [StringLength(2)]
        public string SiglaEstado { get; set; } = null!;

        [Column("id_eleicao_candidato")]
        public int? IdEleicaoCandidato { get; set; }

        [Column("id_eleicao_candidato_vice")]
        public int? IdEleicaoCandidatoVice { get; set; }

        [Column("sigla_partido")]
        [StringLength(50)]
        public string? SiglaPartido { get; set; }

        [Column("sigla_partido_vice")]
        [StringLength(50)]
        public string? SiglaPartidoVice { get; set; }

        [Column("nome_urna")]
        [StringLength(255)]
        public string? NomeUrna { get; set; }

        [Column("nome_urna_vice")]
        [StringLength(255)]
        public string? NomeUrnaVice { get; set; }

        [Column("sequencia")]
        [StringLength(50)]
        public string? Sequencia { get; set; }

        [Column("sequencia_vice")]
        [StringLength(50)]
        public string? SequenciaVice { get; set; }
    }
}
