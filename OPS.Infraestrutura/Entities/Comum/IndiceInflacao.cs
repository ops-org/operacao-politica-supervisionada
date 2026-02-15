using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("indice_inflacao")]
    public class IndiceInflacao
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("ano")]
        public short Ano { get; set; }

        [Column("mes")]
        public short Mes { get; set; }

        /// <summary>
        /// Variação mensal do índice (ex: 0.50 para 0.50%)
        /// </summary>
        [Column("valor")]
        public decimal Valor { get; set; }

        /// <summary>
        /// Número índice acumulado
        /// </summary>
        [Column("indice")]
        public decimal Indice { get; set; }
    }
}
