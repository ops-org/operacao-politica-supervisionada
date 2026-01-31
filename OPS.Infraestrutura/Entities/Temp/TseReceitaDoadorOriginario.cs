using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class TseReceitaDoadorOriginario
    {
        [Column("DT_GERACAO")]
        public string? DtGeracao { get; set; }

        [Column("HH_GERACAO")]
        public string? HhGeracao { get; set; }

        [Column("ANO_ELEICAO")]
        public string? AnoEleicao { get; set; }

        [Column("CD_TIPO_ELEICAO")]
        public string? CdTipoEleicao { get; set; }

        [Column("NM_TIPO_ELEICAO")]
        public string? NmTipoEleicao { get; set; }

        [Column("CD_ELEICAO")]
        public string? CdEleicao { get; set; }

        [Column("DS_ELEICAO")]
        public string? DsEleicao { get; set; }

        [Column("DT_ELEICAO")]
        public string? DtEleicao { get; set; }

        [Column("ST_TURNO")]
        public string? StTurno { get; set; }

        [Column("TP_PRESTACAO_CONTAS")]
        public string? TpPrestacaoContas { get; set; }

        [Column("DT_PRESTACAO_CONTAS")]
        public string? DtPrestacaoContas { get; set; }

        [Column("SQ_PRESTADOR_CONTAS")]
        public string? SqPrestadorContas { get; set; }

        [Column("SG_UF")]
        public string? SgUf { get; set; }

        [Column("NR_CPF_CNPJ_DOADOR_ORIGINARIO")]
        public string? NrCpfCnpjDoadorOriginario { get; set; }

        [Column("NM_DOADOR_ORIGINARIO")]
        public string? NmDoadorOriginario { get; set; }

        [Column("NM_DOADOR_ORIGINARIO_RFB")]
        public string? NmDoadorOriginarioRfb { get; set; }

        [Column("TP_DOADOR_ORIGINARIO")]
        public string? TpDoadorOriginario { get; set; }

        [Column("CD_CNAE_DOADOR_ORIGINARIO")]
        public string? CdCnaeDoadorOriginario { get; set; }

        [Column("DS_CNAE_DOADOR_ORIGINARIO")]
        public string? DsCnaeDoadorOriginario { get; set; }

        [Column("SQ_RECEITA")]
        public string? SqReceita { get; set; }

        [Column("DT_RECEITA")]
        public string? DtReceita { get; set; }

        [Column("DS_RECEITA")]
        public string? DsReceita { get; set; }

        [Column("VR_RECEITA")]
        public string? VrReceita { get; set; }
    }
}
