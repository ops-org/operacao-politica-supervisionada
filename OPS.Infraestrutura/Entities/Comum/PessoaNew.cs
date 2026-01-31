using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("pessoa_new")]
    public class PessoaNew
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("cpf")]
        [StringLength(15)]
        public string Cpf { get; set; } = null!;

        [Column("cpf_parcial")]
        [StringLength(6)]
        public string? CpfParcial { get; set; }

        [Column("nome")]
        [StringLength(100)]
        public string? Nome { get; set; }

        [Column("data_nascimento")]
        public DateOnly? DataNascimento { get; set; }

        [Column("id_nacionalidade")]
        public short? IdNacionalidade { get; set; }

        [Column("id_estado_nascimento")]
        public short? IdEstadoNascimento { get; set; }

        [Column("municipio_nascimento")]
        [StringLength(100)]
        public string? MunicipioNascimento { get; set; }

        [Column("id_genero")]
        public short? IdGenero { get; set; }

        [Column("id_etnia")]
        public short? IdEtnia { get; set; }

        [Column("id_estado_civil")]
        public short? IdEstadoCivil { get; set; }

        [Column("id_grau_instrucao")]
        public short? IdGrauInstrucao { get; set; }

        [Column("id_ocupacao")]
        public int? IdOcupacao { get; set; }

        // Navigation properties
        public virtual Estado? EstadoNascimento { get; set; }
        public virtual Estado? Nacionalidade { get; set; }
        public virtual Estado? Genero { get; set; }
        public virtual Estado? Etnia { get; set; }
        public virtual Estado? EstadoCivil { get; set; }
        public virtual Estado? GrauInstrucao { get; set; }
        public virtual Profissao? Ocupacao { get; set; }
    }
}
