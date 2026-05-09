using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.TSE;

public class PessoaFisica
{
    public int Id { get; set; }
    public int? IdPaisNascimento { get; set; }
    public int? IdUnidadeFederativaNascimento { get; set; }
    public int? IdMunicipioNascimento { get; set; }
    
    [Column("tse_key")]
    public string? TseKey { get; set; }
    
    [Column("tse_cpf")]
    public string? TseCpf { get; set; }
    
    [Column("tse_nome")]
    public string? TseNome { get; set; }
    
    [Column("tse_nome_social")]
    public string? TseNomeSocial { get; set; }
    
    [Column("tse_data_hora_nascimento")]
    public DateTime? TseDataHoraNascimento { get; set; }
    
    [Column("tse_numero_titulo_eleitoral")]
    public string? TseNumeroTituloEleitoral { get; set; }

    // Navigation properties
    public Pais? PaisNascimento { get; set; }
    public UnidadeFederativa? UnidadeFederativaNascimento { get; set; }
    public Municipio? MunicipioNascimento { get; set; }
}
