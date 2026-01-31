using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_secretario", Schema = "senado")]
    public class SecretarioSenado
    {
        [Column("id")]
        public int? Id { get; set; }

        [Column("id_senador")]
        public int? IdSenador { get; set; }

        [Column("nome")]
        [StringLength(255)]
        public string? Nome { get; set; }

        [Column("id_funcao")]
        public short? IdFuncao { get; set; }

        [Column("id_cargo")]
        public short? IdCargo { get; set; }

        [Column("id_vinculo")]
        public short? IdVinculo { get; set; }

        [Column("id_categoria")]
        public short? IdCategoria { get; set; }

        [Column("id_referencia_cargo")]
        public short? IdReferenciaCargo { get; set; }

        [Column("id_especialidade")]
        public short? IdEspecialidade { get; set; }

        [Column("id_lotacao")]
        public short? IdLotacao { get; set; }

        [Column("admissao")]
        public short? Admissao { get; set; }

        [Column("id_sf_situacao")]
        public short? IdSfSituacao { get; set; }

        // Navigation properties
        public virtual Senador? Senador { get; set; }
        public virtual Funcao? Funcao { get; set; }
        public virtual Cargo? Cargo { get; set; }
        public virtual Vinculo? Vinculo { get; set; }
        public virtual Categoria? Categoria { get; set; }
        public virtual ReferenciaCargo? ReferenciaCargo { get; set; }
        public virtual Cargo? Especialidade { get; set; }
        public virtual Lotacao? Lotacao { get; set; }
        public virtual Situacao? Situacao { get; set; }
    }
}
