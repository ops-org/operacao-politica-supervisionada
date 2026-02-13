using System.ComponentModel.DataAnnotations.Schema;

namespace OPS.Infraestrutura.Entities.Temp
{
    public class TseCandidato
    {
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Nome completo do candidato. Aparece no TSE como: NOME_CANDIDATO (1994_BR), NM_CANDIDATO (1996).
        /// </summary>
        [Column("nome")]
        public string? Nome { get; set; }

        /// <summary>
        /// Descrição da eleição. Aparece no TSE como: DESCRICAO_ELEICAO (1994_BR), DS_ELEICAO (1996).
        /// </summary>
        [Column("descricao")]
        public string? Descricao { get; set; }

        /// <summary>
        /// Código da eleição. Aparece no TSE como: CD_ELEICAO (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("codigo_eleicao")]
        public string? CodigoEleicao { get; set; }

        /// <summary>
        /// Código do tipo de eleição. Aparece no TSE como: CD_TIPO_ELEICAO (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("codigo_tipo_eleicao")]
        public string? CodigoTipoEleicao { get; set; }

        /// <summary>
        /// Data em que ocorreu a eleição. Aparece no TSE como: DT_ELEICAO (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("data_eleicao")]
        public string? DataEleicao { get; set; }

        /// <summary>
        /// Tipo de abrangência da eleição. Aparece no TSE como: TP_ABRANGENCIA (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("tipo_abrangencia_eleicao")]
        public string? TipoAbrangenciaEleicao { get; set; }

        /// <summary>
        /// Nome do tipo de eleição. Aparece no TSE como: NM_TIPO_ELEICAO (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("tipo_eleicao")]
        public string? NomeTipoEleicao { get; set; }

        /// <summary>
        /// Código da situação de totalização do candidato naquele turno. Aparece no TSE como: COD_SIT_TOT_TURNO (1994_BR), CD_SIT_TOT_TURNO (1996).
        /// </summary>
        [Column("codigo_totalizacao_turno")]
        public string? CodigoTotalizacaoTurno { get; set; }

        /// <summary>
        /// Descrição da situação de totalização do candidato naquele turno. Aparece no TSE como: DESC_SIT_TOT_TURNO (1994_BR), DS_SIT_TOT_TURNO (1996).
        /// </summary>
        [Column("totalizacao_turno")]
        public string? TotalizacaoTurno { get; set; }

        /// <summary>
        /// Número do turno. Aparece no TSE como: NUM_TURNO (1994_BR), NR_TURNO (1996).
        /// </summary>
        [Column("turno")]
        public string? Turno { get; set; }

        /// <summary>
        /// Sigla da Unidade Eleitoral (Em caso de eleição majoritária é a sigla da UF que o candidato concorre (texto) e em caso de eleição municipal é o código TSE do município (número)). Assume os valores especiais BR, ZZ e VT para designar, respectivamente, o Brasil, Exterior e Voto em Trânsito. Aparece no TSE como: SIGLA_UE (1994_BR), SG_UE (1996).
        /// </summary>
        [Column("sigla_unidade_eleitoral")]
        public string? SiglaUnidadeEleitoral { get; set; }

        /// <summary>
        /// Sigla da Unidade da Federação em que ocorreu a eleição. Aparece no TSE como: SIGLA_UF (1994_BR), SG_UF (1996).
        /// </summary>
        [Column("sigla_unidade_federativa")]
        public string? SiglaUnidadeFederativa { get; set; }

        /// <summary>
        /// Ano da eleição. Aparece no TSE como: ANO_ELEICAO (1994_BR).
        /// </summary>
        [Column("ano")]
        public string? Ano { get; set; }

        /// <summary>
        /// Código do estado civil do candidato. Aparece no TSE como: CODIGO_ESTADO_CIVIL (1994_BR), CD_ESTADO_CIVIL (1996).
        /// </summary>
        [Column("codigo_estado_civil")]
        public string? CodigoEstadoCivil { get; set; }

        /// <summary>
        /// Código da etnia (cor/raça) do candidato (auto declaração). Aparece no TSE como: CODIGO_ETNIA (1994_BR), CD_COR_RACA (1996).
        /// </summary>
        [Column("codigo_etnia")]
        public string? CodigoEtnia { get; set; }

        /// <summary>
        /// Código do sexo do candidato. Aparece no TSE como: CODIGO_SEXO (1994_BR), CD_GENERO (1996).
        /// </summary>
        [Column("codigo_genero")]
        public string? CodigoGenero { get; set; }

        /// <summary>
        /// Código do grau de instrução do candidato. Gerado internamente pelos sistemas eleitorais. Aparece no TSE como: CODIGO_GRAU_INSTRUCAO (1994_BR), CD_GRAU_INSTRUCAO (1996).
        /// </summary>
        [Column("codigo_grau_instrucao")]
        public string? CodigoGrauInstrucao { get; set; }

        /// <summary>
        /// Código TSE do município da nascimento do candidato. Aparece no TSE como: CODIGO_MUNICIPIO_NASCIMENTO (1994_BR), CD_MUNICIPIO_NASCIMENTO (1996).
        /// </summary>
        [Column("codigo_municipio_nascimento")]
        public string? CodigoMunicipioNascimento { get; set; }

        /// <summary>
        /// Código da nacionalidade do candidato. Aparece no TSE como: CODIGO_NACIONALIDADE (1994_BR), CD_NACIONALIDADE (1996).
        /// </summary>
        [Column("codigo_nacionalidade")]
        public string? CodigoNacionalidade { get; set; }

        /// <summary>
        /// Código da ocupação do candidato. Aparece no TSE como: CODIGO_OCUPACAO (1994_BR), CD_OCUPACAO (1996).
        /// </summary>
        [Column("codigo_ocupacao")]
        public string? CodigoOcupacao { get; set; }

        /// <summary>
        /// Descrição da federação. Aparece no TSE como: DS_COMPOSICAO_FEDERACAO (2022). Coluna adicionada em 2022.
        /// </summary>
        [Column("composicao_federacao")]
        public string? ComposicaoFederacao { get; set; }

        /// <summary>
        /// CPF do candidato. Aparece no TSE como: CPF_CANDIDATO (1994_BR), NR_CPF_CANDIDATO (1996).
        /// </summary>
        [Column("cpf")]
        public string? Cpf { get; set; }

        /// <summary>
        /// Data de nascimento do candidato. Aparece no TSE como: DATA_NASCIMENTO (1994_BR), DT_NASCIMENTO (1996).
        /// </summary>
        [Column("data_nascimento")]
        public string? DataNascimento { get; set; }

        /// <summary>
        /// Endereço de e-mail do candidato. Aparece no TSE como: NM_EMAIL (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Descrição do estado civil do candidato. Aparece no TSE como: DESCRICAO_ESTADO_CIVIL (1994_BR), DS_ESTADO_CIVIL (1996).
        /// </summary>
        [Column("estado_civil")]
        public string? EstadoCivil { get; set; }

        /// <summary>
        /// Etnia (cor/raça) do candidato (auto declaração). Aparece no TSE como: DS_COR_RACA (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("etnia")]
        public string? Etnia { get; set; }

        /// <summary>
        /// Federação. Aparece no TSE como: NM_FEDERACAO (2022). Coluna adicionada em 2022.
        /// </summary>
        [Column("federacao")]
        public string? Federacao { get; set; }

        /// <summary>
        /// Descrição do sexo do candidato. Aparece no TSE como: DESCRICAO_SEXO (1994_BR), DS_GENERO (1996).
        /// </summary>
        [Column("genero")]
        public string? Genero { get; set; }

        /// <summary>
        /// Descrição do grau de instrução do candidato. Aparece no TSE como: DESCRICAO_GRAU_INSTRUCAO (1994_BR), DS_GRAU_INSTRUCAO (1996).
        /// </summary>
        [Column("grau_instrucao")]
        public string? GrauInstrucao { get; set; }

        /// <summary>
        /// Nome do município de nascimento do candidato. Aparece no TSE como: NOME_MUNICIPIO_NASCIMENTO (1994_BR), NM_MUNICIPIO_NASCIMENTO (1996).
        /// </summary>
        [Column("municipio_nascimento")]
        public string? MunicipioNascimento { get; set; }

        /// <summary>
        /// Descrição da nacionalidade do candidato. Aparece no TSE como: DESCRICAO_NACIONALIDADE (1994_BR), DS_NACIONALIDADE (1996).
        /// </summary>
        [Column("nacionalidade")]
        public string? Nacionalidade { get; set; }

        /// <summary>
        /// Nome social do candidato. Aparece no TSE como: NM_SOCIAL_CANDIDATO (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("nome_social")]
        public string? NomeSocial { get; set; }

        /// <summary>
        /// Número da federação. Aparece no TSE como: NR_FEDERACAO (2022). Coluna adicionada em 2022.
        /// </summary>
        [Column("numero_federacao")]
        public string? NumeroFederacao { get; set; }

        /// <summary>
        /// Descrição da ocupação do candidato. Aparece no TSE como: DESCRICAO_OCUPACAO (1994_BR), DS_OCUPACAO (1996).
        /// </summary>
        [Column("ocupacao")]
        public string? Ocupacao { get; set; }

        /// <summary>
        /// Sigla da federação. Aparece no TSE como: SG_FEDERACAO (2022). Coluna adicionada em 2022.
        /// </summary>
        [Column("sigla_federacao")]
        public string? SiglaFederacao { get; set; }

        /// <summary>
        /// Sigla da UF de nascimento do candidato. Aparece no TSE como: SIGLA_UF_NASCIMENTO (1994_BR), SG_UF_NASCIMENTO (1996).
        /// </summary>
        [Column("sigla_unidade_federativa_nascimento")]
        public string? SiglaUnidadeFederativaNascimento { get; set; }

        /// <summary>
        /// Situação da prestação de contas. Aparece no TSE como: ST_PREST_CONTAS (2022). Coluna adicionada em 2022.
        /// </summary>
        [Column("situacao_prestacao_contas")]
        public string? SituacaoPrestacaoContas { get; set; }

        /// <summary>
        /// ?. Aparece no TSE como: NM_TIPO_DESTINACAO_VOTOS (2022). Coluna adicionada em 2022.
        /// </summary>
        [Column("tipo_destinacao_votos")]
        public string? TipoDestinacaoVotos { get; set; }

        /// <summary>
        /// Número do título eleitoral do candidato. Aparece no TSE como: NUM_TITULO_ELEITORAL_CANDIDATO (1994_BR), NR_TITULO_ELEITORAL_CANDIDATO (1996).
        /// </summary>
        [Column("titulo_eleitoral")]
        public string? TituloEleitoral { get; set; }

        /// <summary>
        /// Descrição da Unidade Eleitoral. Aparece no TSE como: DESCRICAO_UE (1994_BR), NM_UE (1996).
        /// </summary>
        [Column("unidade_eleitoral")]
        public string? UnidadeEleitoral { get; set; }

        /// <summary>
        /// Código sequencial da legenda gerado pela Justiça Eleitoral. Aparece no TSE como: CODIGO_LEGENDA (1994_BR), SQ_COLIGACAO (1996).
        /// </summary>
        [Column("codigo_legenda")]
        public string? CodigoLegenda { get; set; }

        /// <summary>
        /// Composição da legenda. Aparece no TSE como: COMPOSICAO_LEGENDA (1994_BR), DS_COMPOSICAO_COLIGACAO (1996).
        /// </summary>
        [Column("composicao_legenda")]
        public string? ComposicaoLegenda { get; set; }

        /// <summary>
        /// Nome da legenda. Aparece no TSE como: NOME_LEGENDA (1994_BR), NM_COLIGACAO (1996).
        /// </summary>
        [Column("legenda")]
        public string? Legenda { get; set; }

        /// <summary>
        /// Número do partido. Aparece no TSE como: NUMERO_PARTIDO (1994_BR), NR_PARTIDO (1996).
        /// </summary>
        [Column("numero_partido")]
        public string? NumeroPartido { get; set; }

        /// <summary>
        /// Nome do partido. Aparece no TSE como: NOME_PARTIDO (1994_BR), NM_PARTIDO (1996).
        /// </summary>
        [Column("partido")]
        public string? Partido { get; set; }

        /// <summary>
        /// Sigla da legenda. Aparece no TSE como: SIGLA_LEGENDA (1994_BR).
        /// </summary>
        [Column("sigla_legenda")]
        public string? SiglaLegenda { get; set; }

        /// <summary>
        /// Sigla do partido. Aparece no TSE como: SIGLA_PARTIDO (1994_BR), SG_PARTIDO (1996).
        /// </summary>
        [Column("sigla_partido")]
        public string? SiglaPartido { get; set; }

        /// <summary>
        /// Tipo de agremiação - pode assumir os valores "coligação" (quando o candidato concorre por coligação) e "partido isolado" (quando o candidato concorre somente pelo partido). Aparece no TSE como: TP_AGREMIACAO (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("tipo_agremiacao")]
        public string? TipoAgremiacao { get; set; }

        /// <summary>
        /// . Aparece no TSE como: ST_CANDIDATO_INSERIDO_URNA (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("candidatura_inserida_urna")]
        public string? CandidaturaInseridaUrna { get; set; }

        /// <summary>
        /// Descrição do cargo a que o candidato concorre. Aparece no TSE como: DESCRICAO_CARGO (1994_BR), DS_CARGO (1996).
        /// </summary>
        [Column("cargo")]
        public string? Cargo { get; set; }

        /// <summary>
        /// Código do cargo a que o candidato concorre. Aparece no TSE como: CODIGO_CARGO (1994_BR), CD_CARGO (1996).
        /// </summary>
        [Column("codigo_cargo")]
        public string? CodigoCargo { get; set; }

        /// <summary>
        /// Código do detalhe da situação do registro de candidatura do candidato. Aparece no TSE como: CD_DETALHE_SITUACAO_CAND (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("codigo_detalhe_situacao_candidatura")]
        public string? CodigoDetalheSituacaoCandidatura { get; set; }

        /// <summary>
        /// ? (trocar "tot"). Aparece no TSE como: CD_SITUACAO_CANDIDATO_TOT (2022). Coluna adicionada em 2022.
        /// </summary>
        [Column("codigo_situacao_candidato_tot")]
        public string? CodigoSituacaoCandidatoTot { get; set; }

        /// <summary>
        /// Código da situação de candidatura. Aparece no TSE como: COD_SITUACAO_CANDIDATURA (1994_BR), CD_SITUACAO_CANDIDATURA (1996).
        /// </summary>
        [Column("codigo_situacao_candidatura")]
        public string? CodigoSituacaoCandidatura { get; set; }

        /// <summary>
        /// . Aparece no TSE como: CD_SITUACAO_CANDIDATO_PLEITO (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("codigo_situacao_candidatura_pleito")]
        public string? CodigoSituacaoCandidatoPleito { get; set; }

        /// <summary>
        /// . Aparece no TSE como: CD_SITUACAO_CANDIDATO_URNA (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("codigo_situacao_candidatura_urna")]
        public string? CodigoSituacaoCandidatoUrna { get; set; }

        /// <summary>
        /// O indicativo de que o candidato está concorrendo ou não à reeleição - pode assumir os valores "S" (sim) e "N" (não) -apenas para os cargos de presidente, governador e prefeito é possível candidatura à reeleição. Aparece no TSE como: ST_REELEICAO (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("concorre_reeleicao")]
        public string? ConcorreReeleicao { get; set; }

        /// <summary>
        /// O indicativo de que o candidato tem ou não bens a declarar - pode assumiar os valores "S" (em caso positivo) e "N" (em caso negativo). Aparece no TSE como: ST_DECLARAR_BENS (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("declara_bens")]
        public string? DeclaraBens { get; set; }

        /// <summary>
        /// Despesa máxima de campanha declarada pelo partido para aquele cargo. Valores em Reais.. Aparece no TSE como: DESPESA_MAX_CAMPANHA (1994_BR), VR_DESPESA_MAX_CAMPANHA (1996).
        /// </summary>
        [Column("despesa_maxima_campanha")]
        public string? DespesaMaximaCampanha { get; set; }

        /// <summary>
        /// Descrição do detalhe da situação do registro de candidatura do candidato. Aparece no TSE como: DS_DETALHE_SITUACAO_CAND (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("detalhe_situacao_candidatura")]
        public string? DetalheSituacaoCandidatura { get; set; }

        /// <summary>
        /// Idade do candidato da data da eleição. Aparece no TSE como: IDADE_DATA_ELEICAO (1994_BR).
        /// </summary>
        [Column("idade_data_eleicao")]
        public string? IdadeDataEleicao { get; set; }

        /// <summary>
        /// Idade do candidato na data da posse (consultar a data de posse para cada cargo e unidade eleitoral nos arquivos de vagas). Aparece no TSE como: NR_IDADE_DATA_POSSE (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("idade_data_posse")]
        public string? IdadeDataPosse { get; set; }

        /// <summary>
        /// Nome de urna do candidato. Aparece no TSE como: NOME_URNA_CANDIDATO (1994_BR), NM_URNA_CANDIDATO (1996).
        /// </summary>
        [Column("nome_urna")]
        public string? NomeUrna { get; set; }

        /// <summary>
        /// Número do processo de registro de candidatura do candidato. Aparece no TSE como: NR_PROCESSO (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("numero_processo_candidatura")]
        public string? NumeroProcessoCandidatura { get; set; }

        /// <summary>
        /// Número do protocolo de registro de candidatura do candidato. Aparece no TSE como: NR_PROTOCOLO_CANDIDATURA (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("numero_protocolo_candidatura")]
        public string? NumeroProtocoloCandidatura { get; set; }

        /// <summary>
        /// Número sequencial do candidato gerado internamente pelos sistemas eleitorais. Não é o número de campanha do candidato.. Aparece no TSE como: SEQUENCIAL_CANDIDATO (1994_BR), SQ_CANDIDATO (1996).
        /// </summary>
        [Column("numero_sequencial")]
        public string? NumeroSequencial { get; set; }

        /// <summary>
        /// Número do candidato na urna. Aparece no TSE como: NUMERO_CANDIDATO (1994_BR), NR_CANDIDATO (1996).
        /// </summary>
        [Column("numero_urna")]
        public string? Numero { get; set; }

        /// <summary>
        /// ? (trocar "tot"). Aparece no TSE como: DS_SITUACAO_CANDIDATO_TOT (2022). Coluna adicionada em 2022.
        /// </summary>
        [Column("situacao_candidato_tot")]
        public string? SituacaoCandidatoTot { get; set; }

        /// <summary>
        /// Descrição da situação de candidatura. Aparece no TSE como: DES_SITUACAO_CANDIDATURA (1994_BR), DS_SITUACAO_CANDIDATURA (1996).
        /// </summary>
        [Column("situacao_candidatura")]
        public string? SituacaoCandidatura { get; set; }

        /// <summary>
        /// . Aparece no TSE como: DS_SITUACAO_CANDIDATO_PLEITO (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("situacao_candidatura_pleito")]
        public string? SituacaoCandidatoPleito { get; set; }

        /// <summary>
        /// . Aparece no TSE como: DS_SITUACAO_CANDIDATO_URNA (1996). Coluna adicionada em 1996.
        /// </summary>
        [Column("situacao_candidatura_urna")]
        public string? SituacaoCandidatoUrna { get; set; }
    }
}
