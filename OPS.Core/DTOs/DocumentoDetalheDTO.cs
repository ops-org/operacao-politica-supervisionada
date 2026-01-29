using System;

namespace OPS.Core.DTOs
{
    public class DocumentoDetalheDTO
    {
        public dynamic IdDespesa { get; set; }
        public dynamic IdDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public string TipoDocumento { get; set; }
        public string DataEmissao { get; set; }
        public string ValorDocumento { get; set; }
        public string ValorGlosa { get; set; }
        public string ValorLiquido { get; set; }
        public string ValorRestituicao { get; set; }
        public string NomePassageiro { get; set; }
        public string TrechoViagem { get; set; }
        public int Ano { get; set; }
        public short Mes { get; set; }
        public string Competencia { get; set; }
        public dynamic IdDespesaTipo { get; set; }
        public string DescricaoDespesa { get; set; }
        public string DescricaoDespesaEspecificacao { get; set; }
        public dynamic IdParlamentar { get; set; }
        public dynamic IdDeputado { get; set; }
        public string NomeParlamentar { get; set; }
        public string SiglaEstado { get; set; }
        public string SiglaPartido { get; set; }
        public dynamic IdFornecedor { get; set; }
        public string CnpjCpf { get; set; }
        public string NomeFornecedor { get; set; }
        public string Link { get; set; }
        public string Favorecido { get; set; }
        public string Observacao { get; set; }
    }
}
