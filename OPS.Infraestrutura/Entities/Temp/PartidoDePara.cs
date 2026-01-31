using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace OPS.Infraestrutura.Entities.Temp
{
    [DebuggerDisplay("Id = {Id}, SiglaNome = {SiglaNome}")]
    [Keyless]
    [Table("partido_de_para")]
    public class PartidoDePara
    {
        [Column("id_partido")]
        public byte Id { get; set; }

        [Column("nome")]
        [StringLength(100)]
        public string? SiglaNome { get; set; }
    }
}
