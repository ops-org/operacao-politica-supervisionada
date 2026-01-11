using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("fornecedor_socio_qualificacao")]
    public class FornecedorSocioQualificacao
    {
        [Key]
        [Column("id")]
        public byte Id { get; set; }

        [Column("descricao")]
        [StringLength(100)]
        public string Descricao { get; set; } = null!;

        // Navigation properties - removed to prevent shadow property creation
        // public virtual ICollection<FornecedorSocio> FornecedorSocios { get; set; } = new List<FornecedorSocio>();
    }
}
