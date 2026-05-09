using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class CandidaturaBem
{
    public int Id { get; set; }
    public int IdCandidatura { get; set; }
    
    [Column("tse_key")]
    public string? TseKey { get; set; }
    
    [Column("tse_ordem")]
    public string? TseOrdem { get; set; }
    
    [Column("tse_tipo_codigo")]
    public string? TseTipoCodigo { get; set; }
    
    [Column("tse_tipo_descricao")]
    public string? TseTipoDescricao { get; set; }
    
    [Column("tse_descricao")]
    public string? TseDescricao { get; set; }
    
    [Column("tse_valor")]
    public decimal? TseValor { get; set; }
    
    [Column("tse_data_hora_ultima_atualizacao")]
    public DateTime? TseDataHoraUltimaAtualizacao { get; set; }

    // Navigation properties
    public Candidatura? Candidatura { get; set; }
}
