using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class PleitoGeral
{
    public int Id { get; set; }
    
    [Column("tse_key")]
    public string? TseKey { get; set; }
    
    [Column("tse_codigo")]
    public string? TseCodigo { get; set; }
    
    [Column("tse_descricao")]
    public string? TseDescricao { get; set; }
    
    [Column("tse_turno")]
    public string? TseTurno { get; set; }
    
    [Column("tse_data_hora")]
    public DateTime? TseDataHora { get; set; }
    
    [Column("tse_tipo_codigo")]
    public string? TseTipoCodigo { get; set; }
    
    [Column("tse_tipo_descricao")]
    public string? TseTipoDescricao { get; set; }
}
