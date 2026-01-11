using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE
{
    [Table("tse_eleicao_cargo")]
    public class EleicaoCargo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("nome")]
        [StringLength(50)]
        public string? Nome { get; set; }
    }
}
