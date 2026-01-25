using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("fornecedor_natureza_juridica")]
    public class FornecedorNaturezaJuridica
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("codigo")]
        [StringLength(10)]
        public string Codigo { get; set; } = null!;

        [Column("descricao")]
        [StringLength(100)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<FornecedorInfo> FornecedorInfos { get; set; } = new List<FornecedorInfo>();
    }
}
