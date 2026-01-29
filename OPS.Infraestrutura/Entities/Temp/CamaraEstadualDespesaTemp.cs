using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace OPS.Infraestrutura.Entities.Temp
{
    [DebuggerDisplay("Id={Id}, Cpf={Cpf}, IdDeputado={IdDeputado}, Nome={Nome}, NomeCivil={NomeCivil}, Empresa={Empresa}, CnpjCpf={CnpjCpf}, DataEmissao={DataEmissao}, TipoVerba={TipoVerba}, TipoDespesa={TipoDespesa}, Valor={Valor}, Ano={Ano}, Mes={Mes}, Documento={Documento}, Favorecido={Favorecido}, Observacao={Observacao}")]
    public class CamaraEstadualDespesaTemp
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("cpf")]
        public string? Cpf { get; set; }

        [Column("id_cl_deputado")]
        public long? IdDeputado { get; set; }

        [Column("nome")]
        public string? Nome { get; set; }

        [Column("nome_civil")]
        public string? NomeCivil { get; set; }

        //[NotMapped, JsonIgnore]
        //public string? Partido { get; set; }

        [Column("id_fornecedor")]
        public int? IdFornecedor { get; set; }

        [Column("fornecedor")]
        public string? NomeFornecedor { get; set; }

        [Column("cnpj_cpf")]
        public string? CnpjCpf { get; set; }

        [Column("data_emissao")]
        public DateOnly? DataEmissao { get; set; }

        [Column("tipo_verba")]
        public string? TipoVerba { get; set; }

        [Column("despesa_tipo")]
        public string? TipoDespesa { get; set; }

        [Required]
        [Column("valor")]
        public decimal Valor { get; set; }

        [Required]
        [Column("ano")]
        public int Ano { get; set; }

        [Column("mes")]
        public int? Mes { get; set; }

        [NotMapped, JsonIgnore]
        public DateOnly DataVigencia
        {
            get { return new DateOnly(Ano, Mes ?? DataEmissao!.Value.Month, 1); }
            set { Ano = value.Year; Mes = value.Month; }
        }

        [Column("documento")]
        public string? Documento { get; set; }

        [Column("favorecido")]
        public string? Favorecido { get; set; }

        [Column("observacao")]
        public string? Observacao { get; set; }

        [Column("hash"), JsonIgnore]
        public byte[]? Hash { get; set; }

        /// <summary>
        /// Origem dos dados (Ex.: arquivo ou site especifico)
        /// </summary>
        [NotMapped, JsonIgnore]
        public string Origem { get; set; }

        [NotMapped, JsonIgnore]
        public DateOnly DataColeta { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    }
}
