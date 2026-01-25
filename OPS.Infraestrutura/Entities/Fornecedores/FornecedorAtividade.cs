using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("fornecedor_atividade")]
    public class FornecedorAtividade
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("codigo")]
        [StringLength(15)]
        public string Codigo { get; set; } = null!;

        [Column("descricao")]
        [StringLength(255)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<FornecedorInfo> FornecedorInfos { get; set; } = new List<FornecedorInfo>();

        public virtual ICollection<FornecedorAtividadeSecundaria> FornecedorAtividadeSecundarias { get; set; } = new List<FornecedorAtividadeSecundaria>();
    }
}
