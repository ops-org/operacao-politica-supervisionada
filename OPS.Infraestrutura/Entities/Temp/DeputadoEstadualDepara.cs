using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    [Table("cl_deputado_de_para", Schema = "temp")]
    public class DeputadoEstadualDepara
    {
        [Key]
        [Required]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("nome")]
        public string Nome { get; set; }

        [Required]
        [Column("id_estado")]
        public short IdEstado { get; set; }
    }
}
