using CsvHelper.Configuration;
using System.Globalization;

namespace OPS.Importador.TribunalSuperiorEleitoral
{
    public sealed class TseCandidatoMap : ClassMap<TseCandidato>
    {
        public TseCandidatoMap()
        {
            Map(m => m.Ano).Name("ANO_ELEICAO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.CodigoTipoEleicao).Name("CD_TIPO_ELEICAO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.NomeTipoEleicao).Name("NM_TIPO_ELEICAO").TypeConverter(new StringConverterCustom(StringConversionOptions.TitleCase));
            Map(m => m.Turno).Name("NR_TURNO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.CodigoEleicao).Name("CD_ELEICAO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Descricao).Name("DS_ELEICAO", "DESCRICAO_ELEICAO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.DataEleicao).Name("DT_ELEICAO").TypeConverter(new BrazilianDateTimeConverter());
            Map(m => m.TipoAbrangenciaEleicao).Name("TP_ABRANGENCIA").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.SiglaUnidadeFederativa).Name("SG_UF").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning));
            Map(m => m.SiglaUnidadeEleitoral).Name("SG_UE", "SIGLA_UE").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning));
            Map(m => m.UnidadeEleitoral).Name("NM_UE", "DESCRICAO_UE").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoCargo).Name("CD_CARGO", "CODIGO_CARGO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Cargo).Name("DS_CARGO", "DESCRICAO_CARGO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.NumeroSequencial).Name("SQ_CANDIDATO", "SEQUENCIAL_CANDIDATO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Numero).Name("NR_CANDIDATO", "NUMERO_CANDIDATO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Nome).Name("NM_CANDIDATO", "NOME_CANDIDATO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.NomeUrna).Name("NM_URNA_CANDIDATO", "NOME_URNA_CANDIDATO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.NomeSocial).Name("NM_SOCIAL_CANDIDATO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.Cpf).Name("NR_CPF_CANDIDATO", "CPF_CANDIDATO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Email).Name("NM_EMAIL").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.LowerCase)).Optional();
            Map(m => m.CodigoSituacaoCandidatura).Name("CD_SITUACAO_CANDIDATURA", "CODIGO_SITUACAO_CANDIDATURA").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.SituacaoCandidatura).Name("DS_SITUACAO_CANDIDATURA", "DESC_SITUACAO_CANDIDATURA").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoDetalheSituacaoCandidatura).Name("CD_DETALHE_SITUACAO_CAND").Optional();
            Map(m => m.DetalheSituacaoCandidatura).Name("DS_DETALHE_SITUACAO_CAND").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.TipoAgremiacao).TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Name("TP_AGREMIACAO");
            Map(m => m.NumeroPartido).Name("NR_PARTIDO", "NUMERO_PARTIDO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.SiglaPartido).Name("SG_PARTIDO", "SIGLA_PARTIDO");
            Map(m => m.Partido).Name("NM_PARTIDO", "NOME_PARTIDO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoLegenda).Name("SQ_COLIGACAO", "CODIGO_LEGENDA").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Legenda).Name("NM_COLIGACAO", "NOME_LEGENDA").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.NumeroFederacao).Name("NR_FEDERACAO", "NUMERO_FEDERACAO").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.Federacao).Name("NM_FEDERACAO", "NOME_FEDERACAO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.SiglaFederacao).Name("SG_FEDERACAO", "SIGLA_FEDERACAO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning)).Optional();
            Map(m => m.ComposicaoFederacao).Name("DS_COMPOSICAO_FEDERACAO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.ComposicaoLegenda).Name("DS_COMPOSICAO_COLIGACAO", "COMPOSICAO_LEGENDA").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning));
            Map(m => m.CodigoNacionalidade).Name("CD_NACIONALIDADE", "CODIGO_NACIONALIDADE").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.Nacionalidade).Name("DS_NACIONALIDADE", "DESCRICAO_NACIONALIDADE").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.SiglaUnidadeFederativaNascimento).Name("SG_UF_NASCIMENTO", "SIGLA_UF_NASCIMENTO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning));
            Map(m => m.CodigoMunicipioNascimento).Name("CD_MUNICIPIO_NASCIMENTO", "CODIGO_MUNICIPIO_NASCIMENTO").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.MunicipioNascimento).Name("NM_MUNICIPIO_NASCIMENTO", "NOME_MUNICIPIO_NASCIMENTO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.DataNascimento).Name("DT_NASCIMENTO", "DATA_NASCIMENTO").TypeConverter(new BrazilianDateTimeConverter());
            Map(m => m.IdadeDataPosse).Name("NR_IDADE_DATA_POSSE").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.TituloEleitoral).Name("NR_TITULO_ELEITORAL_CANDIDATO", "NUM_TITULO_ELEITORAL_CANDIDATO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.CodigoGenero).Name("CD_GENERO", "CODIGO_SEXO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Genero).Name("DS_GENERO", "DESCRICAO_SEXO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoGrauInstrucao).Name("CD_GRAU_INSTRUCAO", "CODIGO_GRAU_INSTRUCAO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.GrauInstrucao).Name("DS_GRAU_INSTRUCAO", "DESCRICAO_GRAU_INSTRUCAO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoEstadoCivil).Name("CD_ESTADO_CIVIL", "CODIGO_ESTADO_CIVIL").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.EstadoCivil).Name("DS_ESTADO_CIVIL", "DESCRICAO_ESTADO_CIVIL").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoEtnia).Name("CD_COR_RACA", "CODIGO_ETNIA").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Etnia).Name("DS_COR_RACA", "DESCRICAO_ETNIA").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoOcupacao).Name("CD_OCUPACAO", "CODIGO_OCUPACAO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Ocupacao).Name("DS_OCUPACAO", "DESCRICAO_OCUPACAO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.DespesaMaximaCampanha).Name("VR_DESPESA_MAX_CAMPANHA", "DESPESA_MAX_CAMPANHA").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.CodigoTotalizacaoTurno).Name("CD_SIT_TOT_TURNO", "COD_SIT_TOT_TURNO").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.TotalizacaoTurno).Name("DS_SIT_TOT_TURNO", "DESC_SIT_TOT_TURNO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.ConcorreReeleicao).Name("ST_REELEICAO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning)).Optional();
            Map(m => m.DeclaraBens).Name("ST_DECLARAR_BENS").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning)).Optional();
            Map(m => m.NumeroProtocoloCandidatura).Name("NR_PROTOCOLO_CANDIDATURA", "NUM_PROTOCOLO_CANDIDATURA").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.NumeroProcessoCandidatura).Name("NR_PROCESSO", "NUM_PROCESSO").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.CodigoSituacaoCandidatoPleito).Name("CD_SITUACAO_CANDIDATO_PLEITO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning)).Optional();
            Map(m => m.SituacaoCandidatoPleito).Name("DS_SITUACAO_CANDIDATO_PLEITO").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.CodigoSituacaoCandidatoUrna).Name("CD_SITUACAO_CANDIDATO_URNA").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.SituacaoCandidatoUrna).Name("DS_SITUACAO_CANDIDATO_URNA").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.CandidaturaInseridaUrna).Name("ST_CANDIDATO_INSERIDO_URNA").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
        }
    }
}