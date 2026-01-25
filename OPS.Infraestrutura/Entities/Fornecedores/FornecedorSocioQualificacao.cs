using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("fornecedor_socio_qualificacao", Schema = "fornecedor")]
    public class FornecedorSocioQualificacao
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("descricao")]
        [StringLength(100)]
        public string Descricao { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<FornecedorSocio> FornecedorSocios { get; set; } = new List<FornecedorSocio>();
        public virtual ICollection<FornecedorSocio> FornecedorSocioRepresentantes { get; set; } = new List<FornecedorSocio>();
    }
}
