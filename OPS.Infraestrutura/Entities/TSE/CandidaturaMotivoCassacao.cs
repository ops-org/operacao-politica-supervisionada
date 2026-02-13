using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class CandidaturaMotivoCassacao
{
    public int Id { get; set; }
    public int IdCandidatura { get; set; }
    public int IdMotivoCassacao { get; set; }

    // Navigation properties
    public Candidatura? Candidatura { get; set; }
    public MotivoCassacao? MotivoCassacao { get; set; }
}
