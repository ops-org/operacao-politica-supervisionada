using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OPS.Infraestrutura.Entities.Fornecedores;

namespace OPS.Infraestrutura.Entities.AssembleiasLegislativas;

[Table("cl_despesa")]
public class Despesa
{
    [Key]
    [Column("id")]
    public ulong Id { get; set; }

    [Column("id_cl_deputado")]
    public uint IdDeputado { get; set; }

    [Column("id_cl_despesa_tipo")]
    public byte? IdDespesaTipo { get; set; }

    [Column("id_cl_despesa_especificacao")]
    public uint? IdDespesaEspecificacao { get; set; }

    [Column("id_fornecedor")]
    public uint? IdFornecedor { get; set; }

    [Column("data_emissao")]
    public DateTime? DataEmissao { get; set; }

    [Column("ano_mes")]
    public int? AnoMes { get; set; }

    [Column("numero_documento")]
    [StringLength(50)]
    public string? NumeroDocumento { get; set; }

    [Column("valor_liquido", TypeName = "decimal(10,2)")]
    public decimal ValorLiquido { get; set; }

    [Column("favorecido")]
    [StringLength(200)]
    public string? Favorecido { get; set; }

    [Column("observacao")]
    [StringLength(8000)]
    public string? Observacao { get; set; }

    [Column("hash")]
    public byte[]? Hash { get; set; }

    // Navigation properties
    public virtual Deputado Deputado { get; set; } = null!;
    public virtual DespesaTipo? DespesaTipo { get; set; }
    public virtual DespesaEspecificacao? DespesaEspecificacao { get; set; }
    public virtual Fornecedor? Fornecedor { get; set; }
}