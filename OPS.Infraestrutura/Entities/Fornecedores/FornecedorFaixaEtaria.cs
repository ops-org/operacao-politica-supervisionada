using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Fornecedores
{
    [Table("fornecedor_faixa_etaria")]
    public class FornecedorFaixaEtaria
    {
        [Key]
        [Column("id")]
        public byte Id { get; set; }

        [Column("descricao")]
        [StringLength(50)]
        public string Descricao { get; set; } = null!;

        [Column("idade_minima")]
        public byte? IdadeMinima { get; set; }

        [Column("idade_maxima")]
        public byte? IdadeMaxima { get; set; }

        // Navigation properties
        public virtual ICollection<FornecedorSocio> FornecedorSocios { get; set; } = new List<FornecedorSocio>();
    }
}
