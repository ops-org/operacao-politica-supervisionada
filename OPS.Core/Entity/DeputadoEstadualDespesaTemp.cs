﻿using System;
using System.Text.Json.Serialization;
using Dapper;

namespace OPS.Core.Entity
{
    [Table("cl_despesa_temp", Schema = "ops_tmp")]
    public class CamaraEstadualDespesaTemp
    {
        [JsonIgnore]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_cl_deputado")]
        public uint? IdDeputado { get; set; }

        [Column("nome")]
        public string Nome { get; set; }

        [Column("nome_civil")]
        public string NomeCivil { get; set; }

        [Column("cpf")]
        public string Cpf { get; set; }

        [Column("empresa")]
        public string Empresa { get; set; }

        [Column("cnpj_cpf")]
        public string CnpjCpf { get; set; }

        [Column("data_emissao")]
        public DateTime DataEmissao { get; set; }

        [Column("tipo_verba")]
        public string TipoVerba { get; set; }

        [Column("despesa_tipo")]
        public string TipoDespesa { get; set; }

        [Column("documento")]
        public string Documento { get; set; }

        [Column("observacao")]
        public string Observacao { get; set; }

        [Column("valor")]
        public decimal Valor { get; set; }

        [Column("ano")]
        public short Ano { get; set; }

        [Column("mes")]
        public short? Mes { get; set; }

        [Column("favorecido")]
        public string Favorecido { get; set; }

        [JsonIgnore]
        [Column("hash")]
        public byte[] Hash { get; set; }
    }
}
