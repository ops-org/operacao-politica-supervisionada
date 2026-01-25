using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Importador.Fornecedores.MinhaReceita
{
    [Table("fornecedor_atividade_secundaria")]
    public class FornecedorAtividadeSecundaria
    {
        //[Key, Required]
        [Column("id_fornecedor")]
        public int IdFornecedor { get; set; }

        //[Key, Required]
        [Column("id_fornecedor_atividade")]
        public short IdAtividade { get; set; }
    }
}
