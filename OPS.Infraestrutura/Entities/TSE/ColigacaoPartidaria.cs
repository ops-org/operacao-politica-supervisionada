using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class ColigacaoPartidaria
{
    public int Id { get; set; }
    
    [Column("tse_key")]
    public string? TseKey { get; set; }
    
    [Column("tse_sequencial_coligacao")]
    public string? TseSequencialColigacao { get; set; }
    
    [Column("tse_nome")]
    public string? TseNome { get; set; }
    
    [Column("tse_tipo_descricao")]
    public string? TseTipoDescricao { get; set; }
}
