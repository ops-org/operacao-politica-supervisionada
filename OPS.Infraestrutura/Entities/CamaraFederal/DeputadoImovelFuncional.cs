using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_deputado_imovel_funcional")]
    public class DeputadoImovelFuncional
    {
        [Key]
        [Column("id_cf_deputado")]
        public uint IdDeputado { get; set; }

        [Column("endereco")]
        [StringLength(255)]
        public string? Endereco { get; set; }

        [Column("bairro")]
        [StringLength(100)]
        public string? Bairro { get; set; }

        [Column("cidade")]
        [StringLength(100)]
        public string? Cidade { get; set; }

        [Column("estado")]
        [StringLength(2)]
        public string? Estado { get; set; }

        [Column("cep")]
        [StringLength(10)]
        public string? Cep { get; set; }

        // Navigation properties
        public virtual Deputado Deputado { get; set; } = null!;
    }
}
