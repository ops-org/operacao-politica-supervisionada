using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("fornecedor_atividade_secundaria")]
    [DebuggerDisplay("FornecedorAtividadeSecundaria {{Id}} - {{Nome}}")]
    public class FornecedorAtividadeSecundaria
    {
        [Key]
        [Column("id_fornecedor")]
        public int IdFornecedor { get; set; }

        [Key]
        [Column("id_fornecedor_atividade")]
        public short IdAtividade { get; set; }

        // Navigation properties
        public virtual Fornecedor Fornecedor { get; set; } = null!;
        public virtual FornecedorAtividade FornecedorAtividade { get; set; } = null!;
    }
}

