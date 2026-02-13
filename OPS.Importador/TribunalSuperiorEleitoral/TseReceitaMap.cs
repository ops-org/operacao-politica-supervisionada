using CsvHelper.Configuration;

namespace OPS.Importador.TribunalSuperiorEleitoral
{
    public sealed class TseReceitaMap : ClassMap<TseReceita>
    {
        public TseReceitaMap()
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
            Map(m => m.SgUe).Name("SG_UE");
            Map(m => m.NmUe).Name("NM_UE");
            Map(m => m.NrCnpjPrestadorConta).Name("NR_CNPJ_PRESTADOR_CONTA");
            Map(m => m.CdCargo).Name("CD_CARGO");
            Map(m => m.DsCargo).Name("DS_CARGO");
            Map(m => m.SqCandidato).Name("SQ_CANDIDATO");
            Map(m => m.NrCandidato).Name("NR_CANDIDATO");
            Map(m => m.NmCandidato).Name("NM_CANDIDATO");
            Map(m => m.NrCpfCandidato).Name("NR_CPF_CANDIDATO");
            Map(m => m.NrCpfViceCandidato).Name("NR_CPF_VICE_CANDIDATO");
            Map(m => m.NrPartido).Name("NR_PARTIDO");
            Map(m => m.SgPartido).Name("SG_PARTIDO");
            Map(m => m.NmPartido).Name("NM_PARTIDO");
            Map(m => m.CdFonteReceita).Name("CD_FONTE_RECEITA");
            Map(m => m.DsFonteReceita).Name("DS_FONTE_RECEITA");
            Map(m => m.CdOrigemReceita).Name("CD_ORIGEM_RECEITA");
            Map(m => m.DsOrigemReceita).Name("DS_ORIGEM_RECEITA");
            Map(m => m.CdNaturezaReceita).Name("CD_NATUREZA_RECEITA");
            Map(m => m.DsNaturezaReceita).Name("DS_NATUREZA_RECEITA");
            Map(m => m.CdEspecieReceita).Name("CD_ESPECIE_RECEITA");
            Map(m => m.DsEspecieReceita).Name("DS_ESPECIE_RECEITA");
            Map(m => m.CdCnaeDoador).Name("CD_CNAE_DOADOR");
            Map(m => m.DsCnaeDoador).Name("DS_CNAE_DOADOR");
            Map(m => m.NrCpfCnpjDoador).Name("NR_CPF_CNPJ_DOADOR");
            Map(m => m.NmDoador).Name("NM_DOADOR");
            Map(m => m.NmDoadorRfb).Name("NM_DOADOR_RFB");
            Map(m => m.CdEsferaPartidariaDoador).Name("CD_ESFERA_PARTIDARIA_DOADOR");
            Map(m => m.DsEsferaPartidariaDoador).Name("DS_ESFERA_PARTIDARIA_DOADOR");
            Map(m => m.SgUfDoador).Name("SG_UF_DOADOR");
            Map(m => m.CdMunicipioDoador).Name("CD_MUNICIPIO_DOADOR");
            Map(m => m.NmMunicipioDoador).Name("NM_MUNICIPIO_DOADOR");
            // TODO
            //Map(m => m.SqCandidatoDoador).Name("SQ_CANDIDATO_DOADOR");
            //Map(m => m.NrCandidatoDoador).Name("NR_CANDIDATO_DOADOR");
            //Map(m => m.CdCargoCandidatoDoador).Name("CD_CARGO_CANDIDATO_DOADOR");
            //Map(m => m.DsCargoCandidatoDoador).Name("DS_CARGO_CANDIDATO_DOADOR");
            //Map(m => m.NrPartidoDoador).Name("NR_PARTIDO_DOADOR");
            //Map(m => m.SgPartidoDoador).Name("SG_PARTIDO_DOADOR");
            //Map(m => m.NmPartidoDoador).Name("NM_PARTIDO_DOADOR");
            //Map(m => m.NrReciboDoacao).Name("NR_RECIBO_DOACAO");
            //Map(m => m.NrDocumentoDoacao).Name("NR_DOCUMENTO_DOACAO");
            Map(m => m.SqReceita).Name("SQ_RECEITA");
            Map(m => m.DtReceita).Name("DT_RECEITA");
            Map(m => m.DsReceita).Name("DS_RECEITA");
            Map(m => m.VrReceita).Name("VR_RECEITA");
        }
    }
}
