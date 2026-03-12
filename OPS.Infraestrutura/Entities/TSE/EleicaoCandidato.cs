using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.TSE
{
    [Table("tse_eleicao_candidato")]
    [DebuggerDisplay("EleicaoCandidato {{Id}} - {{Nome}}")]
    public class EleicaoCandidato
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("cpf")]
        [StringLength(11)]
        public string? Cpf { get; set; }

        [Column("nome")]
        [StringLength(255)]
        public string? Nome { get; set; }

        [Column("email")]
        [StringLength(255)]
        public string? Email { get; set; }
    }
}

