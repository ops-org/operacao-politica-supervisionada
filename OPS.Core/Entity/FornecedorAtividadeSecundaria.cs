using Dapper;

namespace OPS.Core.Entity
{
    [Table("fornecedor_atividade_secundaria")]
    public class FornecedorAtividadeSecundaria
    {
        [Key, Required]
        [Column("id_fornecedor")]
        public int IdFornecedor { get; set; }

        [Key, Required]
        [Column("id_fornecedor_atividade")]
        public int IdAtividade { get; set; }
    }
}
