using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class FornecedorInfoDTO
    {
        [JsonPropertyName("id_fornecedor")]
        public string IdFornecedor { get; set; }

        [JsonPropertyName("cnpj_cpf")]
        public string CnpjCpf { get; set; }

        [JsonPropertyName("data_de_abertura")]
        public string DataDeAbertura { get; set; }

        [JsonPropertyName("categoria")]
        public string Categoria { get; set; }

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("nome_fantasia")]
        public string NomeFantasia { get; set; }

        [JsonPropertyName("atividade_principal")]
        public string AtividadePrincipal { get; set; }

        [JsonPropertyName("natureza_juridica")]
        public string NaturezaJuridica { get; set; }

        [JsonPropertyName("logradouro")]
        public string Logradouro { get; set; }

        [JsonPropertyName("numero")]
        public string Numero { get; set; }

        [JsonPropertyName("complemento")]
        public string Complemento { get; set; }

        [JsonPropertyName("cep")]
        public string Cep { get; set; }

        [JsonPropertyName("bairro")]
        public string Bairro { get; set; }

        [JsonPropertyName("cidade")]
        public string Cidade { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; }

        [JsonPropertyName("situacao_cadastral")]
        public string SituacaoCadastral { get; set; }

        [JsonPropertyName("data_da_situacao_cadastral")]
        public string DataDaSituacaoCadastral { get; set; }

        [JsonPropertyName("motivo_situacao_cadastral")]
        public string MotivoSituacaoCadastral { get; set; }

        [JsonPropertyName("situacao_especial")]
        public string SituacaoEspecial { get; set; }

        [JsonPropertyName("data_situacao_especial")]
        public string DataSituacaoEspecial { get; set; }

        [JsonPropertyName("endereco_eletronico")]
        public string EnderecoEletronico { get; set; }

        [JsonPropertyName("telefone")]
        public string Telefone { get; set; }

        [JsonPropertyName("telefone2")]
        public string Telefone2 { get; set; }

        [JsonPropertyName("ente_federativo_responsavel")]
        public string EnteFederativoResponsavel { get; set; }

        [JsonPropertyName("capital_social")]
        public string CapitalSocial { get; set; }

        [JsonPropertyName("doador")]
        public bool Doador { get; set; }

        [JsonPropertyName("obtido_em")]
        public string ObtidoEm { get; set; }

        [JsonPropertyName("origem")]
        public string Origem { get; set; }

        [JsonPropertyName("atividade_secundaria")]
        public List<string> AtividadeSecundaria { get; set; } = new List<string>();
    }
}
