using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class SenadoDespesaTemp
    {
        [Column("ano")]
        public decimal? Ano { get; set; }

        [Column("mes")]
        public decimal? Mes { get; set; }

        [Column("senador")]
        public string? Senador { get; set; }

        [Column("tipo_despesa")]
        public string? TipoDespesa { get; set; }

        [Column("cnpj_cpf")]
        public string? CnpjCpf { get; set; }

        [Column("fornecedor")]
        public string? Fornecedor { get; set; }

        [Column("documento")]
        public string? Documento { get; set; }

        [Column("data")]
        public DateTime? Data { get; set; }

        [Column("detalhamento")]
        public string? Detalhamento { get; set; }

        [Column("valor_reembolsado")]
        public decimal? ValorReembolsado { get; set; }

        [Column("cod_documento")]
        public decimal? CodDocumento { get; set; }

        [Column("hash")]
        public byte[]? Hash { get; set; }
    }
}
