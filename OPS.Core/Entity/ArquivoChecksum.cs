using System;
using Dapper;

namespace OPS.Core.Entity
{
    [Table("arquivo_cheksum", Schema = "ops_tmp")]
    public class ArquivoChecksum
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("nome")]
        public string Nome { get; set; }

        [Column("checksum")]
        public string Checksum { get; set; }

        [Column("tamanho_bytes")]
        public uint TamanhoBytes { get; set; }

        [Column("criacao")]
        public DateTime Criacao { get; set; }

        [Column("atualizacao")]
        public DateTime Atualizacao { get; set; }

        [Column("verificacao")]
        public DateTime Verificacao { get; set; }
    }
}
