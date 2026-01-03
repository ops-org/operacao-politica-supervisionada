using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.SenadoFederal.Entities;
using OPS.Importador.Utilities;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.SenadoFederal;
using OPS.Infraestrutura.Factories;
using RestSharp;

namespace OPS.Importador.SenadoFederal;

public class SenatorLists
{
    public List<string> ActiveSenators { get; set; } = new();
    public List<string> AllSenators { get; set; } = new();
    public List<string> NewSenators { get; set; } = new();
}

public class ImportadorParlamentarSenado : IImportadorParlamentar
{
    protected readonly ILogger<ImportadorParlamentarSenado> logger;
    protected readonly IDbConnection connection;
    protected readonly AppDbContextFactory dbContextFactory;
    public string rootPath { get; init; }
    public string tempPath { get; init; }

    public HttpClient httpClient { get; }

    public ImportadorParlamentarSenado(IServiceProvider serviceProvider)
    {
        logger = serviceProvider.GetService<ILogger<ImportadorParlamentarSenado>>();
        connection = serviceProvider.GetService<IDbConnection>();
        dbContextFactory = serviceProvider.GetService<AppDbContextFactory>();

        var configuration = serviceProvider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
        rootPath = configuration["AppSettings:SiteRootFolder"];
        tempPath = configuration["AppSettings:SiteTempFolder"];

        httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient("ResilientClient");
    }

    public Task Importar()
    {
        using (var dbContext = dbContextFactory.CreateDbContext())
        //using (var transaction = dbContext.Database.BeginTransaction())
        {
            try
            {
                var senatorLists = GetSenatorLists(dbContext);

                // Mark all senators as inactive
                dbContext.Database.ExecuteSqlRaw("UPDATE sf_senador SET ativo = 'N' WHERE ativo = 'S'");

                var restClient = new RestClient(httpClient);
                ImportActiveSenators(dbContext, restClient, senatorLists); // Can Change senatorLists

                UpdateSenatorDetails(dbContext, restClient, senatorLists);

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
            ActiveSenators = new List<string>(), // Prenchido no ImportActiveSenators
            AllSenators = dbContext.Senadores
                .OrderBy(s => s.Id)
                .Select(s => s.Id.ToString())
                .ToList(),
            NewSenators = dbContext.Senadores // Itens para forçar atualização, Incrementado depois no ImportActiveSenators com novos Suplentes
                .Where(s => s.Ativo == "S" || s.Nome == null)
                .OrderBy(s => s.Id)
                .Select(s => s.Id.ToString())
                .ToList()
        };
    }

    private void ImportActiveSenators(AppDbContext dbContext, RestClient restClient, SenatorLists senatorLists)
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

                var codigoParlamentar = Convert.ToUInt32(parlamentar.CodigoParlamentar);
                var existe = dbContext.Senadores.Find(codigoParlamentar);
                if (existe == null)
                {
                    logger.LogInformation("Novo Senador {IdSenador}: {NomeParlamentar}", parlamentar.CodigoParlamentar, parlamentar.NomeParlamentar);

                    var partido = dbContext.Partidos
                        .FirstOrDefault(p => EF.Functions.Like(p.Sigla, parlamentar.SiglaPartidoParlamentar) ||
                                             EF.Functions.Like(p.Nome, parlamentar.SiglaPartidoParlamentar));

                    var estado = dbContext.Estados
                        .FirstOrDefault(e => EF.Functions.Like(e.Sigla, parlamentar.UfParlamentar));

                    var novoSenador = new Senador
                    {
                        Id = Convert.ToUInt32(parlamentar.CodigoParlamentar),
                        Codigo = Convert.ToUInt32(parlamentar.CodigoPublicoNaLegAtual),
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
                    dbContext.SaveChanges();

                    senatorLists.AllSenators.Add(parlamentar.CodigoParlamentar);
                }
                else
                {
                    senatorLists.ActiveSenators.Add(parlamentar.CodigoParlamentar);

                    if (!senatorLists.AllSenators.Contains(parlamentar.CodigoParlamentar))
                    {
                        senatorLists.AllSenators.Add(parlamentar.CodigoParlamentar);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }
        }

        #endregion Importar senadores ativos
    }

    private void UpdateSenatorDetails(AppDbContext dbContext, RestClient restClient, SenatorLists senatorLists)
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
        UpdateActiveSenatorDetails(dbContext, restClient, senatorLists, urlApiSenador, lstLegislatura, lstMotivoAfastamento, lstProfissao);

        // Atualizar novo senadores (o metodo acima pode incluir novos senadores, ex: suplentes)
        UpdateActiveSenatorDetails(dbContext, restClient, senatorLists, urlApiSenador, lstLegislatura, lstMotivoAfastamento, lstProfissao, onlyNew: true);

        #endregion Atualizar senadores ativos e os que estavam ativos antes da importação
    }

    private void UpdateActiveSenatorDetails(AppDbContext dbContext, RestClient restClient, SenatorLists senatorLists, string urlApiSenador, List<string> lstLegislatura, List<string> lstMotivoAfastamento, Dictionary<string, int> lstProfissao, bool onlyNew = false)
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
            var senadorToUpdate = dbContext.Senadores.Find(Convert.ToUInt32(identificacaoParlamentar.CodigoParlamentar));
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

            ProcessSenatorBasicData(dbContext, senadorToUpdate, identificacaoParlamentar, lstSenadorProfissao, senatorLists);

            if (lstSenadorProfissao.Any())
            {
                ProcessSenatorProfessions(dbContext, idSenador, lstSenadorProfissao, lstProfissao);
            }

            ProcessSenatorAcademicHistory(dbContext, restClient, urlApiSenador, idSenador);

            ProcessSenatorMandates(dbContext, restClient, urlApiSenador, idSenador, lstLegislatura, lstMotivoAfastamento, senatorLists);
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

    private List<Entities.Profissao> GetSenatorProfessions(RestClient restClient, string urlApiSenador, string idSenador, SenadorDetalhes senador)
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

    private void ProcessSenatorBasicData(AppDbContext dbContext, Senador senadorToUpdate, IdentificacaoParlamentar identificacaoParlamentar, List<Entities.Profissao> lstSenadorProfissao, SenatorLists senatorLists)
    {
        var partido = dbContext.Partidos
            .FirstOrDefault(p => EF.Functions.Like(p.Sigla, identificacaoParlamentar.SiglaPartidoParlamentar) ||
                                 EF.Functions.Like(p.Nome, identificacaoParlamentar.SiglaPartidoParlamentar));

        var estado = dbContext.Estados
            .FirstOrDefault(e => EF.Functions.Like(e.Sigla, identificacaoParlamentar.UfParlamentar));

        senadorToUpdate.Codigo = Convert.ToUInt32(identificacaoParlamentar.CodigoPublicoNaLegAtual);
        senadorToUpdate.Nome = identificacaoParlamentar.NomeParlamentar;
        senadorToUpdate.NomeCompleto = identificacaoParlamentar.NomeCompletoParlamentar;
        senadorToUpdate.Sexo = identificacaoParlamentar.SexoParlamentar[0].ToString();
        senadorToUpdate.Profissao = lstSenadorProfissao.Any() ? string.Join(", ", lstSenadorProfissao.Select(obj => obj.NomeProfissao)) : null;
        senadorToUpdate.IdPartido = partido?.Id ?? senadorToUpdate.IdPartido;
        senadorToUpdate.IdEstado ??= estado?.Id;
        senadorToUpdate.Email = identificacaoParlamentar.EmailParlamentar;
        senadorToUpdate.Site = identificacaoParlamentar.UrlPaginaParticular;
        senadorToUpdate.Ativo = senatorLists.ActiveSenators.Contains(identificacaoParlamentar.CodigoParlamentar) ? "S" : "N";

        dbContext.SaveChanges();
    }

    private void ProcessSenatorProfessions(AppDbContext dbContext, string idSenador, List<Entities.Profissao> lstSenadorProfissao, Dictionary<string, int> lstProfissao)
    {
        foreach (var profissao in lstSenadorProfissao)
        {
            if (!lstProfissao.ContainsKey(profissao.NomeProfissao))
            {
                using (var profissaoDbContext = dbContextFactory.CreateDbContext())
                {
                    var novaProfissao = new OPS.Infraestrutura.Entities.Comum.Profissao
                    {
                        Descricao = profissao.NomeProfissao
                    };
                    profissaoDbContext.Profissoes.Add(novaProfissao);
                    profissaoDbContext.SaveChanges();

                    lstProfissao.Add(profissao.NomeProfissao, (int)novaProfissao.Id);
                }
            }

            // Add senator-profession relationship using EF Core
            using (var senadorProfissaoDbContext = dbContextFactory.CreateDbContext())
            {
                var senadorProfissao = new OPS.Infraestrutura.Entities.SenadoFederal.SenadorProfissao
                {
                    IdSenador = Convert.ToUInt32(idSenador),
                    IdProfissao = (uint)lstProfissao[profissao.NomeProfissao]
                };
                senadorProfissaoDbContext.SenadoresProfissao.Add(senadorProfissao);
                senadorProfissaoDbContext.SaveChanges();
            }
        }
    }

    private void ProcessSenatorAcademicHistory(AppDbContext dbContext, RestClient restClient, string urlApiSenador, string idSenador)
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
                foreach (var curso in lstCursos)
                {
                    dbContext.Database.ExecuteSqlRaw(@"
                        INSERT IGNORE INTO sf_senador_historico_academico (id_sf_senador, nome_curso, grau_instrucao, estabelecimento, local) 
                        values ({0}, {1}, {2}, {3}, {4})",
                        idSenador, curso.NomeCurso, curso.GrauInstrucao, curso.Estabelecimento, curso.Local);
                }
            }
        }
    }

    private void ProcessSenatorMandates(AppDbContext dbContext, RestClient restClient, string urlApiSenador, string idSenador, List<string> lstLegislatura, List<string> lstMotivoAfastamento, SenatorLists senatorLists)
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
                    ProcessMandato(dbContext, mandato, idSenador, senatorLists, lstLegislatura, lstMotivoAfastamento);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, ex.Message);
                }
            }
        }
    }

    private void ProcessMandato(AppDbContext dbContext, Entities.Mandato mandato, string idSenador, SenatorLists senatorLists, List<string> lstLegislatura, List<string> lstMotivoAfastamento)
    {
        #region Mandato
        var exerceuMandato = mandato.Exercicios?.Exercicio?.Any() ?? false;
        var siglaPartido = mandato.Partidos?.Partido?.FirstOrDefault()?.Sigla;
        int idPartido = 0;
        if (!string.IsNullOrEmpty(siglaPartido))
        {
            siglaPartido = siglaPartido.Trim();
            var partido = dbContext.Partidos
                            .FirstOrDefault(p => EF.Functions.Like(p.Sigla, siglaPartido) ||
                                                 EF.Functions.Like(p.Nome, siglaPartido));

            idPartido = partido?.Id ?? 0;
        }

        var estado = dbContext.Estados
                    .FirstOrDefault(e => EF.Functions.Like(e.Sigla, mandato.UfParlamentar));

        dbContext.Database.ExecuteSqlRaw(@"
            INSERT INTO sf_mandato (
                id, id_sf_senador, id_estado, id_partido, participacao, exerceu
            ) VALUES (
                {0}, {1}, {2}, {3}, {4}, {5}
            )
            ON DUPLICATE KEY UPDATE
                id_estado = {2},
                id_partido = {3},
                participacao = {4},
                exerceu = {5}",
            mandato.CodigoMandato, idSenador, estado.Id, idPartido, mandato.DescricaoParticipacao, exerceuMandato);

        ProcessSuplentes(dbContext, mandato, idSenador, senatorLists);

        ProcessLegislaturas(dbContext, mandato, lstLegislatura);

        ProcessExercicios(dbContext, mandato, idSenador, lstMotivoAfastamento);

        ProcessPartidos(dbContext, mandato, idSenador);

        #endregion Mandato Exercicio
    }

    private void ProcessPartidos(AppDbContext dbContext, Entities.Mandato mandato, string idSenador)
    {
        if (mandato.Partidos?.Partido == null) return;

        foreach (var partido in mandato.Partidos.Partido)
        {
            int idPartido = 0;
            if (!string.IsNullOrEmpty(partido.Sigla))
            {
                {
                    partido.Sigla = partido.Sigla.Trim();
                }
                // Find party in database
                var partidoDb = dbContext.Partidos
                    .FirstOrDefault(p => EF.Functions.Like(p.Sigla, partido.Sigla));

                if (partidoDb == null)
                {
                    // TODO: Remover se não gerar logs
                    partidoDb = dbContext.Partidos
                       .FirstOrDefault(p => EF.Functions.Like(p.Nome, partido.Sigla));

                    if (partidoDb != null)
                    {
                        logger.LogError("Partido localizado pelo nome: {Sigla}", partido.Sigla);
                    }
                }

                idPartido = partidoDb?.Id ?? 0;
            }

            // Parse dates
            DateTime? filiacao = null;
            DateTime? desfiliacao = null;

            if (!string.IsNullOrEmpty(partido.DataFiliacao) &&
                DateTime.TryParse(partido.DataFiliacao, out DateTime filiacaoDate))
            {
                filiacao = filiacaoDate;
            }

            if (!string.IsNullOrEmpty(partido.DataDesfiliacao) &&
                DateTime.TryParse(partido.DataDesfiliacao, out DateTime desfiliacaoDate))
            {
                desfiliacao = desfiliacaoDate;
            }

            dbContext.Database.ExecuteSqlRaw(@"
                INSERT INTO sf_senador_partido (
                    id_sf_senador, id_partido, filiacao, desfiliacao
                ) VALUES (
                    {0}, {1}, {2}, {3}
                ) ON DUPLICATE KEY UPDATE
                    desfiliacao = {3}",
                idSenador, idPartido, filiacao, desfiliacao);
        }
    }

    private void ProcessSuplentes(AppDbContext dbContext, Entities.Mandato mandato, string idSenador, SenatorLists senatorLists)
    {
        if (mandato.Suplentes?.Suplente == null || mandato.DescricaoParticipacao != "Titular") return;

        foreach (var suplente in mandato.Suplentes.Suplente)
        {
            var idSuplente = suplente.CodigoParlamentar;

            // Ensure senator exists
            if (!senatorLists.AllSenators.Contains(idSuplente))
            {
                senatorLists.AllSenators.Add(idSuplente);
                senatorLists.NewSenators.Add(idSuplente);
                dbContext.Database.ExecuteSqlRaw("INSERT INTO sf_senador (id) VALUES ({0})", idSuplente);
            }
        }
    }

    private void ProcessLegislaturas(AppDbContext dbContext, Entities.Mandato mandato, List<string> lstLegislatura)
    {
        var lstLegislaturaMandato = new List<LegislaturaDoMandato>();
        lstLegislaturaMandato.Add(mandato.PrimeiraLegislaturaDoMandato);
        lstLegislaturaMandato.Add(mandato.SegundaLegislaturaDoMandato);

        foreach (var legislatura in lstLegislaturaMandato)
        {
            if (!lstLegislatura.Contains(legislatura.NumeroLegislatura))
            {
                dbContext.Database.ExecuteSqlRaw(@"
                    INSERT INTO sf_legislatura (
                        id, inicio, final
                    ) VALUES (
                        {0}, {1}, {2}
                    )", legislatura.NumeroLegislatura, legislatura.DataInicio, legislatura.DataFim);

                lstLegislatura.Add(legislatura.NumeroLegislatura);
            }

            dbContext.Database.ExecuteSqlRaw(@"
                INSERT IGNORE INTO sf_mandato_legislatura (
                    id_sf_mandato, id_sf_legislatura
                ) VALUES (
                    {0}, {1}
                )", mandato.CodigoMandato, legislatura.NumeroLegislatura);
        }
    }

    private void ProcessExercicios(AppDbContext dbContext, Entities.Mandato mandato, string idSenador, List<string> lstMotivoAfastamento)
    {
        if (mandato.Exercicios?.Exercicio == null) return;

        foreach (var exercicio in mandato.Exercicios.Exercicio)
        {
            if (exercicio.SiglaCausaAfastamento != null)
            {
                exercicio.SiglaCausaAfastamento = exercicio.SiglaCausaAfastamento.Trim();
                if (!lstMotivoAfastamento.Contains(exercicio.SiglaCausaAfastamento))
                {
                    dbContext.Database.ExecuteSqlRaw(@"
                        INSERT INTO sf_motivo_afastamento (
                            id, descricao
                        ) VALUES (
                            {0}, {1}
                        )", exercicio.SiglaCausaAfastamento, exercicio.DescricaoCausaAfastamento);

                    lstMotivoAfastamento.Add(exercicio.SiglaCausaAfastamento);
                }
            }

            dbContext.Database.ExecuteSqlRaw(@"
                INSERT INTO sf_mandato_exercicio (
                    id, id_sf_mandato, id_sf_senador, id_sf_motivo_afastamento, inicio, final
                ) VALUES (
                    {0}, {1}, {2}, {3}, {4}, {5}
                ) ON DUPLICATE KEY UPDATE
                    id_sf_motivo_afastamento = {3},
                    final = {5}", exercicio.CodigoExercicio, mandato.CodigoMandato, idSenador, exercicio.SiglaCausaAfastamento, exercicio.DataInicio, exercicio.DataFim);
        }
    }

    public async Task DownloadFotos()
    {
        var sSenadoressImagesPath = System.IO.Path.Combine(rootPath, @"public\img\senador\");

        using (var dbContext = dbContextFactory.CreateDbContext())
        {
            var activeSenators = dbContext.Senadores
                .Where(s => s.Ativo == "S")
                .Select(s => s.Id.ToString())
                .ToList();

            foreach (var senatorId in activeSenators)
            {
                string url = "https://www.senado.leg.br/senadores/img/fotos-oficiais/senador" + senatorId + ".jpg";
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

        var parlamentares = restClient.Get<ParlamentarLegislatura>(request);
    }

    public void AtualizarDatasImportacaoParlamentar(DateTime? pInicio = null, DateTime? pFim = null)
    {
        var importacao = connection.GetList<Importacao>(new { chave = "Senado" }).FirstOrDefault();
        if (importacao == null)
        {
            importacao = new Importacao()
            {
                Chave = "Senado"
            };
            importacao.Id = (ushort)connection.Insert(importacao);
        }

        if (pInicio != null)
        {
            importacao.ParlamentarInicio = pInicio.Value;
            importacao.ParlamentarFim = null;
        }
        if (pFim != null) importacao.ParlamentarFim = pFim.Value;

        connection.Update(importacao);
    }

}