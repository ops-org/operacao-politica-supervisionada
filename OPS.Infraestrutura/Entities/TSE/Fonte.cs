using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class Fonte
{
    public int Id { get; set; }
    
    [Column("sigla")]
    public string? Sigla { get; set; }
    
    [Column("descricao")]
    public string? Descricao { get; set; }
    
    [Column("url")]
    public string? Url { get; set; }
    
    [Column("repositorio_url")]
    public string? RepositorioUrl { get; set; }
    
    [Column("obtencao_data_hora")]
    public DateTime? ObtencaoDataHora { get; set; }
    
    [Column("importacao_data_hora")]
    public DateTime? ImportacaoDataHora { get; set; }
}
