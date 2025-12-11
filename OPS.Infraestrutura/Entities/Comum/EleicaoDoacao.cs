using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Comum
{
    [Table("eleicao_doacao")]
    public class EleicaoDoacao
    {
        [Key]
        [Column("id")]
        public uint Id { get; set; }

        [Column("id_eleicao_cargo")]
        public uint? IdEleicaoCargo { get; set; }

        [Column("id_eleicao_candidadto")]
        public uint? IdEleicaoCandidadto { get; set; }

        [Column("ano_eleicao", TypeName = "decimal(4,0)")]
        public decimal? AnoEleicao { get; set; }

        [Column("num_documento")]
        [StringLength(50)]
        public string? NumDocumento { get; set; }

        [Column("cnpj_cpf_doador")]
        [StringLength(14)]
        public string? CnpjCpfDoador { get; set; }

        [Column("raiz_cnpj_cpf_doador")]
        [StringLength(14)]
        public string? RaizCnpjCpfDoador { get; set; }

        [Column("data_receita")]
        public DateTime? DataReceita { get; set; }

        [Column("valor_receita", TypeName = "decimal(10,2)")]
        public decimal? ValorReceita { get; set; }
    }
}
