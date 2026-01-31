using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class TseDespesaContratada
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

        [Column("SG_UE")]
        public string? SgUe { get; set; }

        [Column("NM_UE")]
        public string? NmUe { get; set; }

        [Column("NR_CNPJ_PRESTADOR_CONTA")]
        public string? NrCnpjPrestadorConta { get; set; }

        [Column("CD_CARGO")]
        public string? CdCargo { get; set; }

        [Column("DS_CARGO")]
        public string? DsCargo { get; set; }

        [Column("SQ_CANDIDATO")]
        public string? SqCandidato { get; set; }

        [Column("NR_CANDIDATO")]
        public string? NrCandidato { get; set; }

        [Column("NM_CANDIDATO")]
        public string? NmCandidato { get; set; }

        [Column("NR_CPF_CANDIDATO")]
        public string? NrCpfCandidato { get; set; }

        [Column("NR_CPF_VICE_CANDIDATO")]
        public string? NrCpfViceCandidato { get; set; }

        [Column("NR_PARTIDO")]
        public string? NrPartido { get; set; }

        [Column("SG_PARTIDO")]
        public string? SgPartido { get; set; }

        [Column("NM_PARTIDO")]
        public string? NmPartido { get; set; }

        [Column("CD_TIPO_FORNECEDOR")]
        public string? CdTipoFornecedor { get; set; }

        [Column("DS_TIPO_FORNECEDOR")]
        public string? DsTipoFornecedor { get; set; }

        [Column("CD_CNAE_FORNECEDOR")]
        public string? CdCnaeFornecedor { get; set; }

        [Column("DS_CNAE_FORNECEDOR")]
        public string? DsCnaeFornecedor { get; set; }

        [Column("NR_CPF_CNPJ_FORNECEDOR")]
        public string? NrCpfCnpjFornecedor { get; set; }

        [Column("NM_FORNECEDOR")]
        public string? NmFornecedor { get; set; }

        [Column("NM_FORNECEDOR_RFB")]
        public string? NmFornecedorRfb { get; set; }

        [Column("CD_ESFERA_PART_FORNECEDOR")]
        public string? CdEsferaPartFornecedor { get; set; }

        [Column("DS_ESFERA_PART_FORNECEDOR")]
        public string? DsEsferaPartFornecedor { get; set; }

        [Column("SG_UF_FORNECEDOR")]
        public string? SgUfFornecedor { get; set; }

        [Column("CD_MUNICIPIO_FORNECEDOR")]
        public string? CdMunicipioFornecedor { get; set; }

        [Column("NM_MUNICIPIO_FORNECEDOR")]
        public string? NmMunicipioFornecedor { get; set; }

        [Column("SQ_CANDIDATO_FORNECEDOR")]
        public string? SqCandidatoFornecedor { get; set; }

        [Column("NR_CANDIDATO_FORNECEDOR")]
        public string? NrCandidatoFornecedor { get; set; }

        [Column("CD_CARGO_FORNECEDOR")]
        public string? CdCargoFornecedor { get; set; }

        [Column("DS_CARGO_FORNECEDOR")]
        public string? DsCargoFornecedor { get; set; }

        [Column("NR_PARTIDO_FORNECEDOR")]
        public string? NrPartidoFornecedor { get; set; }

        [Column("SG_PARTIDO_FORNECEDOR")]
        public string? SgPartidoFornecedor { get; set; }

        [Column("NM_PARTIDO_FORNECEDOR")]
        public string? NmPartidoFornecedor { get; set; }

        [Column("DS_TIPO_DOCUMENTO")]
        public string? DsTipoDocumento { get; set; }

        [Column("NR_DOCUMENTO")]
        public string? NrDocumento { get; set; }

        [Column("CD_ORIGEM_DESPESA")]
        public string? CdOrigemDespesa { get; set; }

        [Column("DS_ORIGEM_DESPESA")]
        public string? DsOrigemDespesa { get; set; }

        [Column("SQ_DESPESA")]
        public string? SqDespesa { get; set; }

        [Column("DT_DESPESA")]
        public string? DtDespesa { get; set; }

        [Column("DS_DESPESA")]
        public string? DsDespesa { get; set; }

        [Column("VR_DESPESA_CONTRATADA")]
        public string? VrDespesaContratada { get; set; }
    }
}
