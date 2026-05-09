using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class FonteReferencia
{
    public int Id { get; set; }
    public int? IdFonte { get; set; }
    public int? IdColigacaoPartidaria { get; set; }
    public int? IdColigacaoPartidariaComposicao { get; set; }
    public int? IdPartido { get; set; }
    public int? IdPleitoGeral { get; set; }
    public int? IdPleitoRegional { get; set; }
    public int? IdPessoaFisica { get; set; }
    public int? IdCargo { get; set; }
    public int? IdPais { get; set; }
    public int? IdUnidadeFederativa { get; set; }
    public int? IdMunicipio { get; set; }
    public int? IdCandidatura { get; set; }
    public int? IdCandidaturaBem { get; set; }
    public int? IdCandidaturaMotivoCassacao { get; set; }
    public int? IdMotivoCassacao { get; set; }
    public int? IdPleitoGeralCargo { get; set; }
    public int? IdPleitoRegionalCargo { get; set; }
    
    [Column("registro")]
    public string? Registro { get; set; }

    // Navigation properties
    public Fonte? Fonte { get; set; }
    public Candidatura? Candidatura { get; set; }
    public CandidaturaBem? CandidaturaBem { get; set; }
    public CandidaturaMotivoCassacao? CandidaturaMotivoCassacao { get; set; }
    public Cargo? Cargo { get; set; }
    public ColigacaoPartidaria? ColigacaoPartidaria { get; set; }
    public ColigacaoPartidariaPartido? ColigacaoPartidariaComposicao { get; set; }
    public MotivoCassacao? MotivoCassacao { get; set; }
    public Municipio? Municipio { get; set; }
    public Pais? Pais { get; set; }
    public Partido? Partido { get; set; }
    public PessoaFisica? PessoaFisica { get; set; }
    public PleitoGeral? PleitoGeral { get; set; }
    public PleitoGeralCargo? PleitoGeralCargo { get; set; }
    public PleitoRegional? PleitoRegional { get; set; }
    public PleitoRegionalCargo? PleitoRegionalCargo { get; set; }
    public UnidadeFederativa? UnidadeFederativa { get; set; }
}
