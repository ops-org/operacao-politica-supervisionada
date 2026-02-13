using CsvHelper.Configuration;

namespace OPS.Importador.TribunalSuperiorEleitoral
{
    public sealed class TseReceitaDoadorOriginarioMap : ClassMap<TseReceitaDoadorOriginario>
    {
        public TseReceitaDoadorOriginarioMap()
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
            Map(m => m.NrCpfCnpjDoadorOriginario).Name("NR_CPF_CNPJ_DOADOR_ORIGINARIO");
            Map(m => m.NmDoadorOriginario).Name("NM_DOADOR_ORIGINARIO");
            Map(m => m.NmDoadorOriginarioRfb).Name("NM_DOADOR_ORIGINARIO_RFB");
            Map(m => m.TpDoadorOriginario).Name("TP_DOADOR_ORIGINARIO");
            Map(m => m.CdCnaeDoadorOriginario).Name("CD_CNAE_DOADOR_ORIGINARIO");
            Map(m => m.DsCnaeDoadorOriginario).Name("DS_CNAE_DOADOR_ORIGINARIO");
            Map(m => m.SqReceita).Name("SQ_RECEITA");
            Map(m => m.DtReceita).Name("DT_RECEITA");
            Map(m => m.DsReceita).Name("DS_RECEITA");
            Map(m => m.VrReceita).Name("VR_RECEITA");
        }
    }
}
