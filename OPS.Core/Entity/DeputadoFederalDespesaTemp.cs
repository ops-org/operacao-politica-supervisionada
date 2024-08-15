using System;
using System.Text.Json.Serialization;
using Dapper;

namespace OPS.Core.Entity
{
    [Table("cf_despesa_temp", Schema = "ops_tmp")]
    public class DeputadoFederalDespesaTemp
    {
        [JsonIgnore]
        [Column("id")]
        public int Id { get; set; }

        [Column("idDeputado")]
        public long? IdDeputado { get; set; }

        [Column("nomeParlamentar")]
        public string Nome { get; set; }

        [Column("numeroCarteiraParlamentar")]
        public int? CarteiraParlamentar { get; set; }

        [Column("legislatura")]
        public int? Legislatura { get; set; }

        [Column("siglaUF")]
        public string SiglaUF { get; set; }

        [Column("siglaPartido")]
        public string SiglaPartido { get; set; }

        [Column("codigoLegislatura")]
        public int? CodigoLegislatura { get; set; }

        [Column("numeroSubCota")]
        public int? NumeroSubCota { get; set; }

        [Column("descricao")]
        public string Descricao { get; set; }

        [Column("numeroEspecificacaoSubCota")]
        public int? NumeroEspecificacaoSubCota { get; set; }

        [Column("descricaoEspecificacao")]
        public string DescricaoEspecificacao { get; set; }

        [Column("fornecedor")]
        public string Fornecedor { get; set; }

        [Column("cnpjCPF")]
        public string CnpjCpf { get; set; }

        [Column("numero")]
        public string Numero { get; set; }

        [Column("tipoDocumento")]
        public int? TipoDocumento { get; set; }

        [Column("dataEmissao")]
        public DateOnly? DataEmissao { get; set; }

        [Column("valorDocumento")]
        public decimal ValorDocumento { get; set; }

        [Column("valorGlosa")]
        public decimal ValorGlosa { get; set; }

        [Column("valorLiquido")]
        public decimal ValorLiquido { get; set; }

        [Column("mes")]
        public short Mes { get; set; }

        [Column("ano")]
        public short Ano { get; set; }

        [Column("parcela")]
        public int? Parcela { get; set; }

        [Column("passageiro")]
        public string Passageiro { get; set; }

        [Column("trecho")]
        public string Trecho { get; set; }

        [Column("lote")]
        public int? Lote { get; set; }

        [Column("ressarcimento")]
        public int? NumeroRessarcimento { get; set; }

        [Column("idDocumento")]
        public string IdDocumento { get; set; }

        [Column("restituicao")]
        public decimal? ValorRestituicao { get; set; }

        [Column("datPagamentoRestituicao")]
        public DateOnly? DataPagamentoRestituicao { get; set; }

        [Column("numeroDeputadoID")]
        public int? NumeroDeputadoID { get; set; }

        [Column("cpf")]
        public string Cpf { get; set; }

        [Column("urlDocumento")]
        public string UrlDocumento { get; set; }

        [JsonIgnore]
        [Column("hash")]
        public byte[] Hash { get; set; }
    }
}
