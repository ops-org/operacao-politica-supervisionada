using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPS.Core.Utilities;
using OPS.Importador.Comum;
using OPS.Importador.Comum.Parlamentar;
using OPS.Importador.Comum.Utilities;
using OPS.Importador.SenadoFederal.Entities;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.SenadoFederal;
using RestSharp;

namespace OPS.Importador.SenadoFederal;

public class SenatorLists
{
    public List<int> ActiveSenators { get; set; } = new();
    public List<int> AllSenators { get; set; } = new();
    public List<int> NewSenators { get; set; } = new();
}

public class ImportadorParlamentarSenado : IImportadorParlamentar
{
    protected readonly ILogger<ImportadorParlamentarSenado> logger;
    protected readonly AppSettings appSettings;
    protected readonly AppDbContext dbContext;
    protected readonly HttpClient httpClient;

    public List<PartidoDePara> PartidoDeParas { get; init; }

    public ImportadorParlamentarSenado(IServiceProvider serviceProvider)
    {
        logger = serviceProvider.GetService<ILogger<ImportadorParlamentarSenado>>();
        dbContext = serviceProvider.GetService<AppDbContext>();

        appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient("ResilientClient");

        PartidoDeParas = dbContext.PartidoDeParas.ToList();
    }

    public Task Importar()
    {
        //using (var dbContext = dbContextFactory.CreateDbContext())
        //using (var transaction = dbContext.Database.BeginTransaction())
        {
            try
            {
                var senatorLists = GetSenatorLists(dbContext);

                // Mark all senators as inactive
                dbContext.Senadores
                    .Where(x => x.Ativo == "S")
                    .ExecuteUpdate(x => x.SetProperty(y => y.Ativo, "N"));

                var restClient = new RestClient(httpClient);
                ImportActiveSenators(restClient, senatorLists); // Can Change senatorLists

                UpdateSenatorDetails(restClient, senatorLists);

                //transaction.Commit();
            }
            catch (Exception ex)
            {
                //transaction.Rollback();
                logger.LogError(ex, "Error during senator import");
                throw;
            }
        }

        return Task.CompletedTask;
    }

    private SenatorLists GetSenatorLists(AppDbContext dbContext)
    {
        return new SenatorLists
        {
            ActiveSenators = new List<int>(), // Prenchido no ImportActiveSenators
            AllSenators = dbContext.Senadores
                .OrderBy(s => s.Id)
                .Select(s => s.Id)
                .ToList(),
            NewSenators = dbContext.Senadores // Itens para forçar atualização, Incrementado depois no ImportActiveSenators com novos Suplentes
                .Where(s => s.Ativo == "S" || s.Nome == null)
                .OrderBy(s => s.Id)
                .Select(s => s.Id)
                .ToList()
        };
    }

    private void ImportActiveSenators(RestClient restClient, SenatorLists senatorLists)
    {
        #region Importar senadores ativos
        var request = new RestRequest("https://legis.senado.gov.br/dadosabertos/senador/lista/atual");
        request.AddHeader("Accept", "application/json");

        RestResponse resSenadores = restClient.Get(request);
        JsonDocument jSenadores = JsonDocument.Parse(resSenadores.Content);
        JsonElement arrIdentificacaoParlamentar = jSenadores.RootElement.GetProperty("ListaParlamentarEmExercicio").GetProperty("Parlamentares").GetProperty("Parlamentar");
        var senadores = arrIdentificacaoParlamentar.EnumerateArray();

        while (senadores.MoveNext())
        {
            try
            {
                var senador = senadores.Current;
                var parlamentar = JsonSerializer.Deserialize<IdentificacaoParlamentar>(senador.GetProperty("IdentificacaoParlamentar"));

                var codigoParlamentar = Convert.ToInt32(parlamentar.CodigoParlamentar);
                var existe = dbContext.Senadores.Find(codigoParlamentar);
                if (existe == null)
                {
                    logger.LogInformation("Novo Senador {IdSenador}: {NomeParlamentar}", parlamentar.CodigoParlamentar, parlamentar.NomeParlamentar);

                    var partido = dbContext.Partidos
                        .FirstOrDefault(p => EF.Functions.ILike(p.Sigla, parlamentar.SiglaPartidoParlamentar) ||
                                             EF.Functions.ILike(p.Nome, parlamentar.SiglaPartidoParlamentar));

                    var estado = dbContext.Estados
                        .FirstOrDefault(e => EF.Functions.ILike(e.Sigla, parlamentar.UfParlamentar));

                    var novoSenador = new Senador
                    {
                        Id = Convert.ToInt32(parlamentar.CodigoParlamentar),
                        Codigo = Convert.ToInt32(parlamentar.CodigoPublicoNaLegAtual),
                        Nome = parlamentar.NomeParlamentar,
                        NomeCompleto = parlamentar.NomeCompletoParlamentar,
                        Sexo = parlamentar.SexoParlamentar[0].ToString(),
                        IdPartido = partido?.Id ?? 0,
                        IdEstado = estado?.Id,
                        Email = parlamentar.EmailParlamentar,
                        Site = parlamentar.UrlPaginaParticular,
                        Ativo = "S"
                    };

                    dbContext.Senadores.Add(novoSenador);
                    senatorLists.AllSenators.Add(Convert.ToInt32(parlamentar.CodigoParlamentar));
                }
                else
                {
                    var idSenador = Convert.ToInt32(parlamentar.CodigoParlamentar);
                    senatorLists.ActiveSenators.Add(idSenador);

                    if (!senatorLists.AllSenators.Contains(idSenador))
                    {
                        senatorLists.AllSenators.Add(idSenador);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        dbContext.SaveChanges();
        #endregion Importar senadores ativos
    }

    private void UpdateSenatorDetails(RestClient restClient, SenatorLists senatorLists)
    {
        #region Atualizar senadores ativos e os que estavam ativos antes da importação
        var urlApiSenador = "https://legis.senado.gov.br/dadosabertos/senador/";

        var lstLegislatura = dbContext.LegislaturasSenado
            .OrderBy(l => l.Id)
            .Select(l => l.Id.ToString())
            .ToList();

        var lstMotivoAfastamento = dbContext.MotivoAfastamentos
            .OrderBy(m => m.Id)
            .Select(m => m.Id.ToString())
            .ToList();

        var lstProfissao = dbContext.Profissoes
            .ToDictionary(p => p.Descricao, p => (int)p.Id);

        // Atualizar Senadores ativos
        UpdateActiveSenatorDetails(restClient, senatorLists, urlApiSenador, lstLegislatura, lstMotivoAfastamento, lstProfissao);

        // Atualizar novo senadores (o metodo acima pode incluir novos senadores, ex: suplentes)
        UpdateActiveSenatorDetails(restClient, senatorLists, urlApiSenador, lstLegislatura, lstMotivoAfastamento, lstProfissao, onlyNew: true);

        #endregion Atualizar senadores ativos e os que estavam ativos antes da importação
    }

    private void UpdateActiveSenatorDetails(RestClient restClient, SenatorLists senatorLists, string urlApiSenador, List<string> lstLegislatura, List<string> lstMotivoAfastamento, Dictionary<string, int> lstProfissao, bool onlyNew = false)
    {
        var senadoresParaAtualizar = onlyNew ? senatorLists.NewSenators.ToList() : senatorLists.ActiveSenators.ToList();
        var indice = 0;
        var totalSenadores = senadoresParaAtualizar.Count;

        foreach (var idSenador in senadoresParaAtualizar)
        {
            var request = new RestRequest(urlApiSenador + idSenador);
            request.AddHeader("Accept", "application/json");

            var resSenador = restClient.Get(request);
            JsonDocument jSenador = JsonDocument.Parse(resSenador.Content);
            SenadorDetalhes senador = JsonSerializer.Deserialize<SenadorDetalhes>(resSenador.Content);
            var parlamentar = senador.DetalheParlamentar.Parlamentar;
            var identificacaoParlamentar = parlamentar.IdentificacaoParlamentar;
            var senadorToUpdate = dbContext.Senadores.Find(Convert.ToInt32(identificacaoParlamentar.CodigoParlamentar));
            if (senadorToUpdate == null) continue;

            var jParlamentar = jSenador.RootElement.GetProperty("DetalheParlamentar").GetProperty("Parlamentar");

            logger.LogDebug("{IndiceAtual}/{Total} Consultando Senador {IdSenador}: {NomeParlamentar}",
                ++indice, totalSenadores, identificacaoParlamentar.CodigoParlamentar, identificacaoParlamentar.NomeParlamentar);

            ProcessSenatorPhones(jParlamentar, parlamentar);

            if (string.IsNullOrEmpty(identificacaoParlamentar.NomeParlamentar))
            {
                identificacaoParlamentar.NomeParlamentar = identificacaoParlamentar.NomeCompletoParlamentar;
            }

            var lstSenadorProfissao = GetSenatorProfessions(restClient, urlApiSenador, idSenador, senador);

            ProcessSenatorBasicData(senadorToUpdate, identificacaoParlamentar, lstSenadorProfissao, senatorLists);

            if (lstSenadorProfissao.Any())
            {
                ProcessSenatorProfessions(idSenador, lstSenadorProfissao, lstProfissao);
            }

            ProcessSenatorAcademicHistory(restClient, urlApiSenador, idSenador);

            ProcessSenatorMandates(restClient, urlApiSenador, idSenador, lstLegislatura, lstMotivoAfastamento, senatorLists);

            dbContext.SaveChanges();
            dbContext.ChangeTracker.Clear();
        }
    }

    private void ProcessSenatorPhones(JsonElement jParlamentar, SenadorParlamentar parlamentar)
    {
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
    }

    private List<Entities.Profissao> GetSenatorProfessions(RestClient restClient, string urlApiSenador, int idSenador, SenadorDetalhes senador)
    {
        var request = new RestRequest(urlApiSenador + idSenador.ToString() + "/profissao?v=1");
        request.AddHeader("Accept", "application/json");

        RestResponse resProfissoes = restClient.Get(request);
        var lstSenadorProfissao = new List<Entities.Profissao>();

        if (!resProfissoes.Content.Contains("HistoricoAcademicoParlamentar"))
        {
            JsonDocument jSenadorProfissoes = JsonDocument.Parse(resProfissoes.Content);
            var jParlamentarProfissoes = jSenadorProfissoes.RootElement.GetProperty("ProfissaoParlamentar").GetProperty("Parlamentar");

            JsonElement jProfissoes;
            if (jParlamentarProfissoes.TryGetProperty("Profissoes", out jProfissoes))
            {
                if (jProfissoes.GetProperty("Profissao").ValueKind == JsonValueKind.Array)
                {
                    lstSenadorProfissao = JsonSerializer.Deserialize<List<Entities.Profissao>>(jProfissoes.GetProperty("Profissao"));
                }
                else
                {
                    lstSenadorProfissao.Add(JsonSerializer.Deserialize<Entities.Profissao>(jProfissoes.GetProperty("Profissao")));
                }
            }
        }

        return lstSenadorProfissao;
    }

    private void ProcessSenatorBasicData(Senador senadorToUpdate, IdentificacaoParlamentar identificacaoParlamentar, List<Entities.Profissao> lstSenadorProfissao, SenatorLists senatorLists)
    {
        var partido = dbContext.Partidos
            .FirstOrDefault(p => EF.Functions.ILike(p.Sigla, identificacaoParlamentar.SiglaPartidoParlamentar) ||
                                 EF.Functions.ILike(p.Nome, identificacaoParlamentar.SiglaPartidoParlamentar));

        var estado = dbContext.Estados
            .FirstOrDefault(e => EF.Functions.ILike(e.Sigla, identificacaoParlamentar.UfParlamentar));

        senadorToUpdate.Codigo = Convert.ToInt32(identificacaoParlamentar.CodigoPublicoNaLegAtual);
        senadorToUpdate.Nome = identificacaoParlamentar.NomeParlamentar;
        senadorToUpdate.NomeCompleto = identificacaoParlamentar.NomeCompletoParlamentar;
        senadorToUpdate.Sexo = identificacaoParlamentar.SexoParlamentar[0].ToString();
        senadorToUpdate.Profissao = lstSenadorProfissao.Any() ? string.Join(", ", lstSenadorProfissao.Select(obj => obj.NomeProfissao)) : null;
        senadorToUpdate.IdPartido = partido?.Id ?? senadorToUpdate.IdPartido;
        senadorToUpdate.IdEstado ??= estado?.Id;
        senadorToUpdate.Email = identificacaoParlamentar.EmailParlamentar;
        senadorToUpdate.Site = identificacaoParlamentar.UrlPaginaParticular;
        senadorToUpdate.Ativo = senatorLists.ActiveSenators.Contains(Convert.ToInt32(identificacaoParlamentar.CodigoParlamentar)) ? "S" : "N";
    }

    private void ProcessSenatorProfessions(int idSenador, List<Entities.Profissao> lstSenadorProfissao, Dictionary<string, int> lstProfissao)
    {
        foreach (var profissao in lstSenadorProfissao)
        {
            if (!lstProfissao.ContainsKey(profissao.NomeProfissao))
            {
                {
                    var novaProfissao = new OPS.Infraestrutura.Entities.Comum.Profissao
                    {
                        Descricao = profissao.NomeProfissao
                    };
                    dbContext.Profissoes.Add(novaProfissao);

                    lstProfissao.Add(profissao.NomeProfissao, (int)novaProfissao.Id);
                }
            }

            var senadorProfissao = new SenadorProfissao
            {
                IdSenador = Convert.ToInt32(idSenador),
                IdProfissao = (int)lstProfissao[profissao.NomeProfissao]
            };
            dbContext.SenadoresProfissao.Add(senadorProfissao);
        }
    }

    private void ProcessSenatorAcademicHistory(RestClient restClient, string urlApiSenador, int idSenador)
    {
        var request = new RestRequest(urlApiSenador + idSenador.ToString() + "/historicoAcademico?v=1");
        request.AddHeader("Accept", "application/json");

        RestResponse resHistoricoAcademico = restClient.Get(request);
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
                var cursosDoSenador = dbContext.SenadoresHistoricoAcademico.Where(x => x.IdSenador == idSenador).ToList();

                foreach (var curso in lstCursos)
                {
                    if (!cursosDoSenador.Any(x => x.NomeCurso == curso.NomeCurso && x.GrauInstrucao == curso.GrauInstrucao))
                    {
                        dbContext.SenadoresHistoricoAcademico.Add(new SenadorHistoricoAcademico
                        {
                            IdSenador = idSenador,
                            NomeCurso = curso.NomeCurso,
                            GrauInstrucao = curso.GrauInstrucao,
                            Estabelecimento = curso.Estabelecimento,
                            Local = curso.Local
                        });
                    }
                }
            }
        }
    }

    private void ProcessSenatorMandates(RestClient restClient, string urlApiSenador, int idSenador, List<string> lstLegislatura, List<string> lstMotivoAfastamento, SenatorLists senatorLists)
    {
        var request = new RestRequest(urlApiSenador + idSenador.ToString() + "/mandatos?v=5");
        request.AddHeader("Accept", "application/json");

        RestResponse resSenadorMandato = restClient.Get(request);
        var senadorMandato = JsonSerializer.Deserialize<SenadorMandato>(resSenadorMandato.Content);
        var arrMandatosPalamentar = senadorMandato?.MandatoParlamentar?.Parlamentar?.Mandatos?.Mandato;

        if (arrMandatosPalamentar != null)
        {
            logger.LogDebug("Consultando {TotalMandatos} mandato(s)", arrMandatosPalamentar.Count());

            foreach (var mandato in arrMandatosPalamentar)
            {
                if (mandato.CodigoMandato == null) continue;

                try
                {
                    ProcessMandato(mandato, idSenador, senatorLists, lstLegislatura, lstMotivoAfastamento);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }
        }
    }

    private void ProcessMandato(Entities.Mandato mandato, int idSenador, SenatorLists senatorLists, List<string> lstLegislatura, List<string> lstMotivoAfastamento)
    {
        #region Mandato
        var exerceuMandato = mandato.Exercicios?.Exercicio?.Any() ?? false;
        var siglaPartido = mandato.Partidos?.Partido?.FirstOrDefault()?.Sigla;
        short idPartido = 0;
        if (!string.IsNullOrEmpty(siglaPartido))
        {
            siglaPartido = siglaPartido.Trim();
            var partido = dbContext.Partidos
                            .FirstOrDefault(p => EF.Functions.ILike(p.Sigla, siglaPartido) ||
                                                 EF.Functions.ILike(p.Nome, siglaPartido));

            idPartido = partido?.Id ?? 0;
        }

        var estado = dbContext.Estados
                    .FirstOrDefault(e => EF.Functions.ILike(e.Sigla, mandato.UfParlamentar));

        var idMandato = Convert.ToInt32(mandato.CodigoMandato);

        var mandatoDb = dbContext.MandatosSenado.FirstOrDefault(x => x.Id == idMandato && x.IdSenador == idSenador);
        if (mandatoDb != null)
        {
            mandatoDb.IdEstado = estado.Id;
            mandatoDb.IdPartido = idPartido;
            mandatoDb.Participacao = mandato.DescricaoParticipacao;
            mandatoDb.Exerceu = exerceuMandato;

        }
        else
        {
            dbContext.MandatosSenado.Add(new MandatoSenado
            {
                Id = idMandato,
                IdSenador = idSenador,
                IdEstado = estado.Id,
                IdPartido = idPartido,
                Participacao = mandato.DescricaoParticipacao,
                Exerceu = exerceuMandato
            });

        }

        ProcessSuplentes(mandato, senatorLists);

        ProcessLegislaturas(mandato, idSenador, lstLegislatura);

        ProcessExercicios(mandato, idSenador, lstMotivoAfastamento);

        ProcessPartidos(mandato, idSenador);

        #endregion Mandato Exercicio
    }

    private void ProcessPartidos(Entities.Mandato mandato, int idSenador)
    {
        if (mandato.Partidos?.Partido == null) return;

        var senadorPartidos = dbContext.SenadorPartidos.Where(p => p.IdSenador == idSenador).ToList();

        foreach (var partido in mandato.Partidos.Partido)
        {
            short idPartido = 0;
            if (!string.IsNullOrEmpty(partido.Sigla) && partido.Sigla != "S/Partido")
            {
                {
                    partido.Sigla = partido.Sigla.Trim();
                }
                // Find party in database
                var partidoDb = PartidoDeParas.FirstOrDefault(p => p.SiglaNome.Equals(partido.Sigla, StringComparison.CurrentCultureIgnoreCase));

                if (partidoDb == null)
                {
                    logger.LogError("Partido não localizado: {Sigla}", partido.Sigla);
                }

                idPartido = partidoDb?.Id ?? 0;
            }

            // Parse dates
            DateTime? filiacao = null;
            DateTime? desfiliacao = null;

            if (!string.IsNullOrEmpty(partido.DataFiliacao) &&
                DateTime.TryParse(partido.DataFiliacao, out DateTime filiacaoDate))
            {
                filiacao = new DateTime(filiacaoDate.Year, filiacaoDate.Month, filiacaoDate.Day, 0, 0, 0, DateTimeKind.Utc);
            }

            if (!string.IsNullOrEmpty(partido.DataDesfiliacao) &&
                DateTime.TryParse(partido.DataDesfiliacao, out DateTime desfiliacaoDate))
            {
                desfiliacao = new DateTime(desfiliacaoDate.Year, desfiliacaoDate.Month, desfiliacaoDate.Day, 0, 0, 0, DateTimeKind.Utc);
            }

            //dbContext.Database.ExecuteSqlInterpolated($@"
            //        INSERT INTO senado.sf_senador_partido (id_sf_senador, id_partido, filiacao, desfiliacao)
            //        VALUES ({idSenador}, {idPartido}, {filiacao}, {desfiliacao})
            //        ON CONFLICT (id_sf_senador, id_partido, filiacao) DO UPDATE SET
            //            desfiliacao = EXCLUDED.desfiliacao
            //        WHERE EXCLUDED.id_sf_senador = {idSenador} AND EXCLUDED.id_partido = {idPartido} AND EXCLUDED.filiacao = {filiacao}");

            // TODO: FIX Npgsql.PostgresException (0x80004005): 23505: duplicate key value violates unique constraint "id_sf_senador_id_partido_filiacao" DETAIL: Key(id_sf_senador, id_partido, filiacao) = (5982, 1, 2023 - 06 - 21) already exists.
            var senadorPartidoDb = senadorPartidos
                .FirstOrDefault(p => p.IdPartido == idPartido && p.Filiacao == filiacao);

            if (senadorPartidoDb != null)
            {
                senadorPartidoDb.Desfiliacao = desfiliacao;
            }
            else
            {
                dbContext.SenadorPartidos.Add(new SenadorPartido
                {
                    IdSenador = idSenador,
                    IdPartido = idPartido,
                    Filiacao = filiacao,
                    Desfiliacao = desfiliacao
                });
            }
        }
    }

    private void ProcessSuplentes(Entities.Mandato mandato, SenatorLists senatorLists)
    {
        if (mandato.Suplentes?.Suplente == null || mandato.DescricaoParticipacao != "Titular") return;

        foreach (var suplente in mandato.Suplentes.Suplente)
        {
            var idSuplente = Convert.ToInt32(suplente.CodigoParlamentar);

            // Ensure senator exists
            if (!senatorLists.AllSenators.Contains(idSuplente))
            {
                senatorLists.AllSenators.Add(idSuplente);
                senatorLists.NewSenators.Add(idSuplente);
                dbContext.Senadores.Add(new Senador()
                {
                    Id = idSuplente,
                });
            }
        }
    }

    private void ProcessLegislaturas(Entities.Mandato mandato, int idSenador, List<string> lstLegislatura)
    {
        var lstLegislaturaMandato = new List<LegislaturaDoMandato>();
        lstLegislaturaMandato.Add(mandato.PrimeiraLegislaturaDoMandato);
        lstLegislaturaMandato.Add(mandato.SegundaLegislaturaDoMandato);

        foreach (var legislatura in lstLegislaturaMandato)
        {
            if (!lstLegislatura.Contains(legislatura.NumeroLegislatura))
            {
                dbContext.LegislaturasSenado.Add(new LegislaturaSenado
                {
                    Id = Convert.ToInt16(legislatura.NumeroLegislatura),
                    Inicio = Convert.ToDateTime(legislatura.DataInicio),
                    Final = Convert.ToDateTime(legislatura.DataFim)
                });

                lstLegislatura.Add(legislatura.NumeroLegislatura);
            }

            var idMandato = Convert.ToInt32(mandato.CodigoMandato);
            var idLegislatura = Convert.ToInt16(legislatura.NumeroLegislatura);

            var existeMandatoCadastrado = dbContext.MandatoLegislaturasSenado
                .Any(x => x.IdMandato == idMandato && x.IdLegislatura == idLegislatura);

            if (!existeMandatoCadastrado)
            {
                dbContext.MandatoLegislaturasSenado.Add(new MandatoLegislatura
                {
                    IdMandato = idMandato,
                    IdLegislatura = (short)idLegislatura,
                    IdSenador = idSenador
                });
            }
        }
    }

    private void ProcessExercicios(Entities.Mandato mandato, int idSenador, List<string> lstMotivoAfastamento)
    {
        if (mandato.Exercicios?.Exercicio == null) return;

        foreach (var exercicio in mandato.Exercicios.Exercicio)
        {
            if (exercicio.SiglaCausaAfastamento != null)
            {
                exercicio.SiglaCausaAfastamento = exercicio.SiglaCausaAfastamento.Trim();
                if (!lstMotivoAfastamento.Contains(exercicio.SiglaCausaAfastamento))
                {
                    dbContext.MotivoAfastamentos.Add(new MotivoAfastamento
                    {
                        Id = exercicio.SiglaCausaAfastamento,
                        Descricao = exercicio.DescricaoCausaAfastamento
                    });

                    lstMotivoAfastamento.Add(exercicio.SiglaCausaAfastamento);
                }
            }

            var idExercicio = Convert.ToInt32(exercicio.CodigoExercicio);
            var idMandato = Convert.ToInt32(mandato.CodigoMandato);

            var exercicioDb = dbContext.MandatoExerciciosSenado.Where(x => x.Id == idExercicio).FirstOrDefault();
            if (exercicioDb != null)
            {
                exercicioDb.IdMotivoAfastamento = exercicio.SiglaCausaAfastamento;
            }
            else
            {
                dbContext.MandatoExerciciosSenado.Add(new MandatoExercicio
                {
                    Id = idExercicio,
                    IdMandato = idMandato,
                    IdSenador = idSenador,
                    IdMotivoAfastamento = exercicio.SiglaCausaAfastamento,
                    Inicio = Convert.ToDateTime(exercicio.DataInicio),
                    Final = Convert.ToDateTime(exercicio.DataFim)
                });
            }
        }
    }

    public async Task DownloadFotos()
    {
        var sSenadoressImagesPath = System.IO.Path.Combine(appSettings.SiteRootFolder, @"public\img\senador\");

        {
            var activeSenators = dbContext.Senadores
                .Where(s => s.Ativo == "S")
                .Select(s => s.Id.ToString())
                .ToList();

            foreach (var senatorId in activeSenators)
            {
                string url = "https://legis.senado.leg.br/senadores/fotos-oficiais/" + senatorId;
                string src = sSenadoressImagesPath + senatorId + ".jpg";
                if (File.Exists(src))
                {
                    if (!File.Exists(sSenadoressImagesPath + senatorId + "_120x160.jpg"))
                        ImportacaoUtils.CreateImageThumbnail(src);

                    if (!File.Exists(sSenadoressImagesPath + senatorId + "_240x300.jpg"))
                        ImportacaoUtils.CreateImageThumbnail(src, 240, 300);

                    continue;
                }

                try
                {
                    await httpClient.DownloadFile(url, src);

                    ImportacaoUtils.CreateImageThumbnail(src, 120, 160);
                    ImportacaoUtils.CreateImageThumbnail(src, 240, 300);

                    logger.LogDebug("Atualizado imagem do senador {IdSenador}", senatorId);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("404"))
                    {
                        logger.LogInformation("Imagem do senador {IdSenador} inexistente! Motivo: {Motivo}", senatorId, ex.GetBaseException().Message);
                    }
                }
            }
        }
    }

    public async Task Importar(int legislatura)
    {
        var restClient = new RestClient(httpClient);

        var request = new RestRequest($"https://legis.senado.leg.br/dadosabertos/senador/lista/legislatura/{legislatura}.json?v=4");
        request.AddHeader("Accept", "application/json");

        var parlamentares = await restClient.GetAsync<ParlamentarLegislatura>(request);
    }

    public void AtualizarDatasImportacaoParlamentar(DateTime? pInicio = null, DateTime? pFim = null)
    {
        var importacao = dbContext.Importacoes.FirstOrDefault(x => x.Chave == "Senado");
        if (importacao == null)
        {
            importacao = new Importacao()
            {
                Chave = "Senado"
            };
            dbContext.Importacoes.Add(importacao);
        }

        if (pInicio != null)
        {
            importacao.ParlamentarInicio = pInicio.Value;
            importacao.ParlamentarFim = null;
        }
        if (pFim != null) importacao.ParlamentarFim = pFim.Value;

        dbContext.SaveChanges();
    }

}