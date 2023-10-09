﻿using Dapper;

namespace OPS.Core.Entity
{
    [Table("fornecedor_natureza_juridica")]
    public class NaturezaJuridica
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; }

        [Column("descricao")]
        public string Descricao { get; set; }
    }
}
