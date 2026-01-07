using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_especificacao_tipo")]
    public class EspecificacaoTipo
    {
        [Key]
        [Column("id_cf_despesa_tipo")]
        public ushort IdDespesaTipo { get; set; }

        [Key]
        [Column("id_cf_especificacao")]
        public byte IdEspecificacao { get; set; }

        [Column("descricao")]
        [StringLength(100)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual DespesaTipoCamara DespesaTipo { get; set; } = null!;
        public virtual ICollection<DespesaCamara> Despesas { get; set; } = new List<DespesaCamara>();
    }
}
