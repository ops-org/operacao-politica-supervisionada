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

        /// <summary>
        /// Controle de alteração do arquivo baseado no checksum MD5 do arquivo.
        /// </summary>
        [Column("checksum")]
        public string? Checksum { get; set; }

        [Column("tamanho_bytes")]
        public long? TamanhoBytes { get; set; }

        /// <summary>
        /// Data/Hora que o arquivo foi criado.
        /// </summary>
        [Required]
        [Column("criacao")]
        public DateTime Criacao { get; set; }

        /// <summary>
        /// Data/Hora que o arquivo foi atualizado com sucesso pela última vez.
        /// </summary>
        [Required]
        [Column("atualizacao")]
        public DateTime Atualizacao { get; set; }

        /// <summary>
        /// Data/Hora que o arquivo foi verificado com sucesso pela última vez.
        /// Caso o arquivo tenha sido verificado, mas sem alterações identificadas, o arquivo fisico e a data de atualização não devem ser alterados.
        /// </summary>
        [Required]
        [Column("verificacao")]
        public DateTime Verificacao { get; set; }

        /// <summary>
        /// Valor total lido no arquivo. TODO: alterar para valor calculado.
        /// </summary>
        [Required]
        [Column("valor_total")]
        public decimal ValorTotal { get; set; }

        /// <summary>
        /// Divergência entre o valor total lido no arquivo e o valor total calculado a partir dos registros do arquivo.
        /// </summary>
        [Required]
        [Column("divergencia")]
        public decimal Divergencia { get; set; }

        /// <summary>
        /// Quando divergencia for diferente de zero, indica se a divergência já foi revisada manualmente.
        /// </summary>
        [Required]
        [Column("revisado")]
        public bool Revisado { get; set; }
    }
}
