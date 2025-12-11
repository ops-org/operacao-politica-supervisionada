using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Comum;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Assembleias.Parlamentar;

namespace OPS.Importador.Assembleias;

/// <summary>
/// Assembleia Legislativa do Estado de Minas Gerais
/// https://dadosabertos.almg.gov.br/
/// </summary>
public class MinasGerais : ImportadorBase
{
    public MinasGerais(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarMinasGerais(serviceProvider);
        importadorDespesas = new ImportadorDespesasMinasGerais(serviceProvider);
    }
}

public class ImportadorDespesasMinasGerais : ImportadorDespesasArquivo
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasMinasGerais(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://dadosabertos.almg.gov.br/",
            Estado = Estado.MinasGerais,
            ChaveImportacao = ChaveDespesaTemp.Matricula
        };
    }

    /// <summary>
    /// https://dadosabertos.almg.gov.br/api/ajuda/swagger/view/lastest#/
    /// </summary>
    /// <param name="context"></param>
    /// <param name="ano"></param>
    public override void Importar(int ano)
    {
        using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano }))
        {
            //// TODO: Criar importação por legislatura
            //if (ano != 2023)
            //{
            //    logger.LogWarning("Importação já realizada para o ano de {Ano}", ano);
            //    throw new BusinessException($"Importação já realizada para o ano de {ano}");
            //}

            CarregarHashes(ano);

            using (var db = new AppDb())
            {
                var dc = db.ExecuteDict($"select id, matricula, nome_parlamentar from cl_deputado where id_estado = {idEstado}");
                foreach (var item in dc)
                {
                    var matricula = item["matricula"].ToString();
                    if (string.IsNullOrEmpty(matricula)) continue;

                    var address = $"{config.BaseAddress}api/v2/prestacao_contas/verbas_indenizatorias/deputados/{matricula}/datas?formato=json";
                    ListaFechamentoVerbaDatas resDiarias = RestApiGet<ListaFechamentoVerbaDatas>(address);

                    foreach (ListaFechamentoVerba data in resDiarias.ListaFechamentoVerba)
                    {
                        var dataReferencia = Convert.ToDateTime(data.DataReferencia);
                        //if (dataReferencia.Year < ano) continue; // TODO: Ajustar para importar do mandato
                        if (dataReferencia.Year != ano) continue; // TODO: Ajustar para importar do mandato

                        ListaMensalDespesasMG despesasMensais;
                        address = $"{config.BaseAddress}api/v2/prestacao_contas/verbas_indenizatorias/deputados/{matricula}/{dataReferencia.Year}/{dataReferencia.Month}?formato=json";
                        try
                        {
                            despesasMensais = RestApiGetWithSqlTimestampConverter<ListaMensalDespesasMG>(address);
                        }
                        catch (HttpRequestException ex)
                        {
                            if (ex.Message != "Response status code does not indicate success: 429 (Too Many Requests).")
                                throw;

                            Thread.Sleep(1000);
                            despesasMensais = RestApiGetWithSqlTimestampConverter<ListaMensalDespesasMG>(address);
                        }

                        var despesas = despesasMensais.List.SelectMany(x => x.ListaDetalheVerba);

                        var sqlFields = new StringBuilder();
                        var sqlValues = new StringBuilder();

                        foreach (ListaDetalheVerba despesa in despesas)
                        {
                            var despesaTemp = new CamaraEstadualDespesaTemp()
                            {
                                Ano = (short)dataReferencia.Year,
                                Mes = (short)dataReferencia.Month,
                                Documento = despesa.DescDocumento,
                                DataEmissao = despesa.DataEmissao,
                                Cpf = matricula,
                                Nome = item["nome_parlamentar"].ToString().ToTitleCase(),
                                TipoDespesa = despesa.DescTipoDespesa,
                                CnpjCpf = despesa.CpfCnpj,
                                Empresa = despesa.NomeEmitente,
                                Valor = Convert.ToDecimal(despesa.ValorReembolsado, cultureInfo)
                            };

                            InserirDespesaTemp(despesaTemp);
                        }
                    }
                }

            }

            ProcessarDespesas(ano);
        }
    }

    //public override void DefinirCompetencias(int ano)
    //{
    //    competenciaInicial = $"{ano}01";
    //    competenciaFinal = $"{ano + 4}12";
    //}

    public override void ImportarDespesas(string caminhoArquivo, int ano)
    {
        throw new NotImplementedException();
    }

    public class ListaFechamentoVerba
    {
        [JsonPropertyName("idDeputado")]
        public int IdDeputado { get; set; }

        [JsonPropertyName("dataReferencia")]
        public string DataReferencia { get; set; }
    }

    public class ListaFechamentoVerbaDatas
    {
        [JsonPropertyName("listaFechamentoVerba")]
        public List<ListaFechamentoVerba> ListaFechamentoVerba { get; set; }
    }

    public class List
    {
        [JsonPropertyName("idDeputado")]
        public int IdDeputado { get; set; }

        [JsonPropertyName("dataReferencia")]
        public DateTime DataReferencia { get; set; }

        [JsonPropertyName("codTipoDespesa")]
        public int CodTipoDespesa { get; set; }

        [JsonPropertyName("valor")]
        public double Valor { get; set; }

        [JsonPropertyName("descTipoDespesa")]
        public string DescTipoDespesa { get; set; }

        [JsonPropertyName("listaDetalheVerba")]
        public List<ListaDetalheVerba> ListaDetalheVerba { get; set; }
    }

    public class ListaDetalheVerba
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("idDeputado")]
        public int IdDeputado { get; set; }

        [JsonPropertyName("dataReferencia")]
        public DateTime DataReferencia { get; set; }

        [JsonPropertyName("valorReembolsado")]
        public double ValorReembolsado { get; set; }

        [JsonPropertyName("dataEmissao")]
        public DateTime DataEmissao { get; set; }

        [JsonPropertyName("cpfCnpj")]
        public string CpfCnpj { get; set; }

        [JsonPropertyName("valorDespesa")]
        public double ValorDespesa { get; set; }

        [JsonPropertyName("nomeEmitente")]
        public string NomeEmitente { get; set; }

        [JsonPropertyName("descDocumento")]
        public string DescDocumento { get; set; }

        [JsonPropertyName("codTipoDespesa")]
        public int CodTipoDespesa { get; set; }

        [JsonPropertyName("descTipoDespesa")]
        public string DescTipoDespesa { get; set; }
    }

    public class ListaMensalDespesasMG
    {
        [JsonPropertyName("list")]
        public List<List> List { get; set; }
    }
}

public class ImportadorParlamentarMinasGerais : ImportadorParlamentarBase
{

    public ImportadorParlamentarMinasGerais(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        // https://dadosabertos.almg.gov.br/api/v2ajuda/sobre

        Configure(new ImportadorParlamentarConfig()
        {
            BaseAddress = "http://dadosabertos.almg.gov.br/",
            Estado = Estado.MinasGerais,
        });
    }

    public override Task Importar()
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));
        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        // 1=em exercício, 2=exerceu mandato, 3=renunciou, 4=afastado, 5=perdeu mandato
        foreach (var situacao in new[] { 1, 2, 3, 4, 5 })
        {
            DeputadoListMG deputados;
            try
            {
                deputados = RestApiGetWithSqlTimestampConverter<DeputadoListMG>(@$"{config.BaseAddress}api/v2/deputados/situacao/{situacao}?formato=json");
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message != "Response status code does not indicate success: 429 (Too Many Requests).")
                    throw;

                Thread.Sleep(1000);
                deputados = RestApiGetWithSqlTimestampConverter<DeputadoListMG>(@$"{config.BaseAddress}api/v2/deputados/situacao/{situacao}?formato=json");
            }

            foreach (DeputadoMG deputado in deputados.List)
            {
                var matricula = deputado.Id;
                var deputadoDb = GetDeputadoByMatriculaOrNew((uint)matricula);

                deputadoDb.IdPartido = BuscarIdPartido(deputado.Partido);
                deputadoDb.NomeParlamentar = deputado.Nome.ToTitleCase();
                deputadoDb.UrlPerfil = $"https://www.almg.gov.br/deputados/conheca_deputados/deputados-info.html?idDep={deputadoDb.Matricula}&leg=20";

                //Thread.Sleep(TimeSpan.FromSeconds(1));
                var detalhes = RestApiGetWithSqlTimestampConverter<DeputadoDetalhesMG>(@$"{config.BaseAddress}api/v2/deputados/{deputadoDb.Matricula}?formato=json");

                deputadoDb.NomeCivil = detalhes.Deputado.NomeServidor.ToTitleCase();
                deputadoDb.Naturalidade = detalhes.Deputado.NaturalidadeMunicipio;
                if (detalhes.Deputado.DataNascimento != null)
                    deputadoDb.Nascimento = DateOnly.Parse(detalhes.Deputado.DataNascimento, cultureInfo);
                deputadoDb.Profissao = detalhes.Deputado.AtividadeProfissional.ToTitleCase();
                deputadoDb.Sexo = detalhes.Deputado.Sexo;

                InsertOrUpdate(deputadoDb);
            }
        }

        logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", registrosInseridos, registrosAtualizados);
        return Task.CompletedTask;
    }

    public class DeputadoMG
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("partido")]
        public string Partido { get; set; }

        [JsonPropertyName("tagLocalizacao")]
        public int TagLocalizacao { get; set; }
    }

    public class DeputadoListMG
    {
        [JsonPropertyName("list")]
        public List<DeputadoMG> List { get; set; }
    }


    public class Candidatura
    {
        [JsonPropertyName("legislatura")]
        public int Legislatura { get; set; }

        [JsonPropertyName("numeroCandidato")]
        public int NumeroCandidato { get; set; }
    }

    public class Deputado
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("partido")]
        public string Partido { get; set; }

        [JsonPropertyName("tagLocalizacao")]
        public int TagLocalizacao { get; set; }

        [JsonPropertyName("sexo")]
        public string Sexo { get; set; }

        [JsonPropertyName("situacao")]
        public string Situacao { get; set; }

        [JsonPropertyName("inicioSituacao")]
        public DateTime InicioSituacao { get; set; }

        [JsonPropertyName("tipoMandato")]
        public string TipoMandato { get; set; }

        [JsonPropertyName("nomeServidor")]
        public string NomeServidor { get; set; }

        [JsonPropertyName("naturalidadeMunicipio")]
        public string NaturalidadeMunicipio { get; set; }

        [JsonPropertyName("naturalidadeUf")]
        public string NaturalidadeUf { get; set; }

        [JsonPropertyName("dataNascimento")]
        public string DataNascimento { get; set; }

        [JsonPropertyName("apresentarEmail")]
        public string ApresentarEmail { get; set; }

        [JsonPropertyName("atividadeProfissional")]
        public string AtividadeProfissional { get; set; }

        [JsonPropertyName("codigoSituacao")]
        public int CodigoSituacao { get; set; }

        [JsonPropertyName("emails")]
        public List<Email> Emails { get; set; }

        [JsonPropertyName("redesSociais")]
        public List<RedesSociai> RedesSociais { get; set; }

        [JsonPropertyName("vidaProfissionalPolitica")]
        public string VidaProfissionalPolitica { get; set; }

        [JsonPropertyName("atuacaoParlamentar")]
        public string AtuacaoParlamentar { get; set; }

        [JsonPropertyName("condecoracao")]
        public string Condecoracao { get; set; }

        [JsonPropertyName("candidaturas")]
        public List<Candidatura> Candidaturas { get; set; }

        [JsonPropertyName("filiacoes")]
        public List<Filiaco> Filiacoes { get; set; }

        [JsonPropertyName("enderecos")]
        public List<Endereco> Enderecos { get; set; }

        [JsonPropertyName("legislaturas")]
        public List<Legislatura> Legislaturas { get; set; }

        [JsonPropertyName("situacaoLegislaturas")]
        public string SituacaoLegislaturas { get; set; }

        [JsonPropertyName("partidosLegislatura")]
        public string PartidosLegislatura { get; set; }

        [JsonPropertyName("anoEleicaoLegislatura")]
        public string AnoEleicaoLegislatura { get; set; }

        [JsonPropertyName("situacaoTerminoLegislatura")]
        public string SituacaoTerminoLegislatura { get; set; }
    }

    public class Email
    {
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; }

        [JsonPropertyName("endereco")]
        public string Endereco { get; set; }
    }

    public class Endereco
    {
        [JsonPropertyName("logradouro")]
        public string Logradouro { get; set; }

        [JsonPropertyName("numero")]
        public string Numero { get; set; }

        [JsonPropertyName("complemento")]
        public string Complemento { get; set; }

        [JsonPropertyName("bairro")]
        public string Bairro { get; set; }

        [JsonPropertyName("cep")]
        public string Cep { get; set; }

        [JsonPropertyName("municipio")]
        public Municipio Municipio { get; set; }

        [JsonPropertyName("descTipo")]
        public string DescTipo { get; set; }

        [JsonPropertyName("telefones")]
        public List<Telefone> Telefones { get; set; }
    }

    public class Filiaco
    {
        [JsonPropertyName("dataInicio")]
        public DateTime DataInicio { get; set; }

        [JsonPropertyName("dataTermino")]
        public DateTime DataTermino { get; set; }

        [JsonPropertyName("partido")]
        public Partido Partido { get; set; }
    }

    public class Legislatura
    {
        [JsonPropertyName("numeroLegislatura")]
        public int NumeroLegislatura { get; set; }

        [JsonPropertyName("inicioLegislatura")]
        public DateTime InicioLegislatura { get; set; }

        [JsonPropertyName("terminoLegislatura")]
        public DateTime TerminoLegislatura { get; set; }

        [JsonPropertyName("tipoMandato")]
        public string TipoMandato { get; set; }

        [JsonPropertyName("situacoes")]
        public List<Situaco> Situacoes { get; set; }

        [JsonPropertyName("inicioExercicio")]
        public DateTime InicioExercicio { get; set; }

        [JsonPropertyName("terminoExercicio")]
        public DateTime TerminoExercicio { get; set; }

        [JsonPropertyName("deputadoAfastado")]
        public string DeputadoAfastado { get; set; }
    }

    public class Municipio
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("uf")]
        public string Uf { get; set; }

        [JsonPropertyName("codigoIbge9")]
        public int CodigoIbge9 { get; set; }
    }

    public class Partido
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("sigla")]
        public string Sigla { get; set; }
    }

    public class RedeSocial
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class RedesSociai
    {
        [JsonPropertyName("redeSocial")]
        public RedeSocial RedeSocial { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }

    public class DeputadoDetalhesMG
    {
        [JsonPropertyName("deputado")]
        public Deputado Deputado { get; set; }
    }

    public class Situaco
    {
        [JsonPropertyName("dataInicio")]
        public DateTime DataInicio { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("dataTermino")]
        public DateTime DataTermino { get; set; }
    }

    public class Telefone
    {
        [JsonPropertyName("ddd")]
        public string Ddd { get; set; }

        [JsonPropertyName("numero")]
        public string Numero { get; set; }

        [JsonPropertyName("fax")]
        public string Fax { get; set; }
    }


}