using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class Partido
{
    public int Id { get; set; }
    
    [Column("tse_key")]
    public string? TseKey { get; set; }
    
    [Column("tse_numero")]
    public string? TseNumero { get; set; }
    
    [Column("tse_sigla")]
    public string? TseSigla { get; set; }
    
    [Column("tse_nome")]
    public string? TseNome { get; set; }
}
