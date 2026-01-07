using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_despesa_tipo")]
    public class DespesaTipoCamara
    {
        [Key]
        [Column("id")]
        public ushort Id { get; set; }

        [Column("descricao")]
        [StringLength(100)]
        public string? Descricao { get; set; }

        // Navigation properties
        public virtual ICollection<DespesaCamara> Despesas { get; set; } = new List<DespesaCamara>();
        public virtual ICollection<EspecificacaoTipo> EspecificacaoTipos { get; set; } = new List<EspecificacaoTipo>();
    }
}
