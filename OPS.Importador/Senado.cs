using CsvHelper;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonArray = System.Text.Json.Nodes.JsonArray;

namespace OPS.Importador
{
    /// <summary>
    /// Senado Federal
    /// https://www12.senado.leg.br/dados-abertos
    /// </summary>
    public class Senado : ImportadorCotaParlamentarBase
    {
        public Senado(ILogger<Senado> logger, IConfiguration configuration, IDbConnection connection) :
            base("SF", logger, configuration, connection)
        {
        }

        public override void ImportarParlamentares()
        {
            using (var banco = new AppDb())
            {
                var lstSenadorAtivo = new List<string>();
                var lstSenador = new List<string>();
                using (var reader = banco.ExecuteReader("SELECT id FROM sf_senador where ativo='S' order by id"))
                    while (reader.Read())
                        lstSenador.Add(reader["id"].ToString());

                var lstSenadorNovo = new List<string>();
                using (var reader = banco.ExecuteReader("SELECT id FROM sf_senador order by id"))
                    while (reader.Read())
                        lstSenadorNovo.Add(reader["id"].ToString());

                banco.ExecuteNonQuery("UPDATE sf_senador SET ativo = 'N' WHERE ativo = 'S'");

                var restClient = new RestClient();
                restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                #region Importar senadores ativos
                var request = new RestRequest("http://legis.senado.gov.br/dadosabertos/senador/lista/atual", Method.GET);
                request.AddHeader("Accept", "application/json");

                IRestResponse resSenadores = restClient.ExecuteWithAutoRetry(request);
                JsonDocument jSenadores = JsonDocument.Parse(resSenadores.Content);
                JsonElement arrIdentificacaoParlamentar = jSenadores.RootElement.GetProperty("ListaParlamentarEmExercicio").GetProperty("Parlamentares").GetProperty("Parlamentar");
                var senadores = arrIdentificacaoParlamentar.EnumerateArray();

                while (senadores.MoveNext())
                {
                    try
                    {
                        var senador = senadores.Current;
                        var parlamentar = JsonSerializer.Deserialize<IdentificacaoParlamentar>(senador.GetProperty("IdentificacaoParlamentar"));

                        banco.ClearParameters();
                        banco.AddParameter("id", parlamentar.CodigoParlamentar);

                        var existe = banco.ExecuteScalar("SELECT 1 FROM sf_senador WHERE id = @id");
                        if (existe == null)
                        {
                            logger.LogInformation("Novo Senador {IdSenador}: {NomeParlamentar}", parlamentar.CodigoParlamentar, parlamentar.NomeParlamentar);

                            banco.AddParameter("id", parlamentar.CodigoParlamentar);
                            banco.AddParameter("codigo", parlamentar.CodigoPublicoNaLegAtual);
                            banco.AddParameter("nome", parlamentar.NomeParlamentar);
                            banco.AddParameter("nome_completo", parlamentar.NomeCompletoParlamentar);
                            banco.AddParameter("sexo", parlamentar.SexoParlamentar[0].ToString());
                            banco.AddParameter("sigla_partido", parlamentar.SiglaPartidoParlamentar);
                            banco.AddParameter("sigla_uf", parlamentar.UfParlamentar);
                            banco.AddParameter("email", parlamentar.EmailParlamentar);
                            banco.AddParameter("site", parlamentar.UrlPaginaParticular);

                            banco.ExecuteNonQuery(@"
                                INSERT INTO sf_senador (
                	                id
                                    , codigo
                                    , nome
                                    , nome_completo
                                    , sexo
                                    , id_partido
                                    , id_estado
                                    , email
                                    , site
                                    , ativo
                                ) VALUES (
                	                @id
                                    , @codigo
                                    , @nome
                                    , @nome_completo
                                    , @sexo
                                    , (SELECT id FROM partido where sigla like @sigla_partido OR nome like @sigla_partido)
                                    , (SELECT id FROM estado where sigla like @sigla_uf)
                                    , @email
                                    , @site
                                    , 'S'
                                )
                            ");

                            lstSenador.Add(parlamentar.CodigoParlamentar);
                        }
                        else
                        {
                            lstSenadorAtivo.Add(parlamentar.CodigoParlamentar);

                            if (!lstSenador.Contains(parlamentar.CodigoParlamentar))
                            {
                                lstSenador.Add(parlamentar.CodigoParlamentar);
                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, ex.Message);
                    }
                }

                #endregion Importar senadores ativos

                #region Atualizar senadores ativos e os que estavam ativos antes da importação

                var lstLegislatura = new List<string>();
                using (var reader = banco.ExecuteReader("SELECT id FROM sf_legislatura"))
                    while (reader.Read())
                        lstLegislatura.Add(reader["id"].ToString());

                var lstMotivoAfastamento = new List<string>();
                using (var reader = banco.ExecuteReader("SELECT id FROM sf_motivo_afastamento"))
                    while (reader.Read())
                        lstMotivoAfastamento.Add(reader["id"].ToString());

                var lstProfissao = new Dictionary<string, int>();
                using (var reader = banco.ExecuteReader("SELECT id, descricao FROM profissao"))
                    while (reader.Read())
                        lstProfissao.Add(reader["descricao"].ToString(), Convert.ToInt32(reader["id"]));

                var indice = 0;
                var totalSenadores = lstSenador.Count();
                foreach (var idSenador in lstSenador)
                {
                    request = new RestRequest("http://legis.senado.gov.br/dadosabertos/senador/" + idSenador, Method.GET);
                    request.AddHeader("Accept", "application/json");

                    var resSenador = restClient.ExecuteWithAutoRetry(request);
                    JsonDocument jSenador = JsonDocument.Parse(resSenador.Content);
                    Senador senador = JsonSerializer.Deserialize<Senador>(resSenador.Content);
                    var parlamentar = senador.DetalheParlamentar.Parlamentar;
                    var identificacaoParlamentar = parlamentar.IdentificacaoParlamentar;
                    var jParlamentar = jSenador.RootElement.GetProperty("DetalheParlamentar").GetProperty("Parlamentar");

                    logger.LogDebug("{IndiceAtual}/{Total} Consultando Senador {IdSenador}: {NomeParlamentar}",
                        ++indice, totalSenadores, identificacaoParlamentar.CodigoParlamentar, identificacaoParlamentar.NomeParlamentar);

                    JsonElement jTelefones;
                    if (jParlamentar.TryGetProperty("Telefones", out jTelefones))
                        if (jTelefones.GetProperty("Telefone").ValueKind == JsonValueKind.Array)
                        {
                            parlamentar.Telefones = JsonSerializer.Deserialize<Telefones>(jTelefones);
                        }
                        else
                        {
                            parlamentar.Telefones = new Telefones();
                            parlamentar.Telefones.Telefone = new Telefone[1];
                            parlamentar.Telefones.Telefone[0] = JsonSerializer.Deserialize<Telefone>(jTelefones.GetProperty("Telefone"));
                        }

                    if (string.IsNullOrEmpty(identificacaoParlamentar.NomeParlamentar))
                    {
                        identificacaoParlamentar.NomeParlamentar = identificacaoParlamentar.NomeCompletoParlamentar;
                    }

                    banco.AddParameter("id", identificacaoParlamentar.CodigoParlamentar);
                    banco.AddParameter("codigo", identificacaoParlamentar.CodigoPublicoNaLegAtual);
                    banco.AddParameter("nome", identificacaoParlamentar.NomeParlamentar);
                    banco.AddParameter("nome_completo", identificacaoParlamentar.NomeCompletoParlamentar);
                    banco.AddParameter("sexo", identificacaoParlamentar.SexoParlamentar[0].ToString());
                    banco.AddParameter("sigla_partido", identificacaoParlamentar.SiglaPartidoParlamentar);
                    banco.AddParameter("sigla_uf", identificacaoParlamentar.UfParlamentar);
                    banco.AddParameter("email", identificacaoParlamentar.EmailParlamentar);
                    banco.AddParameter("site", identificacaoParlamentar.UrlPaginaParticular);

                    var dadosBasicos = senador.DetalheParlamentar.Parlamentar.DadosBasicosParlamentar;
                    banco.AddParameter("nascimento", dadosBasicos?.DataNascimento);
                    banco.AddParameter("naturalidade", dadosBasicos?.Naturalidade);
                    banco.AddParameter("sigla_uf_naturalidade", dadosBasicos?.UfNaturalidade);

                    request = new RestRequest("http://legis.senado.gov.br/dadosabertos/senador/" + idSenador.ToString() + "/profissao?v=1", Method.GET);
                    request.AddHeader("Accept", "application/json");

                    IRestResponse resProfissoes = restClient.ExecuteWithAutoRetry(request);
                    JsonDocument jSenadorProfissoes = JsonDocument.Parse(resProfissoes.Content);
                    var jParlamentarProfissoes = jSenadorProfissoes.RootElement.GetProperty("ProfissaoParlamentar").GetProperty("Parlamentar");

                    var lstSenadorProfissao = new List<Profissao>();
                    JsonElement jProfissoes;
                    if (jParlamentarProfissoes.TryGetProperty("Profissoes", out jProfissoes))
                    {
                        if (jProfissoes.GetProperty("Profissao").ValueKind == JsonValueKind.Array)
                        {
                            lstSenadorProfissao = JsonSerializer.Deserialize<List<Profissao>>(jProfissoes.GetProperty("Profissao"));
                        }
                        else
                        {
                            lstSenadorProfissao.Add(JsonSerializer.Deserialize<Profissao>(jProfissoes.GetProperty("Profissao")));
                        }
                    }

                    if (lstSenadorProfissao.Any())
                    {
                        banco.AddParameter("profissao", string.Join(", ", lstSenadorProfissao.Select(obj => obj.NomeProfissao)));
                    }
                    else
                    {
                        banco.AddParameter("profissao", DBNull.Value);
                    }

                    banco.AddParameter("ativo", lstSenadorAtivo.Contains(identificacaoParlamentar.CodigoParlamentar) ? "S" : "N");

                    banco.ExecuteNonQuery(@"
                        UPDATE sf_senador SET
                            codigo = @codigo
                            , nome = @nome
                            , nome_completo = @nome_completo
                            , sexo = @sexo
                            , nascimento = @nascimento
                            , naturalidade = @naturalidade
                            , id_estado_naturalidade = (SELECT id FROM estado where sigla like @sigla_uf_naturalidade)
                            , profissao = @profissao
                            , id_partido = (SELECT id FROM partido where sigla like @sigla_partido OR nome like @sigla_partido)
                            , id_estado = (SELECT id FROM estado where sigla like @sigla_uf)
                            , email = @email
                            , site = @site
                            , ativo = @ativo
                        WHERE id = @id
                    ");

                    if (lstSenadorProfissao.Any())
                    {
                        foreach (var profissao in lstSenadorProfissao)
                        {
                            if (!lstProfissao.ContainsKey(profissao.NomeProfissao))
                            {
                                banco.AddParameter("descricao", profissao.NomeProfissao);
                                var idProfissao = banco.ExecuteScalar(@"INSERT INTO profissao (descricao) values (@descricao); SELECT LAST_INSERT_ID();");

                                lstProfissao.Add(profissao.NomeProfissao, Convert.ToInt32(idProfissao));
                            }

                            banco.AddParameter("id_sf_senador", idSenador);
                            banco.AddParameter("id_profissao", lstProfissao[profissao.NomeProfissao]);

                            banco.ExecuteNonQuery(@"INSERT IGNORE INTO sf_senador_profissao (id_sf_senador, id_profissao) values (@id_sf_senador, @id_profissao)");
                        }
                    }

                    request = new RestRequest("http://legis.senado.gov.br/dadosabertos/senador/" + idSenador.ToString() + "/historicoAcademico?v=1", Method.GET);
                    request.AddHeader("Accept", "application/json");

                    IRestResponse resHistoricoAcademico = restClient.ExecuteWithAutoRetry(request);
                    JsonDocument jSenadorHistoricoAcademico = JsonDocument.Parse(resHistoricoAcademico.Content);
                    var jHistoricoAcademico = jSenadorHistoricoAcademico.RootElement.GetProperty("HistoricoAcademicoParlamentar").GetProperty("Parlamentar");

                    var lstCursos = new List<SenadorCurso>();
                    JsonElement jCursos;
                    if (jHistoricoAcademico.TryGetProperty("HistoricoAcademico", out jCursos))
                    {
                        if (jCursos.GetProperty("Curso").ValueKind == JsonValueKind.Array)
                        {
                            lstCursos = JsonSerializer.Deserialize<List<SenadorCurso>>(jCursos.GetProperty("Curso"));
                        }
                        else
                        {
                            lstCursos.Add(JsonSerializer.Deserialize<SenadorCurso>(jCursos.GetProperty("Curso")));
                        }

                        if (lstCursos != null)
                        {
                            foreach (var curso in lstCursos)
                            {
                                banco.AddParameter("id_sf_senador", idSenador);
                                banco.AddParameter("nome_curso", curso.NomeCurso);
                                banco.AddParameter("grau_instrucao", curso.GrauInstrucao);
                                banco.AddParameter("estabelecimento", curso.Estabelecimento);
                                banco.AddParameter("local", curso.Local);

                                banco.ExecuteNonQuery(@"
                                INSERT IGNORE INTO sf_senador_historico_academico (id_sf_senador, nome_curso, grau_instrucao, estabelecimento, local) 
                                values (@id_sf_senador, @nome_curso, @grau_instrucao, @estabelecimento, @local)");
                            }
                        }
                    }

                    request = new RestRequest("http://legis.senado.gov.br/dadosabertos/senador/" + idSenador.ToString() + "/mandatos?v=5", Method.GET);
                    request.AddHeader("Accept", "application/json");

                    IRestResponse resSenadorMandato = restClient.ExecuteWithAutoRetry(request);
                    var senadorMandato = JsonSerializer.Deserialize<SenadorMandato>(resSenadorMandato.Content);
                    var arrPalamentar = senadorMandato.MandatoParlamentar.Parlamentar;
                    var arrMandatosPalamentar = arrPalamentar.Mandatos.Mandato;

                    if (arrMandatosPalamentar != null)
                    {
                        logger.LogDebug("Consultando {TotalMandatos} mandato(s)", arrMandatosPalamentar.Count());

                        foreach (var mandato in arrMandatosPalamentar)
                        {
                            //Ignorar mandatos muito antigos
                            if (mandato.CodigoMandato == null) continue;

                            try
                            {
                                #region Mandato
                                banco.AddParameter("id", mandato.CodigoMandato);
                                banco.AddParameter("id_sf_senador", idSenador);
                                banco.AddParameter("sigla_uf", mandato.UfParlamentar);
                                banco.AddParameter("participacao", mandato.DescricaoParticipacao[0].ToString());
                                banco.AddParameter("exerceu", mandato.Exercicios != null);

                                banco.ExecuteNonQuery(@"
                                         INSERT IGNORE INTO sf_mandato (
                                             id, id_sf_senador, id_estado, participacao, exerceu
                                        ) VALUES (
                                             @id, @id_sf_senador, (SELECT id FROM estado where sigla like @sigla_uf), @participacao, @exerceu
                                         )
                                     ");

                                if (mandato.DescricaoParticipacao == "Titular")
                                {
                                    if (mandato.Suplentes != null && mandato.Suplentes.Suplente.Any(obj => obj.DescricaoParticipacao == "1º Suplente"))
                                    {
                                        var id = mandato.Suplentes.Suplente.FirstOrDefault(obj => obj.DescricaoParticipacao == "1º Suplente").CodigoParlamentar;
                                        if (!lstSenadorNovo.Contains(id))
                                        {
                                            lstSenadorNovo.Add(id);

                                            banco.AddParameter("id", id);
                                            banco.ExecuteNonQuery(@"INSERT INTO sf_senador (id) VALUES (@id)");
                                        }

                                        // TODO: Inserir o senador se não existir e importar os detalhes do mandato
                                    }

                                    if (mandato.Suplentes != null && mandato.Suplentes.Suplente.Any(obj => obj.DescricaoParticipacao == "2º Suplente"))
                                    {
                                        var id = mandato.Suplentes.Suplente.FirstOrDefault(obj => obj.DescricaoParticipacao == "2º Suplente").CodigoParlamentar;
                                        if (!lstSenadorNovo.Contains(id))
                                        {
                                            lstSenadorNovo.Add(id);

                                            banco.AddParameter("id", id);
                                            banco.ExecuteNonQuery(@"INSERT INTO sf_senador(id) VALUES (@id)");
                                        }

                                        // TODO: Inserir o senador se não existir e importar os detalhes do mandato
                                    }
                                }
                                #endregion Mandato

                                if (mandato.DescricaoParticipacao == "Titular")
                                {
                                    #region Mandato Legislatura

                                    var lstLegislaturaMandato = new List<LegislaturaDoMandato>();
                                    lstLegislaturaMandato.Add(mandato.PrimeiraLegislaturaDoMandato);
                                    lstLegislaturaMandato.Add(mandato.SegundaLegislaturaDoMandato);

                                    foreach (var legislatura in lstLegislaturaMandato)
                                    {
                                        if (!lstLegislatura.Contains(legislatura.NumeroLegislatura))
                                        {
                                            banco.AddParameter("id", legislatura.NumeroLegislatura);
                                            banco.AddParameter("inicio", legislatura.DataInicio);
                                            banco.AddParameter("final", legislatura.DataFim);
                                            banco.ExecuteNonQuery(@"
                                        INSERT INTO sf_legislatura (
									        id, inicio, final
								        ) VALUES (
                                            @id, @inicio, @final
                                        )
                                    ");

                                            lstLegislatura.Add(legislatura.NumeroLegislatura);
                                        }

                                        banco.AddParameter("id_sf_mandato", mandato.CodigoMandato);
                                        banco.AddParameter("id_sf_legislatura", legislatura.NumeroLegislatura);
                                        banco.ExecuteNonQuery(@"
                                    INSERT IGNORE INTO sf_mandato_legislatura (
									    id_sf_mandato, id_sf_legislatura
								    ) VALUES (
                                        @id_sf_mandato, @id_sf_legislatura
                                    )
                                ");
                                    }
                                    #endregion Mandato Legislatura
                                }

                                #region Mandato Exercicio
                                if (mandato.Exercicios != null)
                                {
                                    foreach (var exercicio in mandato.Exercicios.Exercicio)
                                    {
                                        if (exercicio.SiglaCausaAfastamento != null)
                                        {
                                            exercicio.SiglaCausaAfastamento = exercicio.SiglaCausaAfastamento.Trim();
                                            if (!lstMotivoAfastamento.Contains(exercicio.SiglaCausaAfastamento))
                                            {
                                                banco.AddParameter("id", exercicio.SiglaCausaAfastamento);
                                                banco.AddParameter("descricao", exercicio.DescricaoCausaAfastamento);
                                                banco.ExecuteNonQuery(@"
                                                    INSERT INTO sf_motivo_afastamento (
									                    id, descricao
								                    ) VALUES (
                                                        @id, @descricao
                                                    )
                                                ");

                                                lstMotivoAfastamento.Add(exercicio.SiglaCausaAfastamento);
                                            }
                                        }

                                        banco.AddParameter("id", exercicio.CodigoExercicio);
                                        banco.AddParameter("id_sf_senador", idSenador);
                                        banco.AddParameter("id_sf_mandato", mandato.CodigoMandato);
                                        banco.AddParameter("id_sf_motivo_afastamento", exercicio.SiglaCausaAfastamento);
                                        banco.AddParameter("inicio", exercicio.DataInicio);
                                        banco.AddParameter("final", exercicio.DataFim);
                                        banco.ExecuteNonQuery(@"
                                        INSERT IGNORE INTO sf_mandato_exercicio (
									        id, id_sf_mandato, id_sf_senador, id_sf_motivo_afastamento, inicio, final
								        ) VALUES (
                                            @id, @id_sf_mandato, @id_sf_senador, @id_sf_motivo_afastamento, @inicio, @final
                                        )
                                    ");
                                    }
                                }
                                #endregion Mandato Exercicio
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, ex.Message);
                            }
                        }
                    }
                }

                #endregion Atualizar senadores ativos e os que estavam ativos antes da importação
            }

            //return string.Empty;
        }

        public string AtualizaCadastroParlamentarCompleto()
        {
            using (var banco = new AppDb())
            {
                var lstSenadorNovo = new List<string>();
                var lstSenador = new List<int>();

                using (var reader = banco.ExecuteReader("SELECT s.id FROM sf_senador s left join sf_mandato m ON m.id_sf_senador = s.id WHERE m.id IS null"))
                    while (reader.Read())
                        lstSenador.Add(Convert.ToInt32(reader["id"]));

                using (var reader = banco.ExecuteReader("SELECT id FROM sf_senador order by id"))
                    while (reader.Read())
                        lstSenadorNovo.Add(reader["id"].ToString());

                var lstLegislatura = new List<string>();
                using (var reader = banco.ExecuteReader("SELECT id FROM sf_legislatura"))
                    while (reader.Read())
                        lstLegislatura.Add(reader["id"].ToString());

                var lstMotivoAfastamento = new List<string>();
                using (var reader = banco.ExecuteReader("SELECT id FROM sf_motivo_afastamento"))
                    while (reader.Read())
                        lstMotivoAfastamento.Add(reader["id"].ToString());

                var lstProfissao = new Dictionary<string, int>();
                using (var reader = banco.ExecuteReader("SELECT id, descricao FROM profissao"))
                    while (reader.Read())
                        lstProfissao.Add(reader["descricao"].ToString(), Convert.ToInt32(reader["id"]));

                var restClient = new RestClient();
                restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                var indice = 0;
                var totalSenadores = lstSenador.Count;
                foreach (var idSenador in lstSenador)
                {
                    var reqSenador = new RestRequest($"http://legis.senado.gov.br/dadosabertos/senador/{idSenador}");
                    reqSenador.AddHeader("Accept", "application/json");

                    var resSenador = restClient.Get(reqSenador);
                    JsonNode jSenador = JsonNode.Parse(resSenador.Content).AsObject();
                    JsonNode arrParlamentar = (JsonNode)jSenador["DetalheParlamentar"]["Parlamentar"];

                    var parlamentarCompleto = arrParlamentar.Deserialize<SenadorParlamentar>();
                    logger.LogInformation("{IndiceAtual}/{Total} Consultando Senador {IdSenador}: {NomeParlamentar}", ++indice, totalSenadores, idSenador, parlamentarCompleto.IdentificacaoParlamentar.NomeParlamentar);

                    var parlamentar = parlamentarCompleto.IdentificacaoParlamentar;

                    if (string.IsNullOrEmpty(parlamentar.UfParlamentar))
                    {
                        try
                        {
                            var jUltimoMandato = jSenador["DetalheParlamentar"]["Parlamentar"]["UltimoMandato"];
                            if (jUltimoMandato != null)
                            {
                                if (jUltimoMandato is JsonArray)
                                    parlamentar.UfParlamentar = jUltimoMandato[0]["UfParlamentar"]?.ToString();
                                else
                                    parlamentar.UfParlamentar = jUltimoMandato["UfParlamentar"]?.ToString();
                            }
                        }
                        catch (Exception)
                        { }
                    }

                    if (string.IsNullOrEmpty(parlamentar.NomeParlamentar))
                    {
                        parlamentar.NomeParlamentar = parlamentar.NomeCompletoParlamentar;
                    }

                    banco.AddParameter("id", parlamentar.CodigoParlamentar);
                    banco.AddParameter("codigo", parlamentar.CodigoPublicoNaLegAtual);
                    banco.AddParameter("nome", parlamentar.NomeParlamentar);
                    banco.AddParameter("nome_completo", parlamentar.NomeCompletoParlamentar);
                    banco.AddParameter("sexo", parlamentar.SexoParlamentar[0].ToString());
                    banco.AddParameter("sigla_partido", parlamentar.SiglaPartidoParlamentar);
                    banco.AddParameter("sigla_uf", parlamentar.UfParlamentar);
                    banco.AddParameter("email", parlamentar.EmailParlamentar);
                    banco.AddParameter("site", parlamentar.UrlPaginaParticular);

                    var dadosBasicos = parlamentarCompleto.DadosBasicosParlamentar ?? new DadosBasicosParlamentar();
                    banco.AddParameter("nascimento", dadosBasicos.DataNascimento);
                    banco.AddParameter("naturalidade", dadosBasicos.Naturalidade);
                    banco.AddParameter("sigla_uf_naturalidade", dadosBasicos.UfNaturalidade);

                    var lstSenadorProfissao = new List<Profissao>();
                    if (arrParlamentar["Profissoes"] != null)
                    {
                        if (arrParlamentar["Profissoes"]["Profissao"] is JsonArray)
                        {
                            lstSenadorProfissao = arrParlamentar["Profissoes"]["Profissao"].Deserialize<List<Profissao>>();
                        }
                        else
                        {
                            lstSenadorProfissao.Add(arrParlamentar["Profissoes"]["Profissao"].Deserialize<Profissao>());
                        }
                    }
                    if (lstSenadorProfissao.Any())
                    {
                        banco.AddParameter("profissao", string.Join(", ", lstSenadorProfissao.Select(obj => obj.NomeProfissao)));
                    }
                    else
                    {
                        banco.AddParameter("profissao", DBNull.Value);
                    }

                    banco.ExecuteNonQuery(@"
                        UPDATE sf_senador SET
                            codigo = @codigo
                            , nome = @nome
                            , nome_completo = @nome_completo
                            , sexo = @sexo
                            , nascimento = @nascimento
                            , naturalidade = @naturalidade
                            , id_estado_naturalidade = (SELECT id FROM estado where sigla like @sigla_uf_naturalidade)
                            , profissao = @profissao
                            , id_partido = (SELECT id FROM partido where sigla like @sigla_partido OR nome like @sigla_partido)
                            , id_estado = (SELECT id FROM estado where sigla like @sigla_uf)
                            , email = @email
                            , site = @site
                        WHERE id = @id
                    ");

                    if (lstSenadorProfissao.Any())
                    {
                        foreach (var profissao in lstSenadorProfissao)
                        {
                            if (!lstProfissao.ContainsKey(profissao.NomeProfissao))
                            {
                                banco.AddParameter("descricao", profissao.NomeProfissao);
                                var idProfissao = banco.ExecuteScalar(@"INSERT INTO profissao (descricao) values (@descricao); SELECT LAST_INSERT_ID();");

                                lstProfissao.Add(profissao.NomeProfissao, Convert.ToInt32(idProfissao));
                            }

                            banco.AddParameter("id_sf_senador", idSenador);
                            banco.AddParameter("id_profissao", lstProfissao[profissao.NomeProfissao]);

                            banco.ExecuteNonQuery(@"INSERT IGNORE INTO sf_senador_profissao (id_sf_senador, id_profissao) values (@id_sf_senador, @id_profissao)");
                        }

                    }

                    var lstHistoricoAcademico = new List<SenadorCurso>();
                    if (arrParlamentar["HistoricoAcademico"] != null)
                    {
                        if (arrParlamentar["HistoricoAcademico"]["Curso"] is JsonArray)
                        {
                            lstHistoricoAcademico = arrParlamentar["HistoricoAcademico"]["Curso"].Deserialize<List<SenadorCurso>>();
                        }
                        else
                        {
                            lstHistoricoAcademico.Add(arrParlamentar["HistoricoAcademico"]["Curso"].Deserialize<SenadorCurso>());
                        }
                    }
                    foreach (var curso in lstHistoricoAcademico)
                    {
                        banco.AddParameter("id_sf_senador", idSenador);
                        banco.AddParameter("nome_curso", curso.NomeCurso);
                        banco.AddParameter("grau_instrucao", curso.GrauInstrucao);
                        banco.AddParameter("estabelecimento", curso.Estabelecimento);
                        banco.AddParameter("local", curso.Local);

                        banco.ExecuteNonQuery(@"
                            INSERT IGNORE INTO sf_senador_historico_academico (id_sf_senador, nome_curso, grau_instrucao, estabelecimento, local) 
                            values (@id_sf_senador, @nome_curso, @grau_instrucao, @estabelecimento, @local)");
                    }


                    var reqSenadorMandato = new RestRequest($"http://legis.senado.gov.br/dadosabertos/senador/{idSenador.ToString()}/mandatos");
                    reqSenadorMandato.AddHeader("Accept", "application/json");

                    IRestResponse resSenadorMandato = restClient.Get(reqSenadorMandato);
                    SenadorMandato jSenadorMandato = JsonSerializer.Deserialize<SenadorMandato>(resSenadorMandato.Content);
                    var arrPalamentar = jSenadorMandato.MandatoParlamentar.Parlamentar;
                    var arrMandatosPalamentar = arrPalamentar.Mandatos?.Mandato;


                    if (arrMandatosPalamentar != null)
                    {

                        logger.LogInformation("Consultando {TotalMandatos} mandato(s)", arrMandatosPalamentar.Count());
                        foreach (var mandato in arrMandatosPalamentar)
                        {
                            try
                            {
                                //Ignorar mandatos muito antigos
                                if (mandato.CodigoMandato == null) continue;

                                #region Mandato
                                banco.AddParameter("id", mandato.CodigoMandato);
                                banco.AddParameter("id_sf_senador", idSenador);
                                banco.AddParameter("sigla_uf", mandato.UfParlamentar);
                                banco.AddParameter("participacao", mandato.DescricaoParticipacao[0].ToString());
                                banco.AddParameter("exerceu", mandato.Exercicios != null);

                                banco.ExecuteNonQuery(@"
                                        INSERT IGNORE INTO sf_mandato (
                                            id, id_sf_senador, id_estado, participacao, exerceu
                                        ) VALUES (
                                            @id, @id_sf_senador, (SELECT id FROM estado where sigla like @sigla_uf), @participacao, @exerceu
                                        )
                                     ");

                                if (mandato.DescricaoParticipacao == "Titular")
                                {
                                    if (mandato.Suplentes != null && mandato.Suplentes.Suplente.Any(obj => obj.DescricaoParticipacao == "1º Suplente"))
                                    {
                                        var id = mandato.Suplentes.Suplente.FirstOrDefault(obj => obj.DescricaoParticipacao == "1º Suplente").CodigoParlamentar;
                                        if (!lstSenadorNovo.Contains(id))
                                        {
                                            lstSenadorNovo.Add(id);

                                            banco.AddParameter("id", id);
                                            banco.ExecuteNonQuery(@"INSERT INTO sf_senador (id) VALUES (@id)");
                                        }
                                    }

                                    if (mandato.Suplentes != null && mandato.Suplentes.Suplente.Any(obj => obj.DescricaoParticipacao == "2º Suplente"))
                                    {
                                        var id = mandato.Suplentes.Suplente.FirstOrDefault(obj => obj.DescricaoParticipacao == "2º Suplente").CodigoParlamentar;
                                        if (!lstSenadorNovo.Contains(id))
                                        {
                                            lstSenadorNovo.Add(id);

                                            banco.AddParameter("id", id);
                                            banco.ExecuteNonQuery(@"INSERT INTO sf_senador(id) VALUES (@id)");
                                        }
                                    }
                                }
                                #endregion Mandato

                                if (mandato.DescricaoParticipacao == "Titular")
                                {
                                    #region Mandato Legislatura

                                    var lstLegislaturaMandato = new List<LegislaturaDoMandato>();
                                    lstLegislaturaMandato.Add(mandato.PrimeiraLegislaturaDoMandato);
                                    lstLegislaturaMandato.Add(mandato.SegundaLegislaturaDoMandato);

                                    foreach (var legislatura in lstLegislaturaMandato)
                                    {
                                        if (!lstLegislatura.Contains(legislatura.NumeroLegislatura))
                                        {
                                            banco.AddParameter("id", legislatura.NumeroLegislatura);
                                            banco.AddParameter("inicio", legislatura.DataInicio);
                                            banco.AddParameter("final", legislatura.DataFim);
                                            banco.ExecuteNonQuery(@"
                                                 INSERT INTO sf_legislatura (
                          id, inicio, final
                         ) VALUES (
                                                     @id, @inicio, @final
                                                 )
                                             ");

                                            lstLegislatura.Add(legislatura.NumeroLegislatura);
                                        }

                                        banco.AddParameter("id_sf_mandato", mandato.CodigoMandato);
                                        banco.AddParameter("id_sf_legislatura", legislatura.NumeroLegislatura);
                                        banco.ExecuteNonQuery(@"
                                             INSERT IGNORE INTO sf_mandato_legislatura (
                      id_sf_mandato, id_sf_legislatura
                     ) VALUES (
                                                 @id_sf_mandato, @id_sf_legislatura
                                             )
                                         ");
                                    }
                                    #endregion Mandato Legislatura
                                }

                                #region Mandato Exercicio
                                if (mandato.Exercicios != null)
                                {
                                    foreach (var exercicio in mandato.Exercicios.Exercicio)
                                    {
                                        if (exercicio.SiglaCausaAfastamento != null)
                                        {
                                            exercicio.SiglaCausaAfastamento = exercicio.SiglaCausaAfastamento.Trim();
                                            if (!lstMotivoAfastamento.Contains(exercicio.SiglaCausaAfastamento))
                                            {
                                                banco.AddParameter("id", exercicio.SiglaCausaAfastamento);
                                                banco.AddParameter("descricao", exercicio.DescricaoCausaAfastamento);
                                                banco.ExecuteNonQuery(@"
                                                         INSERT INTO sf_motivo_afastamento (
                                  id, descricao
                                 ) VALUES (
                                                             @id, @descricao
                                                         )
                                                     ");

                                                lstMotivoAfastamento.Add(exercicio.SiglaCausaAfastamento);
                                            }
                                        }

                                        banco.AddParameter("id", exercicio.CodigoExercicio);
                                        banco.AddParameter("id_sf_senador", idSenador);
                                        banco.AddParameter("id_sf_mandato", mandato.CodigoMandato);
                                        banco.AddParameter("id_sf_motivo_afastamento", exercicio.SiglaCausaAfastamento);
                                        banco.AddParameter("inicio", exercicio.DataInicio);
                                        banco.AddParameter("final", exercicio.DataFim);
                                        banco.ExecuteNonQuery(@"
                                                 INSERT IGNORE INTO sf_mandato_exercicio (
                          id, id_sf_mandato, id_sf_senador, id_sf_motivo_afastamento, inicio, final
                         ) VALUES (
                                                     @id, @id_sf_mandato, @id_sf_senador, @id_sf_motivo_afastamento, @inicio, @final
                                                 )
                                             ");
                                    }
                                }
                                #endregion Mandato Exercicio
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, ex.Message);
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        public override Dictionary<string, string> DefinirOrigemDestino(int ano)
        {
            Dictionary<string, string> arquivos = new();

            // https://www12.senado.leg.br/transparencia/dados-abertos-transparencia/dados-abertos-ceaps
            // Arquivos disponiveis anualmente a partir de 2008
            var _urlOrigem = $"https://www.senado.gov.br/transparencia/LAI/verba/despesa_ceaps_{ano}.csv";
            var _caminhoArquivo = $"{tempPath}/SF-{ano}.csv";

            arquivos.Add(_urlOrigem, _caminhoArquivo);
            return arquivos;
        }

        protected override void ProcessarDespesas(string file, int ano)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            string sResumoValores = string.Empty;
            int lote = 0, linhaInserida = 0;
            var dc = new Dictionary<string, UInt64>();
            var gerarHash = ano >= (DateTime.Now.Year - 2);

            int indice = 0;
            int ANO = indice++;
            int MES = indice++;
            int SENADOR = indice++;
            int TIPO_DESPESA = indice++;
            int CNPJ_CPF = indice++;
            int FORNECEDOR = indice++;
            int DOCUMENTO = indice++;
            int DATA = indice++;
            int DETALHAMENTO = indice++;
            int VALOR_REEMBOLSADO = indice++;
            int COD_DOCUMENTO = indice++;

            using (var banco = new AppDb())
            {
                if (gerarHash)
                    using (var dReader = banco.ExecuteReader($"select id, hash from sf_despesa where ano={ano} and hash IS NOT NULL"))
                        while (dReader.Read())
                            dc.Add(Convert.ToHexString((byte[])dReader["hash"]), (UInt64)dReader["id"]);

                LimpaDespesaTemporaria(banco);

                using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
                {
                    short count = 0;

                    while (!reader.EndOfStream)
                    {
                        count++;

                        var linha = reader.ReadLine();
                        if (string.IsNullOrEmpty(linha))
                            continue;
                        if (!linha.EndsWith("\""))
                            linha += reader.ReadLine();

                        var valores = ImportacaoUtils.ParseCsvRowToList(@""";""", linha);

                        if (count == 1) //Pula primeira linha, data da atualização
                            continue;

                        if (count == 2)
                        {
                            if (
                                    (valores[ANO] != "ANO") ||
                                    (valores[MES] != "MES") ||
                                    (valores[SENADOR] != "SENADOR") ||
                                    (valores[TIPO_DESPESA] != "TIPO_DESPESA") ||
                                    (valores[CNPJ_CPF] != "CNPJ_CPF") ||
                                    (valores[FORNECEDOR] != "FORNECEDOR") ||
                                    (valores[DOCUMENTO] != "DOCUMENTO") ||
                                    (valores[DATA] != "DATA") ||
                                    (valores[DETALHAMENTO] != "DETALHAMENTO") ||
                                    (valores[VALOR_REEMBOLSADO] != "VALOR_REEMBOLSADO") ||
                                    (valores[COD_DOCUMENTO] != "COD_DOCUMENTO")

                                )

                            {
                                throw new Exception("Mudança de integração detectada para o Senado Federal");
                            }

                            // Pular linha de titulo
                            continue;
                        }

                        banco.AddParameter("ano", Convert.ToInt32(valores[ANO]));
                        banco.AddParameter("mes", Convert.ToInt32(valores[MES]));
                        banco.AddParameter("senador", valores[SENADOR]);
                        banco.AddParameter("tipo_despesa", valores[TIPO_DESPESA]);
                        banco.AddParameter("cnpj_cpf", !string.IsNullOrEmpty(valores[CNPJ_CPF]) ? Utils.RemoveCaracteresNaoNumericos(valores[CNPJ_CPF]) : "");
                        banco.AddParameter("fornecedor", valores[FORNECEDOR]);
                        banco.AddParameter("documento", valores[DOCUMENTO]);
                        banco.AddParameter("data", !string.IsNullOrEmpty(valores[DATA]) ? (object)Convert.ToDateTime(valores[DATA], cultureInfo) : DBNull.Value);
                        banco.AddParameter("detalhamento", valores[DETALHAMENTO]);
                        banco.AddParameter("valor_reembolsado", Convert.ToDouble(valores[VALOR_REEMBOLSADO], cultureInfo));
                        banco.AddParameter("cod_documento", Convert.ToUInt64(valores[COD_DOCUMENTO], cultureInfo));

                        byte[] hash = null;
                        if (gerarHash)
                        {
                            hash = banco.ParametersHash();
                            var key = Convert.ToHexString(hash);
                            if (dc.Remove(key))
                            {
                                banco.ClearParameters();
                                continue;
                            }
                        }
                        banco.AddParameter("hash", hash);

                        banco.ExecuteNonQuery(
                            @"INSERT INTO ops_tmp.sf_despesa_temp (
								ano, mes, senador, tipo_despesa, cnpj_cpf, fornecedor, documento, `data`, detalhamento, valor_reembolsado, cod_documento, hash
							) VALUES (
								@ano, @mes, @senador, @tipo_despesa, @cnpj_cpf, @fornecedor, @documento, @data, @detalhamento, @valor_reembolsado, @cod_documento, @hash
							)");

                        if (linhaInserida++ == 10000)
                        {
                            logger.LogInformation("Processando lote {Lote}", ++lote);
                            ProcessarDespesasTemp(banco);
                            linhaInserida = 0;
                        }
                    }
                }

                if (linhaInserida > 0)
                {
                    logger.LogInformation("Processando lote {Lote}", ++lote);
                    ProcessarDespesasTemp(banco);
                }

                foreach (var id in dc.Values)
                {
                    banco.AddParameter("id", id);
                    banco.ExecuteNonQuery("delete from sf_despesa where id=@id");
                }

                if (ano == DateTime.Now.Year)
                {
                    AtualizaParlamentarValores();
                    AtualizaCampeoesGastos();
                    AtualizaResumoMensal();
                }
            }
        }

        private void ProcessarDespesasTemp(AppDb banco)
        {
            CorrigeDespesas(banco);
            InsereSenadorFaltante(banco);
            InsereFornecedorFaltante(banco);
            InsereDespesaFinal(banco);
            LimpaDespesaTemporaria(banco);
        }

        private void CorrigeDespesas(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				UPDATE ops_tmp.sf_despesa_temp 
				SET tipo_despesa = 'Aquisição de material de consumo para uso no escritório político' 
				WHERE tipo_despesa LIKE 'Aquisição de material de consumo para uso no escritório político%';

				UPDATE ops_tmp.sf_despesa_temp 
				SET tipo_despesa = 'Contratação de consultorias, assessorias, pesquisas, trabalhos técnicos e outros serviços' 
				WHERE tipo_despesa LIKE 'Contratação de consultorias, assessorias, pesquisas, trabalhos técnicos e outros serviços%';	
			");
        }

        private string InsereSenadorFaltante(AppDb banco)
        {
            //object total = banco.ExecuteScalar(@"select count(1) from ops_tmp.sf_despesa_temp where senador  not in (select ifnull(nome_importacao, nome) from sf_senador);");
            //if (Convert.ToInt32(total) > 0)
            //{
            //	CarregaSenadoresAtuais();

            object total = banco.ExecuteScalar(@"select count(1) from ops_tmp.sf_despesa_temp where senador  not in (select ifnull(nome_importacao, nome) from sf_senador);");
            if (Convert.ToInt32(total) > 0)
            {
                throw new Exception("Existem despesas de senadores que não estão cadastrados!");
            }
            //}

            return string.Empty;
        }

        private string InsereFornecedorFaltante(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				INSERT INTO fornecedor (nome, cnpj_cpf)
				select MAX(dt.fornecedor), dt.cnpj_cpf
				from ops_tmp.sf_despesa_temp dt
				left join fornecedor f on f.cnpj_cpf = dt.cnpj_cpf
				where dt.cnpj_cpf is not null
				and f.id is null
				GROUP BY dt.cnpj_cpf;
			");

            if (banco.RowsAffected > 0)
            {
                return "<p>" + banco.RowsAffected + "+ Fornecedor</p>";
            }

            return string.Empty;
        }

        private string InsereDespesaFinal(AppDb banco)
        {
            var retorno = "";
            banco.ExecuteNonQuery(@"
				ALTER TABLE sf_despesa DISABLE KEYS;

				INSERT IGNORE INTO sf_despesa (
                    id,
					id_sf_senador,
					id_sf_despesa_tipo,
					id_fornecedor,
					ano_mes,
					ano,
					mes,
					documento,
					data_emissao,
					detalhamento,
					valor,
					hash
				)
				SELECT 
                    d.cod_documento,
					p.id,
					dt.id,
					f.id,
					concat(ano, LPAD(mes, 2, '0')),
					d.ano,
					d.mes,
					d.documento,
					d.`data`,
					d.detalhamento,
					d.valor_reembolsado,
					d.hash
				FROM ops_tmp.sf_despesa_temp d
				inner join sf_senador p on ifnull(p.nome_importacao, p.nome) = d.senador
				inner join sf_despesa_tipo dt on dt.descricao = d.tipo_despesa
				inner join fornecedor f on f.cnpj_cpf = d.cnpj_cpf;
    
				ALTER TABLE sf_despesa ENABLE KEYS;
			", 3600);


            if (banco.RowsAffected > 0)
            {
                retorno = +banco.RowsAffected + "+ Despesa nova! ";
            }

            //         banco.ExecuteNonQuery(@"
            //	UPDATE ops_tmp.sf_despesa_temp t 
            //	inner join sf_senador p on ifnull(p.nome_importacao, p.nome) = t.senador
            //	inner join sf_despesa_tipo dt on dt.descricao = t.tipo_despesa
            //	inner join fornecedor f on f.cnpj_cpf = t.cnpj_cpf
            //             inner join sf_despesa d on d.id = t.cod_documento and p.id = d.id_sf_senador
            //             set
            //		d.id_sf_despesa_tipo = dt.id,
            //		d.id_fornecedor = f.id,
            //		d.ano_mes = concat(t.ano, LPAD(t.mes, 2, '0')),
            //		d.ano = t.ano,
            //		d.mes = t.mes,
            //		d.documento = t.documento,
            //		d.data_emissao = t.`data`,
            //		d.detalhamento = t.detalhamento,
            //		d.valor = t.valor_reembolsado,
            //		d.hash = t.hash
            //", 3600);

            //         if (banco.RowsAffected > 0)
            //         {
            //             retorno += banco.RowsAffected + "+ Despesa alterada! ";
            //         }

            //if (!string.IsNullOrEmpty(retorno.Trim()))
            //{
            //    return "<p>" + retorno.Trim() + "</p>";
            //}
            return string.Empty;
        }

        private void InsereDespesaFinalParcial(AppDb banco)
        {
            var dt = banco.GetTable(
                @"DROP TABLE IF EXISTS table_in_memory_d;
                CREATE TEMPORARY TABLE table_in_memory_d
                AS (
	                select id_sf_senador, mes, sum(valor) as total
	                from sf_despesa d
	                where ano = 2017
	                GROUP BY id_sf_senador, mes
                );

                DROP TABLE IF EXISTS table_in_memory_dt;
                CREATE TEMPORARY TABLE table_in_memory_dt
                AS (
	                select p.id as id_sf_senador, mes, sum(valor_reembolsado) as total
	                from ops_tmp.sf_despesa_temp d
                    inner join sf_senador p on p.nome = d.senador
	                GROUP BY p.id, mes
                );

                select dt.id_sf_senador, dt.mes
                from table_in_memory_dt dt
                left join table_in_memory_d d on dt.id_sf_senador = d.id_sf_senador and dt.mes = d.mes
                where (d.id_sf_senador is null or d.total <> dt.total)
                order by d.id_sf_senador, d.mes;
			    ", 3600);

            foreach (DataRow dr in dt.Rows)
            {
                banco.AddParameter("id_sf_senador", dr["id_sf_senador"]);
                banco.AddParameter("mes", dr["mes"]);
                banco.ExecuteNonQuery(@"DELETE FROM sf_despesa WHERE id_sf_senador=@id_sf_senador and mes=@mes");

                banco.AddParameter("id_sf_senador", dr["id_sf_senador"]);
                banco.AddParameter("mes", dr["mes"]);
                banco.ExecuteNonQuery(@"
				        INSERT INTO sf_despesa (
					        id_sf_senador,
					        id_sf_despesa_tipo,
					        id_fornecedor,
					        ano_mes,
					        ano,
					        mes,
					        documento,
					        data_emissao,
					        detalhamento,
					        valor
				        )
				        SELECT 
					        d.id,
					        dt.id,
					        f.id,
					        concat(ano, LPAD(mes, 2, '0')),
					        d.ano,
					        d.mes,
					        d.documento,
					        d.`data`,
					        d.detalhamento,
					        d.valor_reembolsado
					    from (
						    select d.*, p.id					        
						    from ops_tmp.sf_despesa_temp d
                            inner join sf_senador p on p.nome = d.senador
						    WHERE p.id=@id_sf_senador and mes=@mes
					    ) d
				        inner join sf_despesa_tipo dt on dt.descricao = d.tipo_despesa
				        inner join fornecedor f on f.cnpj_cpf = d.cnpj_cpf;
			        ", 3600);
            }
        }

        private void LimpaDespesaTemporaria(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				truncate table ops_tmp.sf_despesa_temp;
			");
        }

        public override void AtualizaParlamentarValores()
        {
            connection.Execute("UPDATE sf_senador SET valor_total_ceaps=0;");

            var dt = connection.Query(@"select id from sf_senador
						WHERE id IN (
						select distinct id_sf_senador
						from sf_despesa
					)");
            object valor_total_ceaps;

            foreach (var dr in dt)
            {
                valor_total_ceaps = connection.ExecuteScalar("select sum(valor) from sf_despesa where id_sf_senador=@id_sf_senador;",
                    new { id_sf_senador = Convert.ToInt32(dr.id) });

                connection.Execute(
                    @"update sf_senador set 
						valor_total_ceaps=@valor_total_ceaps
						where id=@id_sf_senador", new
                    {
                        valor_total_ceaps = valor_total_ceaps,
                        id_sf_senador = Convert.ToInt32(dr.id)
                    }
                );
            }
        }

        public override void AtualizaCampeoesGastos()
        {
            var strSql =
                @"truncate table sf_senador_campeao_gasto;
				insert into sf_senador_campeao_gasto
				SELECT l1.id_sf_senador, d.nome, l1.valor_total, p.sigla, e.sigla
				FROM (
					SELECT 
						l.id_sf_senador,
						sum(l.valor) as valor_total
					FROM  sf_despesa l
					where l.ano_mes >= 201902 
					GROUP BY l.id_sf_senador
					order by valor_total desc 
					limit 4
				) l1 
				INNER JOIN sf_senador d on d.id = l1.id_sf_senador
				LEFT JOIN partido p on p.id = d.id_partido
				LEFT JOIN estado e on e.id = d.id_estado;";

            connection.Execute(strSql);
        }

        public override void AtualizaResumoMensal()
        {
            var strSql =
                @"truncate table sf_despesa_resumo_mensal;
					insert into sf_despesa_resumo_mensal
					(ano, mes, valor) (
						select ano, mes, sum(valor)
						from sf_despesa
						group by ano, mes
					);";

            connection.Execute(strSql);
        }

        public override async void DownloadFotosParlamentares()
        {
            var sSenadoressImagesPath = System.IO.Path.Combine(rootPath, "public/img/senador/");

            var db = new StringBuilder();

            using (var banco = new AppDb())
            {
                DataTable table = banco.GetTable("SELECT id FROM sf_senador where ativo = 'S'");

                foreach (DataRow row in table.Rows)
                {
                    string id = row["id"].ToString();
                    string url = "https://www.senado.leg.br/senadores/img/fotos-oficiais/senador" + id + ".jpg";
                    string src = sSenadoressImagesPath + id + ".jpg";
                    if (File.Exists(src)) continue;

                    try
                    {
                        using (HttpClient client = new())
                        {
                            client.DefaultRequestHeaders.UserAgent.ParseAdd(Utils.DefaultUserAgent);
                            await client.DownloadFile(url, src);

                            ImportacaoUtils.CreateImageThumbnail(src, 120, 160);
                            ImportacaoUtils.CreateImageThumbnail(src, 240, 300);

                            db.AppendLine("Atualizado imagem do senador " + id);
                            logger.LogInformation("Atualizado imagem do senador {IdSenador}", id);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("404"))
                        {
                            db.AppendLine("Imagem do senador " + id + " inexistente! Motivo: " + ex.ToFullDescriptionString());
                            logger.LogInformation("Imagem do senador {IdSenador} inexistente! Motivo: {Motivo}", id, ex.ToFullDescriptionString());
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  https://www.senado.leg.br/transparencia/rh/servidores/consulta_remuneracao.asp
        /// </summary>
        /// <param name="ano"></param>
        /// <param name="mes"></param>
        public override void ImportarRemuneracao(int ano, int mes)
        {
            var anomes = Convert.ToInt32($"{ano:0000}{mes:00}");
            var urlOrigem = string.Format("http://www.senado.leg.br/transparencia/LAI/secrh/SF_ConsultaRemuneracaoServidoresParlamentares_{0}.csv", anomes);
            var caminhoArquivo = System.IO.Path.Combine(tempPath, "SF-RM-" + anomes + ".csv");

            try
            {
                var possuiNovosItens = TentarBaixarArquivo(urlOrigem, caminhoArquivo);

                if (possuiNovosItens)
                    CarregaRemuneracaoCsv(caminhoArquivo, anomes);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                if (File.Exists(caminhoArquivo))
                    File.Delete(caminhoArquivo);
            }
        }

        private void CarregaRemuneracaoCsv(string file, int anomes)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            var sb = new StringBuilder();

            int indice = 0;
            int VÍNCULO = indice++;
            int CATEGORIA = indice++;
            int CARGO = indice++;
            int REFERÊNCIA_CARGO = indice++;
            int SÍMBOLO_FUNÇÃO = indice++;
            int ANO_EXERCÍCIO = indice++;
            int LOTAÇÃO_EXERCÍCIO = indice++;
            int TIPO_FOLHA = indice++;
            int REMUN_BASICA = indice++;
            int VANT_PESSOAIS = indice++;
            int FUNC_COMISSIONADA = indice++;
            int GRAT_NATALINA = indice++;
            int HORAS_EXTRAS = indice++;
            int OUTRAS_EVENTUAIS = indice++;
            int ABONO_PERMANENCIA = indice++;
            int REVERSAO_TETO_CONST = indice++;
            int IMPOSTO_RENDA = indice++;
            int PREVIDENCIA = indice++;
            int FALTAS = indice++;
            int REM_LIQUIDA = indice++;
            int DIARIAS = indice++;
            int AUXILIOS = indice++;
            int VANT_INDENIZATORIAS = indice++;

            using (var banco = new AppDb())
            {
                LimpaRemuneracaoTemporaria();

                using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
                {
                    short linha = 0;

                    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")))
                    {
                        while (csv.Read())
                        {
                            linha++;
                            if (linha == 1) //Pula primeira linha, data da atualização
                                continue;

                            if (linha == 2)
                            {
                                if (
                                        (csv[VÍNCULO] != "VÍNCULO") ||
                                        (csv[VANT_INDENIZATORIAS].Trim() != "VANT_INDENIZATORIAS")
                                    )
                                {
                                    throw new Exception("Mudança de integração detectada para o Senado Federal");
                                }

                                // Pular linha de titulo
                                continue;
                            }

                            banco.AddParameter("ano_mes", anomes);
                            banco.AddParameter("vinculo", csv[VÍNCULO]);
                            banco.AddParameter("categoria", csv[CATEGORIA]);
                            banco.AddParameter("cargo", csv[CARGO]);
                            banco.AddParameter("referencia_cargo", csv[REFERÊNCIA_CARGO]);
                            banco.AddParameter("simbolo_funcao", csv[SÍMBOLO_FUNÇÃO]);
                            banco.AddParameter("admissao", Convert.ToInt32(csv[ANO_EXERCÍCIO]));
                            banco.AddParameter("lotacao_exercicio", csv[LOTAÇÃO_EXERCÍCIO]);
                            banco.AddParameter("tipo_folha", csv[TIPO_FOLHA]);

                            banco.AddParameter("remun_basica", Convert.ToDouble(csv[REMUN_BASICA], cultureInfo));
                            banco.AddParameter("vant_pessoais", Convert.ToDouble(csv[VANT_PESSOAIS], cultureInfo));
                            banco.AddParameter("func_comissionada", Convert.ToDouble(csv[FUNC_COMISSIONADA], cultureInfo));
                            banco.AddParameter("grat_natalina", Convert.ToDouble(csv[GRAT_NATALINA], cultureInfo));
                            banco.AddParameter("horas_extras", Convert.ToDouble(csv[HORAS_EXTRAS], cultureInfo));
                            banco.AddParameter("outras_eventuais", Convert.ToDouble(csv[OUTRAS_EVENTUAIS], cultureInfo));
                            banco.AddParameter("abono_permanencia", Convert.ToDouble(csv[ABONO_PERMANENCIA], cultureInfo));
                            banco.AddParameter("reversao_teto_const", Convert.ToDouble(csv[REVERSAO_TETO_CONST], cultureInfo));
                            banco.AddParameter("imposto_renda", Convert.ToDouble(csv[IMPOSTO_RENDA], cultureInfo));
                            banco.AddParameter("previdencia", Convert.ToDouble(!string.IsNullOrEmpty(csv[PREVIDENCIA]) ? csv[PREVIDENCIA] : "0", cultureInfo));
                            banco.AddParameter("faltas", Convert.ToDouble(!string.IsNullOrEmpty(csv[FALTAS]) ? csv[FALTAS] : "0", cultureInfo));
                            banco.AddParameter("rem_liquida", Convert.ToDouble(!string.IsNullOrEmpty(csv[REM_LIQUIDA]) ? csv[REM_LIQUIDA] : "0", cultureInfo));
                            banco.AddParameter("diarias", Convert.ToDouble(!string.IsNullOrEmpty(csv[DIARIAS]) ? csv[DIARIAS] : "0", cultureInfo));
                            banco.AddParameter("auxilios", Convert.ToDouble(!string.IsNullOrEmpty(csv[AUXILIOS]) ? csv[AUXILIOS] : "0", cultureInfo));
                            banco.AddParameter("vant_indenizatorias", Convert.ToDouble(!string.IsNullOrEmpty(csv[VANT_INDENIZATORIAS]) ? csv[VANT_INDENIZATORIAS] : "0".Trim(), cultureInfo));

                            banco.ExecuteNonQuery(
                                @"INSERT INTO ops_tmp.sf_remuneracao_temp (
								ano_mes, vinculo,categoria,cargo,referencia_cargo,simbolo_funcao,admissao,lotacao_exercicio,tipo_folha,
                                remun_basica ,vant_pessoais ,func_comissionada ,grat_natalina ,horas_extras ,outras_eventuais ,abono_permanencia ,
                                reversao_teto_const ,imposto_renda ,previdencia ,faltas ,rem_liquida ,diarias ,auxilios ,vant_indenizatorias
							) VALUES (
								@ano_mes,@vinculo,@categoria,@cargo,@referencia_cargo,@simbolo_funcao,@admissao,@lotacao_exercicio,@tipo_folha,
                                @remun_basica ,@vant_pessoais ,@func_comissionada ,@grat_natalina ,@horas_extras ,@outras_eventuais ,@abono_permanencia ,
                                @reversao_teto_const ,@imposto_renda ,@previdencia ,@faltas ,@rem_liquida ,@diarias ,@auxilios ,@vant_indenizatorias
							)");


                            if (linha % 1000 == 0)
                            {
                                ProcessarRemuneracaoTemp(banco);
                            }
                        }
                    }

                    logger.LogInformation("{Itens} na fila de processamento!", linha);
                }

                ProcessarRemuneracaoTemp(banco);
            }

            using (var banco = new AppDb())
            {
                banco.AddParameter("ano_mes", anomes);
                banco.ExecuteNonQuery(@"
                    UPDATE sf_senador s
                    JOIN sf_lotacao l ON l.id_senador = s.id
                    JOIN sf_remuneracao r ON l.id = r.id_lotacao
                    SET valor_total_remuneracao = (
	                    SELECT SUM(custo_total) AS total
	                    FROM sf_remuneracao r
	                    JOIN sf_lotacao l ON l.id = r.id_lotacao
	                    where l.id_senador = s.id
                    )
                    WHERE r.ano_mes = @ano_mes
			", 3600);
            }
        }

        private void ProcessarRemuneracaoTemp(AppDb banco)
        {
            var total = banco.ExecuteScalar(@"select count(1) from ops_tmp.sf_remuneracao_temp");

            banco.ExecuteNonQuery(@"
                INSERT IGNORE INTO sf_funcao  (descricao)
                SELECT DISTINCT simbolo_funcao FROM ops_tmp.sf_remuneracao_temp WHERE simbolo_funcao <> '' ORDER BY simbolo_funcao;

                INSERT IGNORE INTO sf_cargo  (descricao)
                SELECT DISTINCT cargo FROM ops_tmp.sf_remuneracao_temp WHERE cargo <> '' ORDER BY cargo;

                INSERT IGNORE INTO sf_categoria  (descricao)
                SELECT DISTINCT categoria FROM ops_tmp.sf_remuneracao_temp WHERE categoria <> '' ORDER BY categoria;

                INSERT IGNORE INTO sf_vinculo  (descricao)
                SELECT DISTINCT vinculo FROM ops_tmp.sf_remuneracao_temp WHERE vinculo <> '' ORDER BY vinculo;

                INSERT IGNORE INTO sf_referencia_cargo  (descricao)
                SELECT DISTINCT referencia_cargo FROM ops_tmp.sf_remuneracao_temp WHERE referencia_cargo IS NOT NULL ORDER BY referencia_cargo;

                INSERT IGNORE INTO sf_lotacao( descricao)
                SELECT DISTINCT lotacao_exercicio FROM ops_tmp.sf_remuneracao_temp WHERE lotacao_exercicio <> '' ORDER BY lotacao_exercicio;
            ", 3600);

            banco.ExecuteNonQuery(@"
                INSERT INTO sf_remuneracao 
	                (id_vinculo, id_categoria, id_cargo, id_referencia_cargo, id_simbolo_funcao, id_lotacao, id_tipo_folha, ano_mes, admissao, remun_basica, vant_pessoais, func_comissionada, grat_natalina, horas_extras, outras_eventuais, abono_permanencia, reversao_teto_const, imposto_renda, previdencia, faltas, rem_liquida, diarias, auxilios, vant_indenizatorias, custo_total)
                SELECT 
	                v.id AS vinculo, ct.id AS categoria, cr.id AS cargo, rc.id as referencia_cargo, sf.id as simbolo_funcao, le.id as lotacao_exercicio, tf.id as tipo_folha, ano_mes, admissao, remun_basica, vant_pessoais, func_comissionada, grat_natalina, horas_extras, outras_eventuais, abono_permanencia, reversao_teto_const, imposto_renda, previdencia, faltas, rem_liquida, diarias, auxilios, vant_indenizatorias,
	                -- (remun_basica + vant_pessoais + func_comissionada + grat_natalina + horas_extras + outras_eventuais + abono_permanencia + reversao_teto_const + faltas + diarias + auxilios + vant_indenizatorias) AS custo_total,
	                (rem_liquida - imposto_renda - previdencia - faltas + diarias + auxilios + vant_indenizatorias) AS custo_total
                FROM ops_tmp.sf_remuneracao_temp tmp
                JOIN sf_vinculo v ON v.descricao LIKE ops_tmp.vinculo
                JOIN sf_categoria ct ON ct.descricao LIKE ops_tmp.categoria
                LEFT JOIN sf_cargo cr ON cr.descricao LIKE ops_tmp.cargo
                LEFT JOIN sf_referencia_cargo rc ON rc.descricao LIKE ops_tmp.referencia_cargo
                LEFT JOIN sf_funcao sf ON sf.descricao LIKE ops_tmp.simbolo_funcao
                JOIN sf_lotacao le ON le.descricao LIKE ops_tmp.lotacao_exercicio
                JOIN sf_tipo_folha tf ON tf.descricao LIKE ops_tmp.tipo_folha
			", 3600);

            if (Convert.ToInt32(total) != banco.RowsAffected)
            {
                throw new Exception("Existem relacionamentos não mapeados!");
            }

            LimpaRemuneracaoTemporaria();
        }

        private void LimpaRemuneracaoTemporaria()
        {
            connection.Execute(@"
				truncate table ops_tmp.sf_remuneracao_temp;
			");
        }
    }
}