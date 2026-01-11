using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    [Table("arquivo_cheksum", Schema = "temp")]
    public class ArquivoChecksum
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("nome")]
        public string Nome { get; set; }

        [Column("checksum")]
        public string? Checksum { get; set; }

        [Column("tamanho_bytes")]
        public long? TamanhoBytes { get; set; }

        [Required]
        [Column("criacao")]
        public DateTime Criacao { get; set; }

        [Required]
        [Column("atualizacao")]
        public DateTime Atualizacao { get; set; }

        [Required]
        [Column("verificacao")]
        public DateTime Verificacao { get; set; }

        [Required]
        [Column("valor_total")]
        public decimal ValorTotal { get; set; }

        [Required]
        [Column("divergencia")]
        public decimal Divergencia { get; set; }

        [Required]
        [Column("revisado")]
        public bool Revisado { get; set; }
    }
}
