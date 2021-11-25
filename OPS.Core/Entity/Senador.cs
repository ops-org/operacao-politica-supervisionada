using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Core.Entity
{
    public class IdentificacaoParlamentar
    {
        public string CodigoParlamentar { get; set; }
        public string CodigoPublicoNaLegAtual { get; set; }
        public string NomeParlamentar { get; set; }
        public string NomeCompletoParlamentar { get; set; }
        public string FormaTratamento { get; set; }
        public string SexoParlamentar { get; set; }
        public string UrlFotoParlamentar { get; set; }
        public string UrlPaginaParlamentar { get; set; }
        public string UrlPaginaParticular { get; set; }
        public string EmailParlamentar { get; set; }
        public string SiglaPartidoParlamentar { get; set; }
        public string UfParlamentar { get; set; }
    }

    public class DadosBasicosParlamentar
    {
        public string DataNascimento { get; set; }
        public string Naturalidade { get; set; }
        public string UfNaturalidade { get; set; }
        public string EnderecoParlamentar { get; set; }
    }

    public class Telefone
    {
        public string NumeroTelefone { get; set; }
        public string OrdemPublicacao { get; set; }
        public string IndicadorFax { get; set; }
    }

    public class Telefones
    {
        public Telefone[] Telefone { get; set; }
    }


    public partial class Profissoes
    {
        public Profissao[] Profissao { get; set; }
    }

    public partial class Profissao
    {
        public string NomeProfissao { get; set; }

        public string IndicadorAtividadePrincipal { get; set; }
    }

    public class Servico
    {
        public string NomeServico { get; set; }
        public string DescricaoServico { get; set; }
        public string UrlServico { get; set; }
    }

    public class OutrasInformacoes
    {
        public List<Servico> Servico { get; set; }
    }

    public class SenadorParlamentar
    {
        public IdentificacaoParlamentar IdentificacaoParlamentar { get; set; }
        public DadosBasicosParlamentar DadosBasicosParlamentar { get; set; }

        [JsonIgnore]
        public Telefones Telefones { get; set; }

        public Profissoes Profissoes { get; set; }
        public OutrasInformacoes OutrasInformacoes { get; set; }
    }

    public class DetalheParlamentar
    {
        public SenadorParlamentar Parlamentar { get; set; }
    }

    public class Senador
    {
        public DetalheParlamentar DetalheParlamentar { get; set; }
    }
}
