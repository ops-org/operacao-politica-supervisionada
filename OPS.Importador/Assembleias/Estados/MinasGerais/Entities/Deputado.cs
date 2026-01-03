using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.MinasGerais.Entities
{
    public class Deputado
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("partido")]
        public string Partido { get; set; }

        [JsonPropertyName("tagLocalizacao")]
        public int TagLocalizacao { get; set; }

        [JsonPropertyName("sexo")]
        public string Sexo { get; set; }

        [JsonPropertyName("situacao")]
        public string Situacao { get; set; }

        [JsonPropertyName("inicioSituacao")]
        public DateTime InicioSituacao { get; set; }

        [JsonPropertyName("tipoMandato")]
        public string TipoMandato { get; set; }

        [JsonPropertyName("nomeServidor")]
        public string NomeServidor { get; set; }

        [JsonPropertyName("naturalidadeMunicipio")]
        public string NaturalidadeMunicipio { get; set; }

        [JsonPropertyName("naturalidadeUf")]
        public string NaturalidadeUf { get; set; }

        [JsonPropertyName("dataNascimento")]
        public string DataNascimento { get; set; }

        [JsonPropertyName("apresentarEmail")]
        public string ApresentarEmail { get; set; }

        [JsonPropertyName("atividadeProfissional")]
        public string AtividadeProfissional { get; set; }

        [JsonPropertyName("codigoSituacao")]
        public int CodigoSituacao { get; set; }

        [JsonPropertyName("emails")]
        public List<Email> Emails { get; set; }

        [JsonPropertyName("redesSociais")]
        public List<RedesSociai> RedesSociais { get; set; }

        [JsonPropertyName("vidaProfissionalPolitica")]
        public string VidaProfissionalPolitica { get; set; }

        [JsonPropertyName("atuacaoParlamentar")]
        public string AtuacaoParlamentar { get; set; }

        [JsonPropertyName("condecoracao")]
        public string Condecoracao { get; set; }

        [JsonPropertyName("candidaturas")]
        public List<Candidatura> Candidaturas { get; set; }

        [JsonPropertyName("filiacoes")]
        public List<Filiaco> Filiacoes { get; set; }

        [JsonPropertyName("enderecos")]
        public List<Endereco> Enderecos { get; set; }

        [JsonPropertyName("legislaturas")]
        public List<Legislatura> Legislaturas { get; set; }

        [JsonPropertyName("situacaoLegislaturas")]
        public string SituacaoLegislaturas { get; set; }

        [JsonPropertyName("partidosLegislatura")]
        public string PartidosLegislatura { get; set; }

        [JsonPropertyName("anoEleicaoLegislatura")]
        public string AnoEleicaoLegislatura { get; set; }

        [JsonPropertyName("situacaoTerminoLegislatura")]
        public string SituacaoTerminoLegislatura { get; set; }
    }
}
