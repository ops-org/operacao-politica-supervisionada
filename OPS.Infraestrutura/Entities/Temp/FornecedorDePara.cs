using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class FornecedorDePara
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("cnpj_incorreto")]
        public string CnpjIncorreto { get; set; } = null!;

        [Column("id_fornecedor_incorreto")]
        public long? IdFornecedorIncorreto { get; set; }

        [Column("nome")]
        public string? NomeFornecedor { get; set; }

        [Column("id_fornecedor_correto")]
        public long? IdFornecedorCorreto { get; set; }

        [Column("cnpj_correto")]
        public string? CnpjCorreto { get; set; }
    }
}
