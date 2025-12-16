using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("fornecedor_atividade_secundaria")]
    public class FornecedorAtividadeSecundaria
    {
        [Key]
        [Column("id_fornecedor")]
        public uint IdFornecedor { get; set; }

        [Key]
        [Column("id_fornecedor_atividade")]
        public uint IdAtividade { get; set; }

        // Navigation properties
        public virtual Fornecedor Fornecedor { get; set; } = null!;
        public virtual FornecedorAtividade FornecedorAtividade { get; set; } = null!;
    }
}
