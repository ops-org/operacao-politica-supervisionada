using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Rondonia.Entities
{
    public class DespesaRO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("classe_despesa__nome")]
        public string ClasseDespesaNome { get; set; }

        [JsonPropertyName("natureza_despesa__nome")]
        public string NaturezaDespesaNome { get; set; }

        [JsonPropertyName("numero_documento_fiscal")]
        public string NumeroDocumentoFiscal { get; set; }

        [JsonPropertyName("documento_fiscal__descricao")]
        public string DocumentoFiscalDescricao { get; set; }

        [JsonPropertyName("data_documento_fiscal")]
        public string DataDocumentoFiscal { get; set; }

        [JsonPropertyName("valor")]
        public string Valor { get; set; }

        [JsonPropertyName("situacao")]
        public int Situacao { get; set; }

        [JsonPropertyName("situacao__descricao")]
        public string SituacaoDescricao { get; set; }

        [JsonPropertyName("valor_recomendado_pagamento")]
        public string ValorRecomendadoPagamento { get; set; }

        [JsonPropertyName("valor_pago")]
        public string ValorPago { get; set; }

        [JsonPropertyName("data_pagamento")]
        public string DataPagamento { get; set; }

        [JsonPropertyName("escritorio__nome")]
        public object EscritorioNome { get; set; }

        [JsonPropertyName("escritorio__endereco")]
        public object EscritorioEndereco { get; set; }

        [JsonPropertyName("escritorio__proprietario")]
        public object EscritorioProprietario { get; set; }

        [JsonPropertyName("escritorio__municipio__nome")]
        public object EscritorioMunicipioNome { get; set; }

        [JsonPropertyName("data_vencimento")]
        public object DataVencimento { get; set; }

        [JsonPropertyName("data_certificacao")]
        public object DataCertificacao { get; set; }

        [JsonPropertyName("observacao")]
        public object Observacao { get; set; }

        [JsonPropertyName("fornecedor__razao_social")]
        public string FornecedorRazaoSocial { get; set; }

        [JsonPropertyName("fornecedor__cnpj_cpf")]
        public string FornecedorCnpjCpf { get; set; }

        [JsonPropertyName("fornecedor__endereco")]
        public string FornecedorEndereco { get; set; }

        [JsonPropertyName("fornecedor__cidade__nome")]
        public string FornecedorCidadeNome { get; set; }

        [JsonPropertyName("fornecedor__estado__sigla")]
        public string FornecedorEstadoSigla { get; set; }

        [JsonPropertyName("arquivo_doc_fiscal")]
        public string ArquivoDocFiscal { get; set; }
    }
}
