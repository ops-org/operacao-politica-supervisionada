using System;
using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class DocumentoDetalheDTO
    {
        [JsonPropertyName("id_despesa")]
        public long IdDespesa { get; set; }

        [JsonPropertyName("id_documento")]
        public long? IdDocumento { get; set; }

        [JsonPropertyName("numero_documento")]
        public string NumeroDocumento { get; set; }

        [JsonPropertyName("tipo_documento")]
        public string TipoDocumento { get; set; }

        [JsonPropertyName("data_emissao")]
        public string DataEmissao { get; set; }

        [JsonPropertyName("valor_documento")]
        public string ValorDocumento { get; set; }

        [JsonPropertyName("valor_glosa")]
        public string ValorGlosa { get; set; }

        [JsonPropertyName("valor_liquido")]
        public string ValorLiquido { get; set; }

        [JsonPropertyName("valor_restituicao")]
        public string ValorRestituicao { get; set; }

        [JsonPropertyName("nome_passageiro")]
        public string NomePassageiro { get; set; }

        [JsonPropertyName("trecho_viagem")]
        public string TrechoViagem { get; set; }

        [JsonPropertyName("ano")]
        public int Ano { get; set; }

        [JsonPropertyName("mes")]
        public short Mes { get; set; }

        [JsonPropertyName("competencia")]
        public string Competencia { get; set; }

        [JsonPropertyName("id_despesa_tipo")]
        public int IdDespesaTipo { get; set; }

        [JsonPropertyName("descricao_despesa")]
        public string DescricaoDespesa { get; set; }

        [JsonPropertyName("descricao_despesa_especificacao")]
        public string DescricaoDespesaEspecificacao { get; set; }

        [JsonPropertyName("id_parlamentar")]
        public int IdParlamentar { get; set; }

        [JsonPropertyName("id_deputado")]
        public int? IdDeputado { get; set; }

        [JsonPropertyName("nome_parlamentar")]
        public string NomeParlamentar { get; set; }

        [JsonPropertyName("sigla_estado")]
        public string SiglaEstado { get; set; }

        [JsonPropertyName("sigla_partido")]
        public string SiglaPartido { get; set; }

        [JsonPropertyName("id_fornecedor")]
        public long IdFornecedor { get; set; }

        [JsonPropertyName("cnpj_cpf")]
        public string CnpjCpf { get; set; }

        [JsonPropertyName("nome_fornecedor")]
        public string NomeFornecedor { get; set; }

        [JsonPropertyName("favorecido")]
        public string Favorecido { get; set; }

        [JsonPropertyName("observacao")]
        public string Observacao { get; set; }

        [JsonPropertyName("url_documento")]
        public string UrlDocumento { get; set; }

        [JsonPropertyName("url_demais_documentos_mes")]
        public string UrlDemaisDocumentosMes { get; set; }

        [JsonPropertyName("url_detalhes_documento")]
        public string UrlDetalhesDocumento { get; set; }
    }
}
