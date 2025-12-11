using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OPS.Infraestrutura.Entities.TSE
{
    [Table("tse_eleicao_candidatura")]
    public class EleicaoCandidatura
    {
        [Column("numero")]
        public uint Numero { get; set; }

        [Column("cargo")]
        public byte Cargo { get; set; }

        [Column("ano")]
        public uint Ano { get; set; }

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

        [Column("sigla_estado")]
        [StringLength(2)]
        [Unicode(false)]
        public string SiglaEstado { get; set; } = string.Empty;

        // Navigation properties
        public virtual EleicaoCandidato? EleicaoCandidato { get; set; }
        public virtual EleicaoCandidato? EleicaoCandidatoVice { get; set; }
    }
}
