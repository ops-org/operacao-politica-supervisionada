using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class PleitoRegionalCargo
{
    public int Id { get; set; }
    public int IdPleitoRegional { get; set; }
    public int IdCargo { get; set; }
    
    [Column("tse_quantidade_vagas")]
    public long? TseQuantidadeVagas { get; set; }

    // Navigation properties
    public PleitoRegional? PleitoRegional { get; set; }
    public Cargo? Cargo { get; set; }
}
