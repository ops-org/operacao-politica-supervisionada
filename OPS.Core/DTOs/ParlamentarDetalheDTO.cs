using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class ParlamentarDetalheDTO
    {
        [JsonPropertyName("id_parlamentar")]
        public int IdParlamentar { get; set; }

        [JsonPropertyName("id_partido")]
        public int? IdPartido { get; set; }

        [JsonPropertyName("sigla_estado")]
        public string SiglaEstado { get; set; }

        [JsonPropertyName("nome_partido")]
        public string NomePartido { get; set; }

        [JsonPropertyName("id_estado")]
        public int? IdEstado { get; set; }

        [JsonPropertyName("sigla_partido")]
        public string SiglaPartido { get; set; }

        [JsonPropertyName("nome_estado")]
        public string NomeEstado { get; set; }

        [JsonPropertyName("nome_parlamentar")]
        public string NomeParlamentar { get; set; }

        [JsonPropertyName("nome_civil")]
        public string NomeCivil { get; set; }

        [JsonPropertyName("condicao")]
        public string Condicao { get; set; }

        [JsonPropertyName("situacao")]
        public string Situacao { get; set; }

        [JsonPropertyName("sexo")]
        public string Sexo { get; set; }

        [JsonPropertyName("id_gabinete")]
        public int? IdGabinete { get; set; }

        [JsonPropertyName("predio")]
        public string Predio { get; set; }

        [JsonPropertyName("sala")]
        public string Sala { get; set; }

        [JsonPropertyName("andar")]
        public short? Andar { get; set; }

        [JsonPropertyName("telefone")]
        public string Telefone { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("profissao")]
        public string Profissao { get; set; }

        [JsonPropertyName("escolaridade")]
        public string Escolaridade { get; set; }

        [JsonPropertyName("nascimento")]
        public string Nascimento { get; set; }

        [JsonPropertyName("falecimento")]
        public string Falecimento { get; set; }

        [JsonPropertyName("sigla_estado_nascimento")]
        public string SiglaEstadoNascimento { get; set; }

        [JsonPropertyName("nome_municipio_nascimento")]
        public string NomeMunicipioNascimento { get; set; }

        [JsonPropertyName("naturalidade")]
        public string Naturalidade { get; set; }

        [JsonPropertyName("site")]
        public string Site { get; set; }

        [JsonPropertyName("perfil")]
        public string Perfil { get; set; }

        [JsonPropertyName("foto")]
        public string Foto { get; set; }

        [JsonPropertyName("valor_total_ceap")]
        public string ValorTotalCeap { get; set; }

        [JsonPropertyName("valor_total_verbas")]
        public string ValorTotalVerbas { get; set; }

        [JsonPropertyName("secretarios_ativos")]
        public string SecretariosAtivos { get; set; }

        [JsonPropertyName("valor_mensal_secretarios")]
        public string ValorMensalSecretarios { get; set; }

        [JsonPropertyName("valor_total_remuneracao")]
        public string ValorTotalRemuneracao { get; set; }

        [JsonPropertyName("valor_total_salario")]
        public string ValorTotalSalario { get; set; }

        [JsonPropertyName("valor_total_auxilio_moradia")]
        public string ValorTotalAuxilioMoradia { get; set; }

        [JsonPropertyName("valor_total")]
        public string ValorTotal { get; set; }
    }
}
