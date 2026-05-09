using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class Pais
{
    public int Id { get; set; }
    
    [Column("tse_codigo")]
    public string? TseCodigo { get; set; }
    
    [Column("tse_sigla")]
    public string? TseSigla { get; set; }
    
    [Column("tse_nome")]
    public string? TseNome { get; set; }
}
