using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.Fornecedores;

namespace OPS.Infraestrutura.Entities.SenadoFederal
{
    [Table("sf_despesa", Schema = "senado")]
    public class DespesaSenado
    {
        [Column("id")]
        public long Id { get; set; }

        [Column("id_sf_senador")]
        public int IdSenador { get; set; }

        [Column("id_sf_despesa_tipo")]
        public short? IdDespesaTipo { get; set; }

        [Column("id_fornecedor")]
        public int? IdFornecedor { get; set; }

        [Column("ano_mes")]
        public int? AnoMes { get; set; }

        [Column("ano")]
        public short? Ano { get; set; }

        [Column("mes")]
        public short? Mes { get; set; }

        [Column("documento")]
        [StringLength(50)]
        public string? Documento { get; set; }

        [Column("data_emissao")]
        public DateOnly? DataEmissao { get; set; }

        [Column("detalhamento")]
        public string? Detalhamento { get; set; }

        [Column("valor", TypeName = "decimal(10,2)")]
        public decimal? Valor { get; set; }

        [Column("hash")]
        public byte[]? Hash { get; set; }

        // Navigation properties
        public virtual Senador Senador { get; set; } = null!;
        public virtual DespesaTipoSenado? DespesaTipo { get; set; }
        public virtual Fornecedor? Fornecedor { get; set; }
    }
}
