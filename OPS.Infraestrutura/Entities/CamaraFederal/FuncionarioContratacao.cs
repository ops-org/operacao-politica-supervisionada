using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_funcionario_contratacao")]
    public class FuncionarioContratacao
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("id_cf_deputado")]
        public int? IdDeputado { get; set; }

        [Column("id_cf_funcionario")]
        public int IdFuncionario { get; set; }

        [Column("id_cf_funcionario_grupo_funcional")]
        public byte? IdFuncionarioGrupoFuncional { get; set; }

        [Column("id_cf_funcionario_cargo")]
        public byte? IdFuncionarioCargo { get; set; }

        [Column("id_cf_funcionario_nivel")]
        public byte? IdFuncionarioNivel { get; set; }

        [Column("id_cf_funcionario_funcao_comissionada")]
        public short? IdFuncionarioFuncaoComissionada { get; set; }

        [Column("id_cf_funcionario_area_atuacao")]
        public byte? IdFuncionarioAreaAtuacao { get; set; }

        [Column("id_cf_funcionario_local_trabalho")]
        public byte? IdFuncionarioLocalTrabalho { get; set; }

        [Column("id_cf_funcionario_situacao")]
        public byte? IdFuncionarioSituacao { get; set; }

        [Column("periodo_de")]
        public DateTime? PeriodoDe { get; set; }

        [Column("periodo_ate")]
        public DateTime? PeriodoAte { get; set; }

        // Navigation properties
        public virtual Deputado? Deputado { get; set; }
        public virtual Funcionario Funcionario { get; set; } = null!;
        public virtual FuncionarioGrupoFuncional? FuncionarioGrupoFuncional { get; set; }
        public virtual FuncionarioCargo? FuncionarioCargo { get; set; }
        public virtual FuncionarioNivel? FuncionarioNivel { get; set; }
        public virtual FuncionarioFuncaoComissionada? FuncionarioFuncaoComissionada { get; set; }
        public virtual FuncionarioAreaAtuacao? FuncionarioAreaAtuacao { get; set; }
        public virtual FuncionarioLocalTrabalho? FuncionarioLocalTrabalho { get; set; }
        public virtual FuncionarioSituacao? FuncionarioSituacao { get; set; }
    }
}
