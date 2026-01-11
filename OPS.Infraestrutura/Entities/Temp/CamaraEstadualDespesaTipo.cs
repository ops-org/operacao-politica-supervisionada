using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class CamaraEstadualDespesaTipo
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("descricao")]
        public string? Descricao { get; set; }
    }
}
