using System;
using Dapper;

namespace OPS.Core.Entity
{
    [Table("cf_deputado_funcionario_temp", Schema = "ops_tmp")]
    public class DeputadoFederalFuncionarioTemp
    {
        [Key, Required]
        [Column("id_cf_deputado")]
        public int IdDeputado { get; set; }

        [Key, Required]
        [Column("chave")]
        public string Chave { get; set; }

        [Column("nome")]
        public string Nome { get; set; }

        [Column("grupo_funcional")]
        public string GrupoFuncional { get; set; }

        [Column("nivel")]
        public string Nivel { get; set; }

        [Column("periodo_de")]
        public DateTime PeriodoDe { get; set; }

        [Column("periodo_ate")]
        public DateTime? PeriodoAte { get; set; }
    }
}
