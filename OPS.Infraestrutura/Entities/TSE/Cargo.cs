using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class Cargo
{
    public int Id { get; set; }
    
    [Column("tse_key")]
    public string? TseKey { get; set; }
    
    [Column("tse_codigo")]
    public string? TseCodigo { get; set; }
    
    [Column("tse_descricao")]
    public string? TseDescricao { get; set; }
}
