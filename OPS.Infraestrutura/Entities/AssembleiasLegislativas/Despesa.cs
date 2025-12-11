using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.Fornecedores;

namespace OPS.Infraestrutura.Entities.AssembleiasLegislativas;

[Table("cl_despesa")]
public class Despesa
{
    [Key]
    [Column("id")]
    public uint Id { get; set; }

    [Column("id_cl_deputado")]
    public uint IdDeputado { get; set; }

    [Column("id_cl_despesa_tipo")]
    public byte? IdDespesaTipo { get; set; }

    [Column("id_fornecedor")]
    public uint? IdFornecedor { get; set; }

    [Column("data_emissao")]
    public DateTime? DataEmissao { get; set; }

    [Column("valor", TypeName = "decimal(10,2)")]
    public decimal? Valor { get; set; }

    [Column("descricao")]
    [StringLength(255)]
    public string? Descricao { get; set; }

    // Navigation properties
    public virtual Deputado Deputado { get; set; } = null!;
    public virtual DespesaTipo? DespesaTipo { get; set; }
    public virtual Fornecedor? Fornecedor { get; set; }
}