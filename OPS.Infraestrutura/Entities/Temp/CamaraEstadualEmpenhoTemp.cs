using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class CamaraEstadualEmpenhoTemp
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("nome_favorecido")]
        public string NomeFavorecido { get; set; } = null!;

        [Required]
        [Column("id_cl_deputado")]
        public long IdClDeputado { get; set; }

        [Required]
        [Column("nome_deputado")]
        public string NomeDeputado { get; set; } = null!;

        [Required]
        [Column("cnpj_cpf")]
        public string CnpjCpf { get; set; } = null!;

        [Required]
        [Column("numero_empenho")]
        public string NumeroEmpenho { get; set; } = null!;

        [Required]
        [Column("data")]
        public DateTime Data { get; set; }

        [Required]
        [Column("competencia")]
        public DateTime Competencia { get; set; }

        [Required]
        [Column("valor_empenhado")]
        public decimal ValorEmpenhado { get; set; }

        [Required]
        [Column("valor_pago")]
        public decimal ValorPago { get; set; }
    }
}
