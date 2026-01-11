using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("importacao")]
    public class Importacao
    {
        [Key]
        [Column("id")]
        public short Id { get; set; }

        [Column("chave")]
        [StringLength(50)]
        public string Chave { get; set; } = null!;

        [Column("url")]
        [StringLength(255)]
        public string Url { get; set; } = null!;

        [Column("info")]
        [StringLength(255)]
        public string Info { get; set; } = null!;

        [Column("parlamentar_inicio")]
        public DateTime? ParlamentarInicio { get; set; }

        [Column("parlamentar_fim")]
        public DateTime? ParlamentarFim { get; set; }

        [Column("despesas_inicio")]
        public DateTime? DespesasInicio { get; set; }

        [Column("despesas_fim")]
        public DateTime? DespesasFim { get; set; }

        [Column("primeira_despesa")]
        public DateOnly? PrimeiraDespesa { get; set; }

        [Column("ultima_despesa")]
        public DateOnly? UltimaDespesa { get; set; }

        [Column("id_estado")]
        public byte? IdEstado { get; set; }

        // Navigation properties
        public virtual Estado? Estado { get; set; }
    }
}
