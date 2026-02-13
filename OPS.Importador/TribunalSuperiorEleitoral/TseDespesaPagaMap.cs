using CsvHelper.Configuration;

namespace OPS.Importador.TribunalSuperiorEleitoral
{
    public sealed class TseDespesaPagaMap : ClassMap<TseDespesaPaga>
    {
        public TseDespesaPagaMap()
        {
            Map(m => m.DtGeracao).Name("DT_GERACAO");
            Map(m => m.HhGeracao).Name("HH_GERACAO");
            Map(m => m.AnoEleicao).Name("ANO_ELEICAO");
            Map(m => m.CdTipoEleicao).Name("CD_TIPO_ELEICAO");
            Map(m => m.NmTipoEleicao).Name("NM_TIPO_ELEICAO");
            Map(m => m.CdEleicao).Name("CD_ELEICAO");
            Map(m => m.DsEleicao).Name("DS_ELEICAO");
            Map(m => m.DtEleicao).Name("DT_ELEICAO");
            Map(m => m.StTurno).Name("ST_TURNO");
            Map(m => m.TpPrestacaoContas).Name("TP_PRESTACAO_CONTAS");
            Map(m => m.DtPrestacaoContas).Name("DT_PRESTACAO_CONTAS");
            Map(m => m.SqPrestadorContas).Name("SQ_PRESTADOR_CONTAS");
            Map(m => m.SgUf).Name("SG_UF");
            Map(m => m.DsTipoDocumento).Name("DS_TIPO_DOCUMENTO");
            Map(m => m.NrDocumento).Name("NR_DOCUMENTO");
            Map(m => m.CdFonteDespesa).Name("CD_FONTE_DESPESA");
            Map(m => m.DsFonteDespesa).Name("DS_FONTE_DESPESA");
            Map(m => m.CdOrigemDespesa).Name("CD_ORIGEM_DESPESA");
            Map(m => m.DsOrigemDespesa).Name("DS_ORIGEM_DESPESA");
            Map(m => m.CdNaturezaDespesa).Name("CD_NATUREZA_DESPESA");
            Map(m => m.DsNaturezaDespesa).Name("DS_NATUREZA_DESPESA");
            Map(m => m.CdEspecieRecurso).Name("CD_ESPECIE_RECURSO");
            Map(m => m.DsEspecieRecurso).Name("DS_ESPECIE_RECURSO");
            Map(m => m.SqDespesa).Name("SQ_DESPESA");
            Map(m => m.SqParcelamentoDespesa).Name("SQ_PARCELAMENTO_DESPESA");
            Map(m => m.DtPagtoDespesa).Name("DT_PAGTO_DESPESA");
            Map(m => m.DsDespesa).Name("DS_DESPESA");
            Map(m => m.VrPagtoDespesa).Name("VR_PAGTO_DESPESA");
        }
    }
}
