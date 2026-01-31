using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class TseCandidato
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

        [Column("NR_TURNO")]
        public string? NrTurno { get; set; }

        [Column("CD_ELEICAO")]
        public string? CdEleicao { get; set; }

        [Column("DS_ELEICAO")]
        public string? DsEleicao { get; set; }

        [Column("DT_ELEICAO")]
        public string? DtEleicao { get; set; }

        [Column("TP_ABRANGENCIA")]
        public string? TpAbrangencia { get; set; }

        [Column("SG_UF")]
        public string? SgUf { get; set; }

        [Column("SG_UE")]
        public string? SgUe { get; set; }

        [Column("NM_UE")]
        public string? NmUe { get; set; }

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

        [Column("NM_URNA_CANDIDATO")]
        public string? NmUrnaCandidato { get; set; }

        [Column("NM_SOCIAL_CANDIDATO")]
        public string? NmSocialCandidato { get; set; }

        [Column("NR_CPF_CANDIDATO")]
        public string? NrCpfCandidato { get; set; }

        [Column("NM_EMAIL")]
        public string? NmEmail { get; set; }

        [Column("CD_SITUACAO_CANDIDATURA")]
        public string? CdSituacaoCandidatura { get; set; }

        [Column("DS_SITUACAO_CANDIDATURA")]
        public string? DsSituacaoCandidatura { get; set; }

        [Column("CD_DETALHE_SITUACAO_CAND")]
        public string? CdDetalheSituacaoCand { get; set; }

        [Column("DS_DETALHE_SITUACAO_CAND")]
        public string? DsDetalheSituacaoCand { get; set; }

        [Column("TP_AGREMIACAO")]
        public string? TpAgremiacao { get; set; }

        [Column("NR_PARTIDO")]
        public string? NrPartido { get; set; }

        [Column("SG_PARTIDO")]
        public string? SgPartido { get; set; }

        [Column("NM_PARTIDO")]
        public string? NmPartido { get; set; }

        [Column("SQ_COLIGACAO")]
        public string? SqColigacao { get; set; }

        [Column("NM_COLIGACAO")]
        public string? NmColigacao { get; set; }

        [Column("DS_COMPOSICAO_COLIGACAO")]
        public string? DsComposicaoColigacao { get; set; }

        [Column("CD_NACIONALIDADE")]
        public string? CdNacionalidade { get; set; }

        [Column("DS_NACIONALIDADE")]
        public string? DsNacionalidade { get; set; }

        [Column("SG_UF_NASCIMENTO")]
        public string? SgUfNascimento { get; set; }

        [Column("CD_MUNICIPIO_NASCIMENTO")]
        public string? CdMunicipioNascimento { get; set; }

        [Column("NM_MUNICIPIO_NASCIMENTO")]
        public string? NmMunicipioNascimento { get; set; }

        [Column("DT_NASCIMENTO")]
        public string? DtNascimento { get; set; }

        [Column("NR_IDADE_DATA_POSSE")]
        public string? NrIdadeDataPosse { get; set; }

        [Column("NR_TITULO_ELEITORAL_CANDIDATO")]
        public string? NrTituloEleitoralCandidato { get; set; }

        [Column("CD_GENERO")]
        public string? CdGenero { get; set; }

        [Column("DS_GENERO")]
        public string? DsGenero { get; set; }

        [Column("CD_GRAU_INSTRUCAO")]
        public string? CdGrauInstrucao { get; set; }

        [Column("DS_GRAU_INSTRUCAO")]
        public string? DsGrauInstrucao { get; set; }

        [Column("CD_ESTADO_CIVIL")]
        public string? CdEstadoCivil { get; set; }

        [Column("DS_ESTADO_CIVIL")]
        public string? DsEstadoCivil { get; set; }

        [Column("CD_COR_RACA")]
        public string? CdCorRaca { get; set; }

        [Column("DS_COR_RACA")]
        public string? DsCorRaca { get; set; }

        [Column("CD_OCUPACAO")]
        public string? CdOcupacao { get; set; }

        [Column("DS_OCUPACAO")]
        public string? DsOcupacao { get; set; }

        [Column("VR_DESPESA_MAX_CAMPANHA")]
        public string? VrDespesaMaxCampanha { get; set; }

        [Column("CD_SIT_TOT_TURNO")]
        public string? CdSitTotTurno { get; set; }

        [Column("DS_SIT_TOT_TURNO")]
        public string? DsSitTotTurno { get; set; }

        [Column("ST_REELEICAO")]
        public string? StReeleicao { get; set; }

        [Column("ST_DECLARAR_BENS")]
        public string? StDeclararBens { get; set; }

        [Column("NR_PROTOCOLO_CANDIDATURA")]
        public string? NrProtocoloCandidatura { get; set; }

        [Column("NR_PROCESSO")]
        public string? NrProcesso { get; set; }

        [Column("CD_SITUACAO_CANDIDATO_PLEITO")]
        public string? CdSituacaoCandidatoPleito { get; set; }

        [Column("DS_SITUACAO_CANDIDATO_PLEITO")]
        public string? DsSituacaoCandidatoPleito { get; set; }

        [Column("CD_SITUACAO_CANDIDATO_URNA")]
        public string? CdSituacaoCandidatoUrna { get; set; }

        [Column("DS_SITUACAO_CANDIDATO_URNA")]
        public string? DsSituacaoCandidatoUrna { get; set; }

        [Column("ST_CANDIDATO_INSERIDO_URNA")]
        public string? StCandidatoInseridoUrna { get; set; }
    }
}
