using System;
using System.Text.Json.Serialization;
using Dapper;

namespace OPS.Core.Entity
{
    [Table("cl_empenho_temp", Schema = "ops_tmp")]
    public class DeputadoEstadualEmpenhoTemp
    {
        [JsonIgnore]
        [Column("id")]
        public int Id { get; set; }

        [Column("nome_favorecido")]
        public string NomeFavorecido { get; set; }

        [Column("cnpj_cpf")]
        public string CNPJCPF { get; set; }

        //[Column("objeto")]
        //public string Objeto { get; set; }

        //[Column("tipo_licitacao")]
        //public string TipoLicitacao { get; set; }

        [Column("numero_empenho")]
        public string NumeroEmpenho { get; set; }

        [Column("data")]
        public DateOnly Data { get; set; }

        [Column("competencia")]
        public DateOnly Competencia { get; set; }

        [Column("valor_empenhado")]
        public decimal ValorEmpenhado { get; set; }

        [Column("valor_pago")]
        public decimal ValorPago { get; set; }

    }
}
