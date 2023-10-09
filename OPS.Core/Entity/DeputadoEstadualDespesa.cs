using System;
using Dapper;

namespace OPS.Core.Entity
{
    [Table("cl_despesa")]
    public class CamaraEstadualDespesa
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("id_cl_deputado")]
        public int IdDeputado { get; set; }

        [Column("id_cl_despesa_tipo")]
        public int IdDespesaTipo { get; set; }

        [Column("id_cl_despesa_especificacao")]
        public int IdDespesaEspecificacao { get; set; }

        [Column("id_fornecedor")]
        public int IdFornecedor { get; set; }

        [Column("data_emissao")]
        public DateOnly DataEmissao { get; set; }

        [Column("ano_mes")]
        public int AnoMes { get; set; }

        [Column("numero_documento")]
        public string NumeroDocumento { get; set; }

        [Column("valor_liquido")]
        public decimal ValorLiquido { get; set; }

        [Column("favorecido")]
        public string Favorecido { get; set; }

        [Column("observacao")]
        public string Observacao { get; set; }

        [Column("hash")]
        public byte[] Hash { get; set; }
    }
}
