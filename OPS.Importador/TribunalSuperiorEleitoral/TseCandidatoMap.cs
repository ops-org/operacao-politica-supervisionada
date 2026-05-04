using CsvHelper.Configuration;
using OPS.Core.Utilities;

namespace OPS.Importador.TribunalSuperiorEleitoral
{
    public sealed class TseCandidatoMap : ClassMap<TseCandidato>
    {
        public TseCandidatoMap()
        {
            Map(m => m.Ano).Name("ano").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.CodigoTipoEleicao).Name("codigo_tipo_eleicao").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.NomeTipoEleicao).Name("tipo_eleicao").TypeConverter(new StringConverterCustom(StringConversionOptions.TitleCase));
            Map(m => m.Turno).Name("turno").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.CodigoEleicao).Name("codigo_eleicao").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Descricao).Name("eleicao").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.DataEleicao).Name("data_eleicao"); //.TypeConverter(new BrazilianDateTimeConverter());
            Map(m => m.TipoAbrangenciaEleicao).Name("tipo_abrangencia_eleicao").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.SiglaUnidadeFederativa).Name("sigla_unidade_federativa").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning));
            Map(m => m.SiglaUnidadeEleitoral).Name("sigla_unidade_eleitoral").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning));
            Map(m => m.UnidadeEleitoral).Name("unidade_eleitoral").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoCargo).Name("codigo_cargo").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Cargo).Name("cargo").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.NumeroSequencial).Name("numero_sequencial").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Numero).Name("numero_urna").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Nome).Name("nome").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.NomeUrna).Name("nome_urna").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.NomeSocial).Name("nome_social").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.Cpf).Name("cpf").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Email).Name("email").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.LowerCase)).Optional();
            Map(m => m.CodigoSituacaoCandidatura).Name("codigo_situacao_candidatura").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.SituacaoCandidatura).Name("situacao_candidatura").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoDetalheSituacaoCandidatura).Name("codigo_detalhe_situacao_candidatura").Optional();
            Map(m => m.DetalheSituacaoCandidatura).Name("detalhe_situacao_candidatura").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.TipoAgremiacao).TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Name("tipo_agremiacao");
            Map(m => m.NumeroPartido).Name("numero_partido").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.SiglaPartido).Name("sigla_partido");
            Map(m => m.Partido).Name("partido").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoLegenda).Name("codigo_legenda").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Legenda).Name("legenda").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.NumeroFederacao).Name("numero_federacao").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.Federacao).Name("federacao").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.SiglaFederacao).Name("sigla_federacao").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning)).Optional();
            Map(m => m.ComposicaoFederacao).Name("composicao_federacao").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.ComposicaoLegenda).Name("composicao_legenda").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning));
            Map(m => m.CodigoNacionalidade).Name("codigo_nacionalidade").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.Nacionalidade).Name("nacionalidade").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.SiglaUnidadeFederativaNascimento).Name("sigla_unidade_federativa_nascimento").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning));
            Map(m => m.CodigoMunicipioNascimento).Name("codigo_municipio_nascimento").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.MunicipioNascimento).Name("municipio_nascimento").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.DataNascimento).Name("data_nascimento").TypeConverter(new BrazilianDateTimeConverter());
            Map(m => m.IdadeDataPosse).Name("idade_data_posse").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.TituloEleitoral).Name("titulo_eleitoral").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.CodigoGenero).Name("codigo_genero").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Genero).Name("genero").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoGrauInstrucao).Name("codigo_grau_instrucao").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.GrauInstrucao).Name("grau_instrucao").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoEstadoCivil).Name("codigo_estado_civil").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.EstadoCivil).Name("estado_civil").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoEtnia).Name("codigo_etnia").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Etnia).Name("etnia").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.CodigoOcupacao).Name("codigo_ocupacao").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.Ocupacao).Name("ocupacao").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.DespesaMaximaCampanha).Name("despesa_maxima_campanha").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.CodigoTotalizacaoTurno).Name("codigo_totalizacao_turno").TypeConverter<NumberWithCleanerConverter>();
            Map(m => m.TotalizacaoTurno).Name("totalizacao_turno").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase));
            Map(m => m.ConcorreReeleicao).Name("concorre_reeleicao").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning)).Optional();
            Map(m => m.DeclaraBens).Name("declara_bens").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning)).Optional();
            Map(m => m.NumeroProtocoloCandidatura).Name("numero_protocolo_candidatura").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.NumeroProcessoCandidatura).Name("numero_processo_candidatura").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.CodigoSituacaoCandidatoPleito).Name("codigo_situacao_candidatura_pleito").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.SituacaoCandidatoPleito).Name("situacao_candidatura_pleito").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.CodigoSituacaoCandidatoUrna).Name("codigo_situacao_candidatura_urna").TypeConverter<NumberWithCleanerConverter>().Optional();
            Map(m => m.SituacaoCandidatoUrna).Name("situacao_candidatura_urna").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
            Map(m => m.CandidaturaInseridaUrna).Name("candidatura_inserida_urna").TypeConverter(new StringConverterCustom(StringConversionOptions.Cleaning | StringConversionOptions.TitleCase)).Optional();
        }
    }
}