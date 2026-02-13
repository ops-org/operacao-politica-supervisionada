using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class PleitoGeralCargo
{
    public int Id { get; set; }
    public int IdPleitoGeral { get; set; }
    public int IdCargo { get; set; }
    
    [Column("tse_quantidade_vagas")]
    public long? TseQuantidadeVagas { get; set; }

    // Navigation properties
    public PleitoGeral? PleitoGeral { get; set; }
    public Cargo? Cargo { get; set; }
}
