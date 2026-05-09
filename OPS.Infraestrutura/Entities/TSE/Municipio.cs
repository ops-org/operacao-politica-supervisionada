using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class Municipio
{
    public int Id { get; set; }
    public int? IdUnidadeFederativa { get; set; }
    
    [Column("tse_key")]
    public string? TseKey { get; set; }
    
    [Column("tse_sigla")]
    public string? TseSigla { get; set; }
    
    [Column("tse_nome")]
    public string? TseNome { get; set; }

    // Navigation properties
    public UnidadeFederativa? UnidadeFederativa { get; set; }
}
