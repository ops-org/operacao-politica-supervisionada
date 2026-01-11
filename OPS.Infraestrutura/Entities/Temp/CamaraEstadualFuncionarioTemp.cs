using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class CamaraEstadualFuncionarioTemp
    {
        [Key]
        [Column("chave")]
        public string Chave { get; set; } = null!;

        [Column("nome")]
        public string? Nome { get; set; }

        [Column("categoria_funcional")]
        public string? CategoriaFuncional { get; set; }

        [Column("cargo")]
        public string? Cargo { get; set; }

        [Column("nivel")]
        public string? Nivel { get; set; }

        [Column("funcao_comissionada")]
        public string? FuncaoComissionada { get; set; }

        [Column("area_atuacao")]
        public string? AreaAtuacao { get; set; }

        [Column("local_trabalho")]
        public string? LocalTrabalho { get; set; }

        [Column("situacao")]
        public string? Situacao { get; set; }

        [Column("data_designacao_funcao")]
        public DateTime? DataDesignacaoFuncao { get; set; }
    }
}
