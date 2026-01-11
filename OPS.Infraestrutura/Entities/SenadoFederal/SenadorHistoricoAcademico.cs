using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_senador_historico_academico")]
    public class SenadorHistoricoAcademico
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_sf_senador")]
        public int IdSenador { get; set; }

        [Column("nome_curso")]
        [StringLength(255)]
        public string? NomeCurso { get; set; }

        [Column("grau_instrucao")]
        [StringLength(50)]
        public string? GrauInstrucao { get; set; }

        [Column("estabelecimento")]
        [StringLength(255)]
        public string? Estabelecimento { get; set; }

        [Column("local")]
        [StringLength(255)]
        public string? Local { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
    }
}
