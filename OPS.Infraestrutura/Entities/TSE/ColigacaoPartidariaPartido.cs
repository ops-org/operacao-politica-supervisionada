using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class ColigacaoPartidariaPartido
{
    public int Id { get; set; }
    public int IdColigacaoPartidaria { get; set; }
    public int IdPartido { get; set; }

    // Navigation properties
    public ColigacaoPartidaria? ColigacaoPartidaria { get; set; }
    public Partido? Partido { get; set; }
}
