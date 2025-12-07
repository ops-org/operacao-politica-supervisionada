using System;
using System.Text.Json.Serialization;
using Dapper;

namespace OPS.Core.Entity
{
    [Table("cf_deputado_remuneracao")]
    public class DeputadoFederalRemuneracao
    {
        [Key, Required]
        [Column("id_cf_deputado")]
        public uint IdDeputado { get; set; }

        [Key, Required]
        [Column("ano")]
        public short Ano { get; set; }

        [Key, Required]
        [Column("mes")]
        public short Mes { get; set; }

        [Column("valor")]
        public decimal Valor { get; set; }
    }
}
