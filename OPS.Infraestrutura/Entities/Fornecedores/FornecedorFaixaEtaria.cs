using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("fornecedor_faixa_etaria", Schema = "fornecedor")]
    public class FornecedorFaixaEtaria
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("nome")]
        [StringLength(50)]
        public string? Nome { get; set; }

        // Navigation properties
        public virtual ICollection<FornecedorSocio> FornecedorSocios { get; set; } = new List<FornecedorSocio>();
    }
}
