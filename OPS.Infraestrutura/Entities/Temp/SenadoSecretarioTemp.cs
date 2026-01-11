using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class SenadoSecretarioTemp
    {
        [Column("id")]
        public long? Id { get; set; }

        [Column("id_senador")]
        public long? IdSenador { get; set; }

        [Column("nome")]
        public string? Nome { get; set; }

        [Column("funcao")]
        public string? Funcao { get; set; }

        [Column("nome_funcao")]
        public string? NomeFuncao { get; set; }

        [Column("vinculo")]
        public string? Vinculo { get; set; }

        [Column("situacao")]
        public string? Situacao { get; set; }

        [Column("admissao")]
        public int? Admissao { get; set; }

        [Column("cargo")]
        public string? Cargo { get; set; }

        [Column("padrao")]
        public string? Padrao { get; set; }

        [Column("especialidade")]
        public string? Especialidade { get; set; }

        [Column("lotacao")]
        public string? Lotacao { get; set; }
    }
}
