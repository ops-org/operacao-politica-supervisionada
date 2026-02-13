using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class UnidadeFederativa
{
    public int Id { get; set; }
    public int? IdPais { get; set; }
    
    [Column("tse_key")]
    public string? TseKey { get; set; }
    
    [Column("tse_sigla")]
    public string? TseSigla { get; set; }
    
    [Column("tse_nome")]
    public string? TseNome { get; set; }

    // Navigation properties
    public Pais? Pais { get; set; }
}
