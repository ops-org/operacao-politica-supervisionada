using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("forcecedor_cnpj_incorreto")]
    public class ForcecedorCnpjIncorreto
    {
        [Key]
        [Column("id_fornecedor")]
        public uint IdFornecedor { get; set; }

        [Column("cnpj_incorreto")]
        [StringLength(20)]
        public string CnpjIncorreto { get; set; } = null!;

        [Column("cnpj_correto")]
        [StringLength(20)]
        public string? CnpjCorreto { get; set; }

        // Navigation properties
        public virtual Fornecedor Fornecedor { get; set; } = null!;
    }
}
