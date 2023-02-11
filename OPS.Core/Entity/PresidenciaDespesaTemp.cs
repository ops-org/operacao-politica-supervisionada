using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Core.Entity
{
    [Table("pr_despesa_temp", Schema = "ops_tmp")]
    public class PresidenciaDespesaTemp
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("cpf")]
        public string Cpf { get; set; }

        [Column("empresa")]
        public string Empresa { get; set; }

        [Column("cnpj_cpf")]
        public string CnpjCpf { get; set; }

        [Column("data_pgto")]
        public DateTime DataPagamento { get; set; }

        [Column("tipo_verba")]
        public string TipoVerba { get; set; }

        [Column("despesa_tipo")]
        public string TipoDespesa { get; set; }

        [Column("documento")]
        public string Documento { get; set; }

        [Column("valor")]
        public decimal Valor { get; set; }

    }
}

