using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class DeputadoFederalDespesaTemp
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Column("id_deputado")]
        public long? IdDeputado { get; set; }

        [Column("nome_parlamentar")]
        public string? NomeParlamentar { get; set; }

        [Column("numero_carteira_parlamentar")]
        public int? NumeroCarteiraParlamentar { get; set; }

        [Column("legislatura")]
        public int? Legislatura { get; set; }

        [Column("sigla_uf")]
        public string? SiglaUF { get; set; }

        [Column("sigla_partido")]
        public string? SiglaPartido { get; set; }

        [Column("codigo_legislatura")]
        public int? CodigoLegislatura { get; set; }

        [Column("numero_sub_cota")]
        public int? NumeroSubCota { get; set; }

        [Column("descricao")]
        public string? Descricao { get; set; }

        [Column("numero_especificacao_sub_cota")]
        public int? NumeroEspecificacaoSubCota { get; set; }

        [Column("descricao_especificacao")]
        public string? DescricaoEspecificacao { get; set; }

        [Column("fornecedor")]
        public string? Fornecedor { get; set; }

        [Column("cnpj_cpf")]
        public string? CnpjCpf { get; set; }

        [Column("numero")]
        public string? Numero { get; set; }

        [Column("tipo_documento")]
        public int? TipoDocumento { get; set; }

        [Column("data_emissao")]
        public DateOnly? DataEmissao { get; set; }

        [Column("valor_documento")]
        public decimal? ValorDocumento { get; set; }

        [Column("valor_glosa")]
        public decimal? ValorGlosa { get; set; }

        [Column("valor_liquido")]
        public decimal? ValorLiquido { get; set; }

        [Column("mes")]
        public int? Mes { get; set; }

        [Column("ano")]
        public int Ano { get; set; }

        [Column("parcela")]
        public decimal? Parcela { get; set; }

        [Column("passageiro")]
        public string? Passageiro { get; set; }

        [Column("trecho")]
        public string? Trecho { get; set; }

        [Column("lote")]
        public int? Lote { get; set; }

        [Column("ressarcimento")]
        public int? Ressarcimento { get; set; }

        [Column("id_documento")]
        public string? IdDocumento { get; set; }

        [Column("restituicao")]
        public decimal? Restituicao { get; set; }

        [Column("data_pagamento_restituicao")]
        public DateOnly? DataPagamentoRestituicao { get; set; }

        [Column("numero_deputado_id")]
        public int? NumeroDeputadoID { get; set; }

        [Column("cpf")]
        public string? Cpf { get; set; }

        [Column("url_documento")]
        public string? UrlDocumento { get; set; }

        [Column("hash")]
        public byte[] Hash { get; set; } = new byte[0];

        [Column("id_fornecedor")]
        public long? IdFornecedor { get; set; }
    }
}
