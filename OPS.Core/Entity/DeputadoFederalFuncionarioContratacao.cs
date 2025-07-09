using System;
using Dapper;

namespace OPS.Core.Entity
{

    [Table("cf_funcionario_contratacao")]
    public class DeputadoFederalFuncionarioContratacao
    {

        [Column("id")]
        public int Id { get; set; }

        [Column("id_cf_deputado")]
        public int IdDeputado { get; set; }

        [Column("id_cf_funcionario")]
        public int IdFuncionario { get; set; }

        [Column("id_cf_funcionario_grupo_funcional")]
        public byte? IdGrupoFuncional { get; set; }

        [Column("id_cf_funcionario_cargo")]
        public byte? IdCargo { get; set; }

        [Column("id_cf_funcionario_nivel")]
        public byte? IdNivel { get; set; }

        [Column("id_cf_funcionario_funcao_comissionada")]
        public short? IdFuncaoComissionada { get; set; }

        [Column("id_cf_funcionario_area_atuacao")]
        public byte? IdAreaAtuacao { get; set; }

        [Column("id_cf_funcionario_local_trabalho")]
        public byte? IdLocalTrabalho { get; set; }

        [Column("id_cf_funcionario_situacao")]
        public byte? IdSituacao { get; set; }

        [Column("periodo_de")]
        public DateTime PeriodoDe { get; set; }

        [Column("periodo_ate")]
        public DateTime PeriodoAte { get; set; }
    }
}
