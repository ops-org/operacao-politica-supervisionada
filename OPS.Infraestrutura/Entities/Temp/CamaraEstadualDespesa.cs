using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class CamaraEstadualDespesa
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("id_cl_deputado")]
        public long IdClDeputado { get; set; }

        [Column("id_cl_despesa_tipo")]
        public long? IdClDespesaTipo { get; set; }

        [Column("id_cl_despesa_especificacao")]
        public long? IdClDespesaEspecificacao { get; set; }

        [Column("id_fornecedor")]
        public long? IdFornecedor { get; set; }

        [Column("data_emissao")]
        public DateTime? DataEmissao { get; set; }

        [Column("ano_mes")]
        public int? AnoMes { get; set; }

        [Column("numero_documento")]
        public string? NumeroDocumento { get; set; }

        [Column("valor_liquido")]
        public decimal ValorLiquido { get; set; }

        [Column("favorecido")]
        public string? Favorecido { get; set; }

        [Column("observacao")]
        public string? Observacao { get; set; }

        [Column("hash")]
        public byte[]? Hash { get; set; }
    }
}
