using CsvHelper.Configuration;

namespace OPS.Importador.TribunalSuperiorEleitoral
{
    public sealed class TseDespesaContratadaMap : ClassMap<TseDespesaContratada>
    {
        public TseDespesaContratadaMap()
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
            Map(m => m.CdTipoFornecedor).Name("CD_TIPO_FORNECEDOR");
            Map(m => m.DsTipoFornecedor).Name("DS_TIPO_FORNECEDOR");
            Map(m => m.CdCnaeFornecedor).Name("CD_CNAE_FORNECEDOR");
            Map(m => m.DsCnaeFornecedor).Name("DS_CNAE_FORNECEDOR");
            Map(m => m.NrCpfCnpjFornecedor).Name("NR_CPF_CNPJ_FORNECEDOR");
            Map(m => m.NmFornecedor).Name("NM_FORNECEDOR");
            Map(m => m.NmFornecedorRfb).Name("NM_FORNECEDOR_RFB");
            Map(m => m.CdEsferaPartFornecedor).Name("CD_ESFERA_PART_FORNECEDOR");
            Map(m => m.DsEsferaPartFornecedor).Name("DS_ESFERA_PART_FORNECEDOR");
            Map(m => m.SgUfFornecedor).Name("SG_UF_FORNECEDOR");
            Map(m => m.CdMunicipioFornecedor).Name("CD_MUNICIPIO_FORNECEDOR");
            Map(m => m.NmMunicipioFornecedor).Name("NM_MUNICIPIO_FORNECEDOR");
            Map(m => m.SqCandidatoFornecedor).Name("SQ_CANDIDATO_FORNECEDOR");
            Map(m => m.NrCandidatoFornecedor).Name("NR_CANDIDATO_FORNECEDOR");
            Map(m => m.CdCargoFornecedor).Name("CD_CARGO_FORNECEDOR");
            Map(m => m.DsCargoFornecedor).Name("DS_CARGO_FORNECEDOR");
            Map(m => m.NrPartidoFornecedor).Name("NR_PARTIDO_FORNECEDOR");
            Map(m => m.SgPartidoFornecedor).Name("SG_PARTIDO_FORNECEDOR");
            Map(m => m.NmPartidoFornecedor).Name("NM_PARTIDO_FORNECEDOR");
            Map(m => m.DsTipoDocumento).Name("DS_TIPO_DOCUMENTO");
            Map(m => m.NrDocumento).Name("NR_DOCUMENTO");
            Map(m => m.CdOrigemDespesa).Name("CD_ORIGEM_DESPESA");
            Map(m => m.DsOrigemDespesa).Name("DS_ORIGEM_DESPESA");
            Map(m => m.SqDespesa).Name("SQ_DESPESA");
            Map(m => m.DtDespesa).Name("DT_DESPESA");
            Map(m => m.DsDespesa).Name("DS_DESPESA");
            Map(m => m.VrDespesaContratada).Name("VR_DESPESA_CONTRATADA");
        }
    }
}
