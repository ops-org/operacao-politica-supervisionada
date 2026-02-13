using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class Candidatura
{
    public int Id { get; set; }
    public int? IdPleitoGeral { get; set; }
    public int? IdPleitoRegional { get; set; }
    public int? IdPessoaFisica { get; set; }
    public int? IdCargo { get; set; }
    public int? IdPais { get; set; }
    public int? IdUnidadeFederativa { get; set; }
    public int? IdMunicipio { get; set; }
    public int? IdPartido { get; set; }
    public int? IdColigacaoPartidaria { get; set; }
    
    [Column("tse_key")]
    public string? TseKey { get; set; }
    
    [Column("tse_candidato_sequencial")]
    public string? TseCandidatoSequencial { get; set; }
    
    [Column("tse_candidato_numero")]
    public string? TseCandidatoNumero { get; set; }
    
    [Column("tse_candidato_nome_urna")]
    public string? TseCandidatoNomeUrna { get; set; }
    
    [Column("tse_candidatura_situacao_codigo")]
    public string? TseCandidaturaSituacaoCodigo { get; set; }
    
    [Column("tse_candidatura_situacao_descricao")]
    public string? TseCandidaturaSituacaoDescricao { get; set; }
    
    [Column("tse_candidatura_protocolo")]
    public string? TseCandidaturaProtocolo { get; set; }
    
    [Column("tse_processo_numero")]
    public string? TseProcessoNumero { get; set; }
    
    [Column("tse_ocupacao_codigo")]
    public string? TseOcupacaoCodigo { get; set; }
    
    [Column("tse_ocupacao_descricao")]
    public string? TseOcupacaoDescricao { get; set; }
    
    [Column("tse_genero_codigo")]
    public string? TseGeneroCodigo { get; set; }
    
    [Column("tse_genero_descricao")]
    public string? TseGeneroDescricao { get; set; }
    
    [Column("tse_grau_instrucao_codigo")]
    public string? TseGrauInstrucaoCodigo { get; set; }
    
    [Column("tse_grau_instrucao_descricao")]
    public string? TseGrauInstrucaoDescricao { get; set; }
    
    [Column("tse_estado_civil_codigo")]
    public string? TseEstadoCivilCodigo { get; set; }
    
    [Column("tse_estado_civil_descricao")]
    public string? TseEstadoCivilDescricao { get; set; }
    
    [Column("tse_cor_raca_codigo")]
    public string? TseCorRacaCodigo { get; set; }
    
    [Column("tse_cor_raca_descricao")]
    public string? TseCorRacaDescricao { get; set; }
    
    [Column("tse_nacionalidade_codigo")]
    public string? TseNacionalidadeCodigo { get; set; }
    
    [Column("tse_nacionalidade_descricao")]
    public string? TseNacionalidadeDescricao { get; set; }
    
    [Column("tse_situacao_turno_codigo")]
    public string? TseSituacaoTurnoCodigo { get; set; }
    
    [Column("tse_situacao_turno_descricao")]
    public string? TseSituacaoTurnoDescricao { get; set; }
    
    [Column("tse_reeleicao")]
    public bool? TseReeleicao { get; set; }
    
    [Column("tse_bens_declarar")]
    public bool? TseBensDeclarar { get; set; }
    
    [Column("tse_email")]
    public string? TseEmail { get; set; }

    // Navigation properties
    public Cargo? Cargo { get; set; }
    public ColigacaoPartidaria? ColigacaoPartidaria { get; set; }
    public Municipio? Municipio { get; set; }
    public Pais? Pais { get; set; }
    public Partido? Partido { get; set; }
    public PessoaFisica? PessoaFisica { get; set; }
    public PleitoGeral? PleitoGeral { get; set; }
    public PleitoRegional? PleitoRegional { get; set; }
    public UnidadeFederativa? UnidadeFederativa { get; set; }
}
