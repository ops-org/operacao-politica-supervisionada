using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using CsvHelper;
using Newtonsoft.Json.Linq;
using OPS.Core;
using RestSharp;

namespace OPS.Importador
{
    public class Senado
    {
        public static string AtualizaCadastroSenadores()
        {
            using (var banco = new AppDb())
            {
                var lstSenadorAtivo = new List<int>();
                var lstSenador = new List<int>();
                using (var reader = banco.ExecuteReader("SELECT id FROM sf_senador where ativo = 'S' order by id"))
                {
                    while (reader.Read())
                    {
                        lstSenador.Add(Convert.ToInt32(reader["id"]));
                    }
                }

                banco.ExecuteNonQuery("UPDATE sf_senador SET ativo = 'N' WHERE ativo = 'S'");

                var client = new RestClient();

                #region Importar novos senadores
                var request = new RestRequest("http://legis.senado.gov.br/dadosabertos/senador/lista/atual", Method.GET);
                request.AddHeader("Accept", "application/json");

                IRestResponse resSenadores = client.Execute(request);
                JObject jSenadores = JObject.Parse(resSenadores.Content);
                JArray arrIdentificacaoParlamentar = (JArray)jSenadores["ListaParlamentarEmExercicio"]["Parlamentares"]["Parlamentar"];

                foreach (var senador in arrIdentificacaoParlamentar)
                {
                    try
                    {
                        var parlamentar = senador["IdentificacaoParlamentar"].ToObject<IdentificacaoParlamentar>();

                        banco.ClearParameters();
                        banco.AddParameter("id", parlamentar.CodigoParlamentar);

                        var existe = banco.ExecuteScalar("SELECT 1 FROM sf_senador WHERE id = @id");
                        if (existe == null)
                        {
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
                        Console.WriteLine(ex.ToFullDescriptionString());
                    }
                }

                #endregion Importar novos senadores

                #region Atualizar senadores ativos e os que estavam ativos antes da importação

                var lstLegislatura = new List<int>();
                using (var reader = banco.ExecuteReader("SELECT id FROM sf_legislatura"))
                {
                    while (reader.Read())
                    {
                        lstLegislatura.Add(Convert.ToInt32(reader["id"]));
                    }
                }

                var lstMotivoAfastamento = new List<string>();
                using (var reader = banco.ExecuteReader("SELECT id FROM sf_motivo_afastamento"))
                {
                    while (reader.Read())
                    {
                        lstMotivoAfastamento.Add(reader["id"].ToString());
                    }
                }

                var lstProfissao = new Dictionary<string, int>();
                using (var reader = banco.ExecuteReader("SELECT id, descricao FROM profissao"))
                {
                    while (reader.Read())
                    {
                        lstProfissao.Add(reader["descricao"].ToString(), Convert.ToInt32(reader["id"]));
                    }
                }

                foreach (var idSenador in lstSenador)
                {
                    request = new RestRequest("http://legis.senado.gov.br/dadosabertos/senador/" + idSenador, Method.GET);
                    request.AddHeader("Accept", "application/json");

                    var resSenador = client.Execute(request);
                    JObject jSenador = JObject.Parse(resSenador.Content);
                    JObject arrParlamentar = (JObject)jSenador["DetalheParlamentar"]["Parlamentar"];

                    var parlamentarCompleto = arrParlamentar.ToObject<Parlamentar>();
                    Console.WriteLine("Consultando dados do senador {0}: {1}", idSenador, parlamentarCompleto.IdentificacaoParlamentar.NomeParlamentar);

                    var parlamentar = parlamentarCompleto.IdentificacaoParlamentar;

                    if (string.IsNullOrEmpty(parlamentar.UfParlamentar))
                    {
                        try
                        {
                            var jUltimoMandato = jSenador["DetalheParlamentar"]["Parlamentar"]["UltimoMandato"];
                            if (jUltimoMandato is JArray)
                                parlamentar.UfParlamentar = jUltimoMandato[0]["UfParlamentar"].ToString();
                            else
                                parlamentar.UfParlamentar = jUltimoMandato["UfParlamentar"].ToString();
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

                    var dadosBasicos = parlamentarCompleto.DadosBasicosParlamentar;
                    banco.AddParameter("nascimento", dadosBasicos.DataNascimento);
                    banco.AddParameter("naturalidade", dadosBasicos.Naturalidade);
                    banco.AddParameter("sigla_uf_naturalidade", dadosBasicos.UfNaturalidade);

                    var lstSenadorProfissao = new List<Profissao>();
                    if (arrParlamentar["Profissoes"] != null)
                    {
                        if (arrParlamentar["Profissoes"]["Profissao"] is JArray)
                        {
                            lstSenadorProfissao = arrParlamentar["Profissoes"]["Profissao"].ToObject<List<Profissao>>();
                        }
                        else
                        {
                            lstSenadorProfissao.Add(arrParlamentar["Profissoes"]["Profissao"].ToObject<Profissao>());
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

                    banco.AddParameter("ativo", lstSenadorAtivo.Contains(parlamentar.CodigoParlamentar) ? "S" : "N");

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

                    var lstHistoricoAcademico = new List<Curso>();
                    if (arrParlamentar["HistoricoAcademico"] != null)
                    {
                        if (arrParlamentar["HistoricoAcademico"]["Curso"] is JArray)
                        {
                            lstHistoricoAcademico = arrParlamentar["HistoricoAcademico"]["Curso"].ToObject<List<Curso>>();
                        }
                        else
                        {
                            lstHistoricoAcademico.Add(arrParlamentar["HistoricoAcademico"]["Curso"].ToObject<Curso>());
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


                    request = new RestRequest("http://legis.senado.gov.br/dadosabertos/senador/" + idSenador.ToString() + "/mandatos", Method.GET);
                    request.AddHeader("Accept", "application/json");

                    IRestResponse resSenadorMandato = client.Execute(request);
                    JObject jSenadorMandato = JObject.Parse(resSenadorMandato.Content);
                    JToken arrPalamentar = jSenadorMandato["MandatoParlamentar"]["Parlamentar"];
                    JToken arrMandatosPalamentar = arrPalamentar["Mandatos"];

                    Console.WriteLine("Consultando mandatos do senador {0}: {1}", idSenador, arrPalamentar["IdentificacaoParlamentar"]["NomeParlamentar"].ToString());

                    if (arrMandatosPalamentar != null)
                    {
                        JArray arrMandatoParlamentar = (JArray)arrMandatosPalamentar["Mandato"];

                        foreach (var jMandato in arrMandatoParlamentar)
                        {
                            var mandato = jMandato.ToObject<Mandato>();

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
                                        //var id = mandato.Suplentes.Suplente.FirstOrDefault(obj => obj.DescricaoParticipacao == "1º Suplente").CodigoParlamentar;
                                        //if (!lstSenadorNovo.Contains(id))
                                        //{
                                        //    lstSenadorNovo.Add(id);

                                        //    banco.AddParameter("id", id);
                                        //    banco.ExecuteNonQuery(@"INSERT INTO sf_senador (id) VALUES (@id)");
                                        //}

                                        // TODO: Inserir o senador se não existir e importar os detalhes do mandato
                                    }

                                    if (mandato.Suplentes != null && mandato.Suplentes.Suplente.Any(obj => obj.DescricaoParticipacao == "2º Suplente"))
                                    {
                                        //var id = mandato.Suplentes.Suplente.FirstOrDefault(obj => obj.DescricaoParticipacao == "2º Suplente").CodigoParlamentar;
                                        //if (!lstSenadorNovo.Contains(id))
                                        //{
                                        //    lstSenadorNovo.Add(id);

                                        //    banco.AddParameter("id", id);
                                        //    banco.ExecuteNonQuery(@"INSERT INTO sf_senador(id) VALUES (@id)");
                                        //}

                                        // TODO: Inserir o senador se não existir e importar os detalhes do mandato
                                    }
                                }
                                #endregion Mandato

                                if (mandato.DescricaoParticipacao == "Titular")
                                {
                                    #region Mandato Legislatura

                                    var lstLegislaturaMandato = new List<ALegislaturaDoMandato>();
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
                                Console.WriteLine(ex.ToFullDescriptionString());
                            }
                        }
                    }
                }

                #endregion Atualizar senadores ativos e os que estavam ativos antes da importação
            }

            return string.Empty;
        }

        public static string AtualizaCadastroSenadores2()
        {
            using (var banco = new AppDb())
            {
                var lstSenadorNovo = new List<int>();
                var lstSenador = new List<int>();
                using (var reader = banco.ExecuteReader("SELECT s.id FROM sf_senador s left join sf_mandato m ON m.id_sf_senador = s.id WHERE m.id IS null"))
                {
                    while (reader.Read())
                    {
                        lstSenador.Add(Convert.ToInt32(reader["id"]));
                    }
                }
                using (var reader = banco.ExecuteReader("SELECT id FROM sf_senador order by id"))
                {
                    while (reader.Read())
                    {
                        lstSenadorNovo.Add(Convert.ToInt32(reader["id"]));
                    }
                }

                var client = new RestClient();

                #region Atualizar senadores ativos e os que estavam ativos antes da importação

                var lstLegislatura = new List<int>();
                using (var reader = banco.ExecuteReader("SELECT id FROM sf_legislatura"))
                {
                    while (reader.Read())
                    {
                        lstLegislatura.Add(Convert.ToInt32(reader["id"]));
                    }
                }

                var lstMotivoAfastamento = new List<string>();
                using (var reader = banco.ExecuteReader("SELECT id FROM sf_motivo_afastamento"))
                {
                    while (reader.Read())
                    {
                        lstMotivoAfastamento.Add(reader["id"].ToString());
                    }
                }

                var lstProfissao = new Dictionary<string, int>();
                using (var reader = banco.ExecuteReader("SELECT id, descricao FROM profissao"))
                {
                    while (reader.Read())
                    {
                        lstProfissao.Add(reader["descricao"].ToString(), Convert.ToInt32(reader["id"]));
                    }
                }

                foreach (var idSenador in lstSenador)
                {
                    //var request = new RestRequest("http://legis.senado.gov.br/dadosabertos/senador/" + idSenador, Method.GET);
                    //request.AddHeader("Accept", "application/json");

                    //var resSenador = client.Execute(request);
                    //JObject jSenador = JObject.Parse(resSenador.Content);
                    //JObject arrParlamentar = (JObject)jSenador["DetalheParlamentar"]["Parlamentar"];

                    //var parlamentarCompleto = arrParlamentar.ToObject<Parlamentar>();
                    //Console.WriteLine("Consultando dados do senador {0}: {1}", idSenador, parlamentarCompleto.IdentificacaoParlamentar.NomeParlamentar);

                    //var parlamentar = parlamentarCompleto.IdentificacaoParlamentar;

                    //if (string.IsNullOrEmpty(parlamentar.UfParlamentar))
                    //{
                    //    try
                    //    {
                    //        var jUltimoMandato = jSenador["DetalheParlamentar"]["Parlamentar"]["UltimoMandato"];
                    //        if (jUltimoMandato is JArray)
                    //            parlamentar.UfParlamentar = jUltimoMandato[0]["UfParlamentar"].ToString();
                    //        else
                    //            parlamentar.UfParlamentar = jUltimoMandato["UfParlamentar"].ToString();
                    //    }
                    //    catch (Exception)
                    //    {  }
                    //}

                    //if (string.IsNullOrEmpty(parlamentar.NomeParlamentar))
                    //{
                    //    parlamentar.NomeParlamentar = parlamentar.NomeCompletoParlamentar;
                    //}

                    //banco.AddParameter("id", parlamentar.CodigoParlamentar);
                    //banco.AddParameter("codigo", parlamentar.CodigoPublicoNaLegAtual);
                    //banco.AddParameter("nome", parlamentar.NomeParlamentar);
                    //banco.AddParameter("nome_completo", parlamentar.NomeCompletoParlamentar);
                    //banco.AddParameter("sexo", parlamentar.SexoParlamentar[0].ToString());
                    //banco.AddParameter("sigla_partido", parlamentar.SiglaPartidoParlamentar);
                    //banco.AddParameter("sigla_uf", parlamentar.UfParlamentar);
                    //banco.AddParameter("email", parlamentar.EmailParlamentar);
                    //banco.AddParameter("site", parlamentar.UrlPaginaParticular);

                    //var dadosBasicos = parlamentarCompleto.DadosBasicosParlamentar ?? new DadosBasicosParlamentar();
                    //banco.AddParameter("nascimento", dadosBasicos.DataNascimento);
                    //banco.AddParameter("naturalidade", dadosBasicos.Naturalidade);
                    //banco.AddParameter("sigla_uf_naturalidade", dadosBasicos.UfNaturalidade);

                    //var lstSenadorProfissao = new List<Profissao>();
                    //if (arrParlamentar["Profissoes"] != null)
                    //{
                    //    if (arrParlamentar["Profissoes"]["Profissao"] is JArray)
                    //    {
                    //        lstSenadorProfissao = arrParlamentar["Profissoes"]["Profissao"].ToObject<List<Profissao>>();
                    //    }
                    //    else
                    //    {
                    //        lstSenadorProfissao.Add(arrParlamentar["Profissoes"]["Profissao"].ToObject<Profissao>());
                    //    }
                    //}
                    //if (lstSenadorProfissao.Any())
                    //{
                    //    banco.AddParameter("profissao", string.Join(", ", lstSenadorProfissao.Select(obj => obj.NomeProfissao)));
                    //}
                    //else
                    //{
                    //    banco.AddParameter("profissao", DBNull.Value);
                    //}

                    ////banco.AddParameter("ativo", lstSenadorAtivo.Contains(parlamentar.CodigoParlamentar) ? "S" : "N");

                    //banco.ExecuteNonQuery(@"
                    //    UPDATE sf_senador SET
                    //        codigo = @codigo
                    //        , nome = @nome
                    //        , nome_completo = @nome_completo
                    //        , sexo = @sexo
                    //        , nascimento = @nascimento
                    //        , naturalidade = @naturalidade
                    //        , id_estado_naturalidade = (SELECT id FROM estado where sigla like @sigla_uf_naturalidade)
                    //        , profissao = @profissao
                    //        , id_partido = (SELECT id FROM partido where sigla like @sigla_partido OR nome like @sigla_partido)
                    //        , id_estado = (SELECT id FROM estado where sigla like @sigla_uf)
                    //        , email = @email
                    //        , site = @site
                    //    WHERE id = @id
                    //");

                    //if (lstSenadorProfissao.Any())
                    //{
                    //    foreach (var profissao in lstSenadorProfissao)
                    //    {
                    //        if (!lstProfissao.ContainsKey(profissao.NomeProfissao))
                    //        {
                    //            banco.AddParameter("descricao", profissao.NomeProfissao);
                    //            var idProfissao = banco.ExecuteScalar(@"INSERT INTO profissao (descricao) values (@descricao); SELECT LAST_INSERT_ID();");

                    //            lstProfissao.Add(profissao.NomeProfissao, Convert.ToInt32(idProfissao));
                    //        }

                    //        banco.AddParameter("id_sf_senador", idSenador);
                    //        banco.AddParameter("id_profissao", lstProfissao[profissao.NomeProfissao]);

                    //        banco.ExecuteNonQuery(@"INSERT IGNORE INTO sf_senador_profissao (id_sf_senador, id_profissao) values (@id_sf_senador, @id_profissao)");
                    //    }

                    //}

                    //var lstHistoricoAcademico = new List<Curso>();
                    //if (arrParlamentar["HistoricoAcademico"] != null)
                    //{
                    //    if (arrParlamentar["HistoricoAcademico"]["Curso"] is JArray)
                    //    {
                    //        lstHistoricoAcademico = arrParlamentar["HistoricoAcademico"]["Curso"].ToObject<List<Curso>>();
                    //    }
                    //    else
                    //    {
                    //        lstHistoricoAcademico.Add(arrParlamentar["HistoricoAcademico"]["Curso"].ToObject<Curso>());
                    //    }
                    //}
                    //foreach (var curso in lstHistoricoAcademico)
                    //{
                    //    banco.AddParameter("id_sf_senador", idSenador);
                    //    banco.AddParameter("nome_curso", curso.NomeCurso);
                    //    banco.AddParameter("grau_instrucao", curso.GrauInstrucao);
                    //    banco.AddParameter("estabelecimento", curso.Estabelecimento);
                    //    banco.AddParameter("local", curso.Local);

                    //    banco.ExecuteNonQuery(@"
                    //        INSERT IGNORE INTO sf_senador_historico_academico (id_sf_senador, nome_curso, grau_instrucao, estabelecimento, local) 
                    //        values (@id_sf_senador, @nome_curso, @grau_instrucao, @estabelecimento, @local)");
                    //}


                    var request = new RestRequest("http://legis.senado.gov.br/dadosabertos/senador/" + idSenador.ToString() + "/mandatos", Method.GET);
                    request.AddHeader("Accept", "application/json");

                    IRestResponse resSenadorMandato = client.Execute(request);
                    JObject jSenadorMandato = JObject.Parse(resSenadorMandato.Content);
                    JToken arrPalamentar = jSenadorMandato["MandatoParlamentar"]["Parlamentar"];
                    JToken arrMandatosPalamentar = arrPalamentar["Mandatos"];

                    Console.WriteLine("Consultando mandatos do senador {0}: {1}", idSenador, arrPalamentar["IdentificacaoParlamentar"]["NomeParlamentar"].ToString());

                    if (arrMandatosPalamentar != null)
                    {
                        JArray arrMandatoParlamentar = (JArray)arrMandatosPalamentar["Mandato"];

                        foreach (var jMandato in arrMandatoParlamentar)
                        {
                            var mandato = jMandato.ToObject<Mandato>();
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

                                if (banco.Rows == 0)
                                {
                                    Console.WriteLine("Mandato não inserido!");
                                }

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

                                    var lstLegislaturaMandato = new List<ALegislaturaDoMandato>();
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

                                        if (banco.Rows == 0)
                                        {
                                            Console.WriteLine("Legislatura do mandato não inserido!");
                                        }
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

                                        if (banco.Rows == 0)
                                        {
                                            Console.WriteLine("Exercício não inserido!");
                                        }
                                    }
                                }
                                #endregion Mandato Exercicio
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToFullDescriptionString());
                            }
                        }
                    }
                }

                #endregion Atualizar senadores ativos e os que estavam ativos antes da importação
            }

            return string.Empty;
        }

        public static string ImportarDespesas(string atualDir, int ano, bool completo)
        {
            var downloadUrl = string.Format("http://www.senado.gov.br/transparencia/LAI/verba/{0}.csv", ano);
            var fullFileNameCsv = System.IO.Path.Combine(atualDir, ano + ".csv");

            if (!Directory.Exists(atualDir))
                Directory.CreateDirectory(atualDir);

            var request = (HttpWebRequest)WebRequest.Create(downloadUrl);

            request.UserAgent = "Other";
            request.Method = "HEAD";
            request.ContentType = "application/json;charset=UTF-8";
            request.Timeout = 1000000;

            using (var resp = request.GetResponse())
            {
                if (File.Exists(fullFileNameCsv))
                {
                    var ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
                    long ContentLengthLocal = 0;

                    if (File.Exists(fullFileNameCsv))
                        ContentLengthLocal = new FileInfo(fullFileNameCsv).Length;

                    if (!completo && ContentLength == ContentLengthLocal)
                    {
                        Console.WriteLine("Não há novos itens para importar!");
                        return "<p>Não há novos itens para importar!</p>";
                    }
                }

                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent: Other");
                    client.DownloadFile(downloadUrl, fullFileNameCsv);
                }
            }

            try
            {
                var resumoImportacao = CarregaDadosCsv(fullFileNameCsv, ano, completo);

                using (var banco = new AppDb())
                {
                    banco.ExecuteNonQuery(@"
					UPDATE parametros SET sf_senador_ultima_atualizacao=NOW();
				");
                }

                return resumoImportacao;
            }
            catch (Exception ex)
            {
                // Excluir o arquivo para tentar importar novamente na proxima execução
                File.Delete(fullFileNameCsv);

                return "Erro ao importar:" + ex.ToFullDescriptionString();
            }
        }

        private static string CarregaDadosCsv(string file, int ano, bool completo)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            var sb = new StringBuilder();
            string sResumoValores = string.Empty;

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

            int linhaAtual = 0;

            using (var banco = new AppDb())
            {
                var lstHash = new List<string>();
                using (var dReader = banco.ExecuteReader("select hash from sf_despesa where ano=" + ano))
                {
                    while (dReader.Read())
                    {
                        try
                        {
                            lstHash.Add(dReader["hash"].ToString());
                        }
                        catch (Exception)
                        {
                            // Vai ter duplicado mesmo
                        }
                    }
                }

                using (var dReader = banco.ExecuteReader("select sum(valor) as valor, count(1) as itens from sf_despesa where ano=" + ano))
                {
                    if (dReader.Read())
                    {
                        sResumoValores = string.Format("[{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
                    }
                }

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

                        string hash = banco.ParametersHash();
                        if (lstHash.Remove(hash))
                        {
                            banco.ClearParameters();
                            continue;
                        }

                        banco.AddParameter("hash", hash);

                        banco.ExecuteNonQuery(
                            @"INSERT INTO sf_despesa_temp (
								ano, mes, senador, tipo_despesa, cnpj_cpf, fornecedor, documento, `data`, detalhamento, valor_reembolsado, cod_documento, hash
							) VALUES (
								@ano, @mes, @senador, @tipo_despesa, @cnpj_cpf, @fornecedor, @documento, @data, @detalhamento, @valor_reembolsado, @cod_documento, @hash
							)");
                    }

                    if (++linhaAtual % 100 == 0)
                    {
                        Console.WriteLine(linhaAtual);
                    }
                }

                if (lstHash.Count == 0 && linhaAtual == 0)
                {
                    sb.AppendFormat("<p>Não há novos itens para importar! #2</p>");
                    return sb.ToString();
                }

                if (lstHash.Count > 0)
                {
                    foreach (var hash in lstHash)
                    {
                        banco.ExecuteNonQuery(string.Format("update sf_despesa set deletado = NOW() where hash = '{0}'", hash));
                    }


                    Console.WriteLine("Registros para exluir: " + lstHash.Count);
                    sb.AppendFormat("<p>{0} registros excluidos</p>", lstHash.Count);
                }

                sb.Append(ProcessarDespesasTemp(banco, completo));
            }

            if (ano == DateTime.Now.Year)
            {
                AtualizaSenadorValores();
                AtualizaCampeoesGastos();
                AtualizaResumoMensal();
            }

            using (var banco = new AppDb())
            {
                using (var dReader = banco.ExecuteReader("select sum(valor) as valor, count(1) as itens from sf_despesa where ano=" + ano))
                {
                    if (dReader.Read())
                    {
                        sResumoValores += string.Format(" -> [{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
                    }
                }

                sb.AppendFormat("<p>Resumo atualização: {0}</p>", sResumoValores);
            }

            return sb.ToString();
        }

        private static string ProcessarDespesasTemp(AppDb banco, bool completo)
        {
            var sb = new StringBuilder();

            CorrigeDespesas(banco);
            sb.Append(InsereSenadorFaltante(banco));
            sb.Append(InsereFornecedorFaltante(banco));

            //if (completo)
            sb.Append(InsereDespesaFinal(banco));
            //else
            //	InsereDespesaFinalParcial(banco);

            LimpaDespesaTemporaria(banco);

            return sb.ToString();
        }

        private static void CorrigeDespesas(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				UPDATE sf_despesa_temp 
				SET tipo_despesa = 'Aquisição de material de consumo para uso no escritório político' 
				WHERE tipo_despesa LIKE 'Aquisição de material de consumo para uso no escritório político%';

				UPDATE sf_despesa_temp 
				SET tipo_despesa = 'Contratação de consultorias, assessorias, pesquisas, trabalhos técnicos e outros serviços' 
				WHERE tipo_despesa LIKE 'Contratação de consultorias, assessorias, pesquisas, trabalhos técnicos e outros serviços%';	
			");
        }

        private static string InsereSenadorFaltante(AppDb banco)
        {
            //object total = banco.ExecuteScalar(@"select count(1) from sf_despesa_temp where senador  not in (select ifnull(nome_importacao, nome) from sf_senador);");
            //if (Convert.ToInt32(total) > 0)
            //{
            //	CarregaSenadoresAtuais();

            object total = banco.ExecuteScalar(@"select count(1) from sf_despesa_temp where senador  not in (select ifnull(nome_importacao, nome) from sf_senador);");
            if (Convert.ToInt32(total) > 0)
            {
                throw new Exception("Existem despesas de senadores que não estão cadastrados!");
            }
            //}

            return string.Empty;
        }

        private static string InsereFornecedorFaltante(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				INSERT INTO fornecedor (nome, cnpj_cpf)
				select MAX(dt.fornecedor), dt.cnpj_cpf
				from sf_despesa_temp dt
				left join fornecedor f on f.cnpj_cpf = dt.cnpj_cpf
				where dt.cnpj_cpf is not null
				and f.id is null
				GROUP BY dt.cnpj_cpf;
			");

            if (banco.Rows > 0)
            {
                return "<p>" + banco.Rows + "+ Fornecedor</p>";
            }

            return string.Empty;
        }

        private static string InsereDespesaFinal(AppDb banco)
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
					data_documento,
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
				FROM sf_despesa_temp d
				inner join sf_senador p on ifnull(p.nome_importacao, p.nome) = d.senador
				inner join sf_despesa_tipo dt on dt.descricao = d.tipo_despesa
				inner join fornecedor f on f.cnpj_cpf = d.cnpj_cpf;
    
				ALTER TABLE sf_despesa ENABLE KEYS;
			", 3600);


            if (banco.Rows > 0)
            {
                retorno = +banco.Rows + "+ Despesa nova! ";
            }

            banco.ExecuteNonQuery(@"
				UPDATE sf_despesa_temp t 
				inner join sf_senador p on ifnull(p.nome_importacao, p.nome) = t.senador
				inner join sf_despesa_tipo dt on dt.descricao = t.tipo_despesa
				inner join fornecedor f on f.cnpj_cpf = t.cnpj_cpf
                inner join sf_despesa d on d.id = t.cod_documento and p.id = d.id_sf_senador
                set
					d.id_sf_despesa_tipo = dt.id,
					d.id_fornecedor = f.id,
					d.ano_mes = concat(t.ano, LPAD(t.mes, 2, '0')),
					d.ano = t.ano,
					d.mes = t.mes,
					d.documento = t.documento,
					d.data_documento = t.`data`,
					d.detalhamento = t.detalhamento,
					d.valor = t.valor_reembolsado,
					d.hash = t.hash,
				    d.deletado = null
                where deletado is not null
			", 3600);

            if (banco.Rows > 0)
            {
                retorno += banco.Rows + "+ Despesa alterada! ";
            }

            if (!string.IsNullOrEmpty(retorno.Trim()))
            {
                return "<p>" + retorno.Trim() + "</p>";
            }
            return string.Empty;
        }

        private static void InsereDespesaFinalParcial(AppDb banco)
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
	                from sf_despesa_temp d
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
					        data_documento,
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
						    from sf_despesa_temp d
                            inner join sf_senador p on p.nome = d.senador
						    WHERE p.id=@id_sf_senador and mes=@mes
					    ) d
				        inner join sf_despesa_tipo dt on dt.descricao = d.tipo_despesa
				        inner join fornecedor f on f.cnpj_cpf = d.cnpj_cpf;
			        ", 3600);
            }
        }

        private static void LimpaDespesaTemporaria(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				truncate table sf_despesa_temp;
			");
        }

        public static void AtualizaSenadorValores()
        {
            using (var banco = new AppDb())
            {
                banco.ExecuteNonQuery("UPDATE sf_senador SET valor_total_ceaps=0;");

                var dt = banco.GetTable(@"select id from sf_senador
						WHERE id IN (
						select distinct id_sf_senador
						from sf_despesa
					)");
                object valor_total_ceaps;

                foreach (DataRow dr in dt.Rows)
                {
                    banco.AddParameter("id_sf_senador", dr["id"]);
                    valor_total_ceaps =
                        banco.ExecuteScalar("select sum(valor) from sf_despesa where id_sf_senador=@id_sf_senador;");

                    banco.AddParameter("valor_total_ceaps", valor_total_ceaps);
                    banco.AddParameter("id_sf_senador", dr["id"]);
                    banco.ExecuteNonQuery(
                        @"update sf_senador set 
						valor_total_ceaps=@valor_total_ceaps
						where id=@id_sf_senador"
                    );
                }
            }
        }

        public static void AtualizaCampeoesGastos()
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

            using (var banco = new AppDb())
            {
                banco.ExecuteNonQuery(strSql);
            }
        }

        public static void AtualizaResumoMensal()
        {
            var strSql =
                @"truncate table sf_despesa_resumo_mensal;
					insert into sf_despesa_resumo_mensal
					(ano, mes, valor) (
						select ano, mes, sum(valor)
						from sf_despesa
						group by ano, mes
					);";

            using (var banco = new AppDb())
            {
                banco.ExecuteNonQuery(strSql);
            }
        }

        public static string DownloadFotosSenadores(string dirRaiz)
        {
            var db = new StringBuilder();

            using (var banco = new AppDb())
            {
                DataTable table = banco.GetTable("SELECT id FROM sf_senador where valor_total_ceaps > 0 or ativo = 'S'");

                foreach (DataRow row in table.Rows)
                {
                    string id = row["id"].ToString();
                    string url = "https://www.senado.leg.br/senadores/img/fotos-oficiais/senador" + id + ".jpg";
                    string src = dirRaiz + id + ".jpg";
                    if (File.Exists(src)) continue;

                    try
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.Headers.Add("User-Agent: Other");
                            client.DownloadFile(url, src);

                            ImportacaoUtils.CreateImageThumbnail(src, 120, 160);
                            ImportacaoUtils.CreateImageThumbnail(src, 240, 300);

                            db.AppendLine("Atualizado imagem do senador " + id);
                            Console.WriteLine("Atualizado imagem do senador " + id);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!ex.Message.Contains("404"))
                        {
                            db.AppendLine("Imagem do senador " + id + " inexistente! Motivo: " + ex.ToFullDescriptionString());
                            Console.WriteLine("Imagem do senador " + id + " inexistente! Motivo: " + ex.ToFullDescriptionString());
                            //ignore
                        }
                    }
                }
            }

            return db.ToString();
        }

        public static string ImportarRemuneracao(string atualDir, int anomes)
        {
            var downloadUrl = string.Format("http://www.senado.leg.br/transparencia/LAI/secrh/SF_ConsultaRemuneracaoServidoresParlamentares_{0}.csv", anomes);
            var fullFileNameCsv = System.IO.Path.Combine(atualDir, anomes + ".csv");

            if (!Directory.Exists(atualDir))
                Directory.CreateDirectory(atualDir);

            var request = (HttpWebRequest)WebRequest.Create(downloadUrl);

            request.UserAgent = "Other";
            request.Method = "HEAD";
            request.ContentType = "application/json;charset=UTF-8";
            request.Timeout = 1000000;

            using (var resp = request.GetResponse())
            {
                if (File.Exists(fullFileNameCsv))
                {
                    var ContentLength = Convert.ToInt64(resp.Headers.Get("Content-Length"));
                    long ContentLengthLocal = 0;

                    if (File.Exists(fullFileNameCsv))
                        ContentLengthLocal = new FileInfo(fullFileNameCsv).Length;

                    if (ContentLength == ContentLengthLocal)
                    {
                        Console.WriteLine("Não há novos itens para importar!");
                        return "<p>Não há novos itens para importar!</p>";
                    }
                }

                using (var client = new WebClient())
                {
                    client.Headers.Add("User-Agent: Other");
                    client.DownloadFile(downloadUrl, fullFileNameCsv);
                }
            }

            try
            {
                var resumoImportacao = CarregaRemuneracaoCsv(fullFileNameCsv, anomes);

                //            using (var banco = new AppDb())
                //            {
                //                banco.ExecuteNonQuery(@"
                //	UPDATE parametros SET sf_senador_ultima_atualizacao=NOW();
                //");
                //            }

                return resumoImportacao;
            }
            catch (Exception ex)
            {
                // Excluir o arquivo para tentar importar novamente na proxima execução
                File.Delete(fullFileNameCsv);

                return "Erro ao importar:" + ex.ToFullDescriptionString();
            }
        }

        private static string CarregaRemuneracaoCsv(string file, int anomes)
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
                LimpaRemuneracaoTemporaria(banco);

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
                                @"INSERT INTO sf_remuneracao_temp (
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
                                Console.WriteLine(linha);
                                ProcessarRemuneracaoTemp(banco);
                            }
                        }
                    }
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

            return sb.ToString();
        }

        private static void ProcessarRemuneracaoTemp(AppDb banco)
        {
            var total = banco.ExecuteScalar(@"select count(1) from sf_remuneracao_temp");

            banco.ExecuteNonQuery(@"
                INSERT IGNORE INTO sf_funcao  (descricao)
                SELECT DISTINCT simbolo_funcao FROM sf_remuneracao_temp WHERE simbolo_funcao <> '' ORDER BY simbolo_funcao;

                INSERT IGNORE INTO sf_cargo  (descricao)
                SELECT DISTINCT cargo FROM sf_remuneracao_temp WHERE cargo <> '' ORDER BY cargo;

                INSERT INTO sf_categoria  (descricao)
                SELECT DISTINCT categoria FROM sf_remuneracao_temp WHERE categoria <> '' ORDER BY categoria;

                INSERT IGNORE INTO sf_vinculo  (descricao)
                SELECT DISTINCT vinculo FROM sf_remuneracao_temp WHERE vinculo <> '' ORDER BY vinculo;

                INSERT IGNORE INTO sf_referencia_cargo  (descricao)
                SELECT DISTINCT referencia_cargo FROM sf_remuneracao_temp WHERE referencia_cargo IS NOT NULL ORDER BY referencia_cargo;

                INSERT IGNORE INTO sf_lotacao( descricao)
                SELECT DISTINCT lotacao_exercicio FROM sf_remuneracao_temp WHERE lotacao_exercicio <> '' ORDER BY lotacao_exercicio;
            ", 3600);

            banco.ExecuteNonQuery(@"
                INSERT INTO sf_remuneracao 
	                (id_vinculo, id_categoria, id_cargo, id_referencia_cargo, id_simbolo_funcao, id_lotacao, id_tipo_folha, ano_mes, admissao, remun_basica, vant_pessoais, func_comissionada, grat_natalina, horas_extras, outras_eventuais, abono_permanencia, reversao_teto_const, imposto_renda, previdencia, faltas, rem_liquida, diarias, auxilios, vant_indenizatorias, custo_total)
                SELECT 
	                v.id AS vinculo, ct.id AS categoria, cr.id AS cargo, rc.id as referencia_cargo, sf.id as simbolo_funcao, le.id as lotacao_exercicio, tf.id as tipo_folha, ano_mes, admissao, remun_basica, vant_pessoais, func_comissionada, grat_natalina, horas_extras, outras_eventuais, abono_permanencia, reversao_teto_const, imposto_renda, previdencia, faltas, rem_liquida, diarias, auxilios, vant_indenizatorias,
	                -- (remun_basica + vant_pessoais + func_comissionada + grat_natalina + horas_extras + outras_eventuais + abono_permanencia + reversao_teto_const + faltas + diarias + auxilios + vant_indenizatorias) AS custo_total,
	                (rem_liquida - imposto_renda - previdencia - faltas + diarias + auxilios + vant_indenizatorias) AS custo_total
                FROM sf_remuneracao_temp tmp
                JOIN sf_vinculo v ON v.descricao LIKE tmp.vinculo
                JOIN sf_categoria ct ON ct.descricao LIKE tmp.categoria
                LEFT JOIN sf_cargo cr ON cr.descricao LIKE tmp.cargo
                LEFT JOIN sf_referencia_cargo rc ON rc.descricao LIKE tmp.referencia_cargo
                LEFT JOIN sf_funcao sf ON sf.descricao LIKE tmp.simbolo_funcao
                JOIN sf_lotacao le ON le.descricao LIKE tmp.lotacao_exercicio
                JOIN sf_tipo_folha tf ON tf.descricao LIKE tmp.tipo_folha
			", 3600);

            if (Convert.ToInt32(total) != banco.Rows)
            {
                throw new Exception("Existem relacionamentos não mapeados!");
            }

            LimpaRemuneracaoTemporaria(banco);
        }

        private static void LimpaRemuneracaoTemporaria(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				truncate table sf_remuneracao_temp;
			");
        }
    }
}