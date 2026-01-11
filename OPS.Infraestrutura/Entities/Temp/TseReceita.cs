using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class TseReceita
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

        [Column("CD_FONTE_RECEITA")]
        public string? CdFonteReceita { get; set; }

        [Column("DS_FONTE_RECEITA")]
        public string? DsFonteReceita { get; set; }

        [Column("CD_ORIGEM_RECEITA")]
        public string? CdOrigemReceita { get; set; }

        [Column("DS_ORIGEM_RECEITA")]
        public string? DsOrigemReceita { get; set; }

        [Column("CD_NATUREZA_RECEITA")]
        public string? CdNaturezaReceita { get; set; }

        [Column("DS_NATUREZA_RECEITA")]
        public string? DsNaturezaReceita { get; set; }

        [Column("CD_ESPECIE_RECEITA")]
        public string? CdEspecieReceita { get; set; }

        [Column("DS_ESPECIE_RECEITA")]
        public string? DsEspecieReceita { get; set; }

        [Column("CD_CNAE_DOADOR")]
        public string? CdCnaeDoador { get; set; }

        [Column("DS_CNAE_DOADOR")]
        public string? DsCnaeDoador { get; set; }

        [Column("NR_CPF_CNPJ_DOADOR")]
        public string? NrCpfCnpjDoador { get; set; }

        [Column("NM_DOADOR")]
        public string? NmDoador { get; set; }

        [Column("NM_DOADOR_RFB")]
        public string? NmDoadorRfb { get; set; }

        [Column("CD_ESFERA_PARTIDARIA_DOADOR")]
        public string? CdEsferaPartidariaDoador { get; set; }

        [Column("DS_ESFERA_PARTIDARIA_DOADOR")]
        public string? DsEsferaPartidariaDoador { get; set; }

        [Column("SG_UF_DOADOR")]
        public string? SgUfDoador { get; set; }

        [Column("CD_MUNICIPIO_DOADOR")]
        public string? CdMunicipioDoador { get; set; }

        [Column("NM_MUNICIPIO_DOADOR")]
        public string? NmMunicipioDoador { get; set; }

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
