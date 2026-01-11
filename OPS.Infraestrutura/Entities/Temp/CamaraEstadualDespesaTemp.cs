using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class CamaraEstadualDespesaTemp
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("id_cl_deputado")]
        public long? IdClDeputado { get; set; }

        [Column("nome")]
        public string? Nome { get; set; }

        [Column("nome_civil")]
        public string? NomeCivil { get; set; }

        [Column("cpf")]
        public string? Cpf { get; set; }

        [Column("empresa")]
        public string? Empresa { get; set; }

        [Column("cnpj_cpf")]
        public string? CnpjCpf { get; set; }

        [Column("data_emissao")]
        public DateTime? DataEmissao { get; set; }

        [Column("tipo_verba")]
        public string? TipoVerba { get; set; }

        [Column("despesa_tipo")]
        public string? TipoDespesa { get; set; }

        [Column("documento")]
        public string? Documento { get; set; }

        [Column("observacao")]
        public string? Observacao { get; set; }

        [Required]
        [Column("valor")]
        public decimal Valor { get; set; }

        [Required]
        [Column("ano")]
        public int Ano { get; set; }

        [Column("mes")]
        public int? Mes { get; set; }

        [Column("favorecido")]
        public string? Favorecido { get; set; }

        [Column("hash")]
        public byte[]? Hash { get; set; }

        [NotMapped]
        public int Lote { get; set; }
    }
}
