using System;
using Dapper;

namespace OPS.Core.Entity
{
    [Table("importacao")]
    public class Importacao
    {
        [Key]
        [Column("id")]
        public ushort Id { get; set; }

        [Column("chave")]
        public string Chave { get; set; }

        [Column("url")]
        public string Url { get; set; }

        [Column("info")]
        public string Info { get; set; }

        [Column("parlamentar_inicio")]
        public DateTime? ParlamentarInicio { get; set; }

        [Column("parlamentar_fim")]
        public DateTime? ParlamentarFim { get; set; }

        [Column("despesas_inicio")]
        public DateTime? DespesasInicio { get; set; }

        [Column("despesas_fim")]
        public DateTime? DespesasFim { get; set; }

        [Column("primeira_despesa")]
        public DateTime? PrimeiraDespesa { get; set; }

        [Column("ultima_despesa")]
        public DateTime? UltimaDespesa { get; set; }
    }
}
