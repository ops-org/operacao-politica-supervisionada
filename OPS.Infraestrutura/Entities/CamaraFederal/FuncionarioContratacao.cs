using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace OPS.Infraestrutura.Entities.CamaraFederal
{
    [Table("cf_funcionario_contratacao")]
    [DebuggerDisplay("FuncionarioContratacao {Id} - Funcionario:{IdFuncionario} Deputado:{IdDeputado}")]
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
        public short? IdGrupoFuncional { get; set; }

        [Column("id_cf_funcionario_cargo")]
        public short? IdCargo { get; set; }

        [Column("id_cf_funcionario_nivel")]
        public short? IdNivel { get; set; }

        [Column("id_cf_funcionario_funcao_comissionada")]
        public short? IdFuncaoComissionada { get; set; }

        [Column("id_cf_funcionario_area_atuacao")]
        public short? IdAreaAtuacao { get; set; }

        [Column("id_cf_funcionario_local_trabalho")]
        public short? IdLocalTrabalho { get; set; }

        [Column("id_cf_funcionario_situacao")]
        public short? IdSituacao { get; set; }

        [Column("periodo_de")]
        public DateOnly? PeriodoDe { get; set; }

        [Column("periodo_ate")]
        public DateOnly? PeriodoAte { get; set; }

        // Navigation properties
        [ForeignKey("IdDeputado")]
        public virtual Deputado? Deputado { get; set; }

        [ForeignKey("IdFuncionario")]
        public virtual Funcionario Funcionario { get; set; } = null!;

        [ForeignKey("IdGrupoFuncional")]
        public virtual FuncionarioGrupoFuncional? FuncionarioGrupoFuncional { get; set; }

        [ForeignKey("IdCargo")]
        public virtual FuncionarioCargo? FuncionarioCargo { get; set; }

        [ForeignKey("IdNivel")]
        public virtual FuncionarioNivel? FuncionarioNivel { get; set; }

        [ForeignKey("IdFuncaoComissionada")]
        public virtual FuncionarioFuncaoComissionada? FuncionarioFuncaoComissionada { get; set; }

        [ForeignKey("IdAreaAtuacao")]
        public virtual FuncionarioAreaAtuacao? FuncionarioAreaAtuacao { get; set; }

        [ForeignKey("IdLocalTrabalho")]
        public virtual FuncionarioLocalTrabalho? FuncionarioLocalTrabalho { get; set; }

        [ForeignKey("IdSituacao")]
        public virtual FuncionarioSituacao? FuncionarioSituacao { get; set; }
    }
}
