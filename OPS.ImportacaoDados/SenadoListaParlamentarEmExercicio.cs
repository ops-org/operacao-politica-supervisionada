using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OPS.ImportacaoDados
{
    public partial class Parlamentar
    {
        [JsonProperty("IdentificacaoParlamentar")]
        public IdentificacaoParlamentar IdentificacaoParlamentar { get; set; }

        [JsonProperty("DadosBasicosParlamentar")]
        public DadosBasicosParlamentar DadosBasicosParlamentar { get; set; }

        //[JsonProperty("Telefones")]
        //public Telefones Telefones { get; set; }

        //[JsonProperty("HistoricoAcademico")]
        //public HistoricoAcademico HistoricoAcademico { get; set; }

        //[JsonProperty("Profissoes")]
        //public Profissoes Profissoes { get; set; }
    }

    public partial class DadosBasicosParlamentar
    {
        [JsonProperty("DataNascimento")]
        public DateTimeOffset DataNascimento { get; set; }

        [JsonProperty("Naturalidade")]
        public string Naturalidade { get; set; }

        [JsonProperty("UfNaturalidade")]
        public string UfNaturalidade { get; set; }

        [JsonProperty("EnderecoParlamentar")]
        public string EnderecoParlamentar { get; set; }
    }

    public partial class HistoricoAcademico
    {
        [JsonProperty("Curso")]
        public Curso[] Curso { get; set; }
    }

    public partial class Curso
    {
        [JsonProperty("NomeCurso")]
        public string NomeCurso { get; set; }

        [JsonProperty("GrauInstrucao", NullValueHandling = NullValueHandling.Ignore)]
        public string GrauInstrucao { get; set; }

        [JsonProperty("Estabelecimento")]
        public string Estabelecimento { get; set; }

        [JsonProperty("Local")]
        public string Local { get; set; }
    }

    public partial class IdentificacaoParlamentar
    {
        [JsonProperty("CodigoParlamentar")]
        public int CodigoParlamentar { get; set; }

        [JsonProperty("CodigoPublicoNaLegAtual")]
        public int CodigoPublicoNaLegAtual { get; set; }

        [JsonProperty("NomeParlamentar")]
        public string NomeParlamentar { get; set; }

        [JsonProperty("NomeCompletoParlamentar")]
        public string NomeCompletoParlamentar { get; set; }

        [JsonProperty("SexoParlamentar")]
        public string SexoParlamentar { get; set; }

        [JsonProperty("FormaTratamento")]
        public string FormaTratamento { get; set; }

        [JsonProperty("UrlFotoParlamentar")]
        public string UrlFotoParlamentar { get; set; }

        [JsonProperty("UrlPaginaParlamentar")]
        public string UrlPaginaParlamentar { get; set; }

        [JsonProperty("UrlPaginaParticular")]
        public string UrlPaginaParticular { get; set; }

        [JsonProperty("EmailParlamentar")]
        public string EmailParlamentar { get; set; }

        [JsonProperty("SiglaPartidoParlamentar")]
        public string SiglaPartidoParlamentar { get; set; }

        [JsonProperty("UfParlamentar")]
        public string UfParlamentar { get; set; }
    }

    public partial class Profissoes
    {
        [JsonProperty("Profissao")]
        public Profissao Profissao { get; set; }
    }

    public partial class Profissao
    {
        [JsonProperty("NomeProfissao")]
        public string NomeProfissao { get; set; }

        [JsonProperty("IndicadorAtividadePrincipal")]
        public string IndicadorAtividadePrincipal { get; set; }
    }

    public partial class Telefones
    {
        [JsonProperty("Telefone")]
        public Telefone[] Telefone { get; set; }
    }

    public partial class Telefone
    {
        [JsonProperty("NumeroTelefone")]
        public long NumeroTelefone { get; set; }

        [JsonProperty("OrdemPublicacao")]
        public long OrdemPublicacao { get; set; }

        [JsonProperty("IndicadorFax")]
        public string IndicadorFax { get; set; }
    }




    public partial class Mandato
    {
        [JsonProperty("CodigoMandato", NullValueHandling = NullValueHandling.Ignore)]
        public long? CodigoMandato { get; set; }

        [JsonProperty("UfParlamentar")]
        public string UfParlamentar { get; set; }

        [JsonProperty("PrimeiraLegislaturaDoMandato")]
        public ALegislaturaDoMandato PrimeiraLegislaturaDoMandato { get; set; }

        [JsonProperty("SegundaLegislaturaDoMandato")]
        public ALegislaturaDoMandato SegundaLegislaturaDoMandato { get; set; }

        [JsonProperty("DescricaoParticipacao", NullValueHandling = NullValueHandling.Ignore)]
        public string DescricaoParticipacao { get; set; }

        [JsonProperty("Suplentes", NullValueHandling = NullValueHandling.Ignore)]
        public Suplentes Suplentes { get; set; }

        [JsonProperty("Exercicios", NullValueHandling = NullValueHandling.Ignore)]
        public Exercicios Exercicios { get; set; }
    }

    public partial class Exercicios
    {
        [JsonProperty("Exercicio")]
        public Exercicio[] Exercicio { get; set; }
    }

    public partial class Exercicio
    {
        [JsonProperty("CodigoExercicio")]
        public long CodigoExercicio { get; set; }

        [JsonProperty("DataInicio")]
        public DateTimeOffset DataInicio { get; set; }

        [JsonProperty("DataFim", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DataFim { get; set; }

        [JsonProperty("SiglaCausaAfastamento", NullValueHandling = NullValueHandling.Ignore)]
        public string SiglaCausaAfastamento { get; set; }

        [JsonProperty("DescricaoCausaAfastamento", NullValueHandling = NullValueHandling.Ignore)]
        public string DescricaoCausaAfastamento { get; set; }

        [JsonProperty("DataLeitura", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DataLeitura { get; set; }
    }

    public partial class ALegislaturaDoMandato
    {
        [JsonProperty("NumeroLegislatura")]
        public int NumeroLegislatura { get; set; }

        [JsonProperty("DataInicio")]
        public DateTimeOffset DataInicio { get; set; }

        [JsonProperty("DataFim")]
        public DateTimeOffset DataFim { get; set; }
    }

    public partial class Suplentes
    {
        [JsonProperty("Suplente")]
        public Suplente[] Suplente { get; set; }
    }

    public partial class Suplente
    {
        [JsonProperty("DescricaoParticipacao")]
        public string DescricaoParticipacao { get; set; }

        [JsonProperty("CodigoParlamentar")]
        public int CodigoParlamentar { get; set; }

        [JsonProperty("NomeParlamentar")]
        public string NomeParlamentar { get; set; }
    }
}
