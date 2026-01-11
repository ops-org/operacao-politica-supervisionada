using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class TseDespesaPaga
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

        [Column("DS_TIPO_DOCUMENTO")]
        public string? DsTipoDocumento { get; set; }

        [Column("NR_DOCUMENTO")]
        public string? NrDocumento { get; set; }

        [Column("CD_FONTE_DESPESA")]
        public string? CdFonteDespesa { get; set; }

        [Column("DS_FONTE_DESPESA")]
        public string? DsFonteDespesa { get; set; }

        [Column("CD_ORIGEM_DESPESA")]
        public string? CdOrigemDespesa { get; set; }

        [Column("DS_ORIGEM_DESPESA")]
        public string? DsOrigemDespesa { get; set; }

        [Column("CD_NATUREZA_DESPESA")]
        public string? CdNaturezaDespesa { get; set; }

        [Column("DS_NATUREZA_DESPESA")]
        public string? DsNaturezaDespesa { get; set; }

        [Column("CD_ESPECIE_RECURSO")]
        public string? CdEspecieRecurso { get; set; }

        [Column("DS_ESPECIE_RECURSO")]
        public string? DsEspecieRecurso { get; set; }

        [Column("SQ_DESPESA")]
        public string? SqDespesa { get; set; }

        [Column("SQ_PARCELAMENTO_DESPESA")]
        public string? SqParcelamentoDespesa { get; set; }

        [Column("DT_PAGTO_DESPESA")]
        public string? DtPagtoDespesa { get; set; }

        [Column("DS_DESPESA")]
        public string? DsDespesa { get; set; }

        [Column("VR_PAGTO_DESPESA")]
        public string? VrPagtoDespesa { get; set; }
    }
}
