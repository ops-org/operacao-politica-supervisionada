using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_senador_historico_academico")]
    public class SenadorHistoricoAcademico
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_sf_senador")]
        public uint IdSenador { get; set; }

        [Column("instituicao")]
        [StringLength(255)]
        public string? Instituicao { get; set; }

        [Column("curso")]
        [StringLength(255)]
        public string? Curso { get; set; }

        [Column("nivel")]
        [StringLength(50)]
        public string? Nivel { get; set; }

        [Column("ano_inicio")]
        public ushort? AnoInicio { get; set; }

        [Column("ano_conclusao")]
        public ushort? AnoConclusao { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
    }
}
