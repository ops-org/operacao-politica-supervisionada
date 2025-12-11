using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("pessoa")]
    public class Pessoa
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("nome")]
        [StringLength(255)]
        public string Nome { get; set; } = null!;

        [Column("cpf")]
        [StringLength(20)]
        public string? Cpf { get; set; }

        [Column("cnpj")]
        [StringLength(20)]
        public string? Cnpj { get; set; }

        [Column("data_nascimento")]
        public DateTime? DataNascimento { get; set; }

        [Column("sexo")]
        [StringLength(1)]
        public string? Sexo { get; set; }

        [Column("id_estado")]
        public byte? IdEstado { get; set; }

        [Column("id_municipio")]
        public uint? IdMunicipio { get; set; }

        // Navigation properties
        public virtual Estado? Estado { get; set; }
    }
}
