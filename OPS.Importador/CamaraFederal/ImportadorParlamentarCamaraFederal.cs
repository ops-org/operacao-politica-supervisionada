using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.CamaraFederal.Entities;
using OPS.Importador.Utilities;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Factories;
using RestSharp;

namespace OPS.Importador.CamaraFederal;

public class ImportadorParlamentarCamaraFederal : IImportadorParlamentar
{
    protected readonly ILogger<ImportadorParlamentarCamaraFederal> logger;
    protected readonly IDbConnection connection;
    protected readonly AppDbContextFactory dbContextFactory;

    public string rootPath { get; set; }
    public string tempPath { get; set; }

    private const short legislaturaAtual = 57;

    public HttpClient httpClient { get; }

    public ImportadorParlamentarCamaraFederal(IServiceProvider serviceProvider)
    {
        logger = serviceProvider.GetService<ILogger<ImportadorParlamentarCamaraFederal>>();
        connection = serviceProvider.GetService<IDbConnection>();
        dbContextFactory = serviceProvider.GetService<AppDbContextFactory>();

        var configuration = serviceProvider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
        rootPath = configuration["AppSettings:SiteRootFolder"];
        tempPath = configuration["AppSettings:SiteTempFolder"];

        httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient("DefaultClient");
    }

    public async Task Importar()
    {
        await ImportarDeputados(legislaturaAtual - 1);
    }

    public async Task ImportarDeputados(int legislaturaInicial)
    {
        int novos = 0;
        int pagina;
        var restClient = new RestClient(httpClient, new RestClientOptions()
        {
            BaseUrl = new Uri("https://dadosabertos.camara.leg.br/api/v2/deputados")
        });

        List<Dado> deputados;
        List<Link> links;

        using (var context = dbContextFactory.CreateDbContext())
        {
            await context.Database.ExecuteSqlRawAsync(@"UPDATE cf_deputado SET id_cf_gabinete=NULL");

            for (int leg = legislaturaInicial; leg <= legislaturaAtual; leg++)
            {
                pagina = 1;
                do
                {
                    var uri = string.Format("?idLegislatura={0}&pagina={1}&itens=1000", leg, pagina++);
                    var request = new RestRequest(uri);
                    request.AddHeader("Accept", "application/json");

                    try
                    {
                        var y = restClient.Execute<Deputados>(request);
                        deputados = y.Data.dados;
                        links = y.Data.links;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(30));

                        var y = restClient.Execute<Deputados>(request);
                        deputados = y.Data.dados;
                        links = y.Data.links;
                    }

                    foreach (var deputado in deputados)
                    {
                        Dados deputadoDetalhes = null;
                        string situacao = null;

                        var count = context.Database.GetDbConnection().ExecuteScalar<int>(@"SELECT COUNT(1) FROM cf_deputado WHERE id=@id", 
                            new NpgsqlParameter("id", deputado.id));

                        request = new RestRequest(deputado.id.ToString());
                        request.AddHeader("Accept", "application/json");
                        try
                        {
                            try
                            {
                                var response = restClient.Execute<DeputadoDetalhes>(request);
                                deputadoDetalhes = response.Data.dados;

                                if (deputadoDetalhes == null)
                                {
                                    logger.LogError("Erro ao consultar parlamentar: {@Response}", response);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogInformation("Erro ao consultar parlamentar: {Id} {ErrorMessage}. Nova tentativa em 30s.", deputado.id, ex.Message);
                                Thread.Sleep(TimeSpan.FromSeconds(30));

                                var x = restClient.Execute<DeputadoDetalhes>(request);
                                deputadoDetalhes = x.Data.dados;

                                if (deputadoDetalhes == null)
                                {
                                    logger.LogError("Erro ao consultar parlamentar {IdDeputado}: {ErrorMessage}", deputado.id, ex.Message);
                                    continue;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError("Erro ao consultar parlamentar {IdDeputado}: {ErrorMessage}", deputado.id, ex.Message);
                            continue;
                        }


                        logger.LogDebug("Sincronizando dados do parlamentar: {IdDeputado} - {NomeDeputado} ({SiglaPartido})",
                            deputado.id, (deputadoDetalhes.ultimoStatus.nomeEleitoral ?? deputadoDetalhes.nomeCivil).ToTitleCase(), deputadoDetalhes.ultimoStatus.siglaPartido);

                        // TODO: salvar para exibir
                        //if (deputadoDetalhes.redeSocial.Any())
                        //{
                        //    Console.WriteLine(string.Join(",", deputadoDetalhes.redeSocial));
                        //}

                        int? id_cf_gabinete = null;
                        if (deputadoDetalhes.ultimoStatus.gabinete.sala != null)
                        {
                            var id = context.Database.GetDbConnection().ExecuteScalar<int>(@"SELECT id from cf_gabinete where sala = @sala",
                                new NpgsqlParameter("sala", deputadoDetalhes.ultimoStatus.gabinete.sala.PadLeft(3, '0')));

                            var gabinete = deputadoDetalhes.ultimoStatus.gabinete;

                            if (!string.IsNullOrEmpty(gabinete.sala))
                                if (id == null || Convert.IsDBNull(id))
                                {
                                    await context.Database.ExecuteSqlRawAsync(@"INSERT INTO cf_gabinete (
                                            id,
                                            nome,
                                            predio,
                                            andar,
                                            sala,
                                            telefone
                                        ) VALUES (
                                            @id,
                                            @nome,
                                            @predio,
                                            @andar,
                                            @sala,
                                            @telefone
                                        )",
                                        new NpgsqlParameter("id", gabinete.sala),
                                        new NpgsqlParameter("nome", gabinete.nome),
                                        new NpgsqlParameter("predio", gabinete.predio),
                                        new NpgsqlParameter("andar", gabinete.andar),
                                        new NpgsqlParameter("sala", gabinete.sala),
                                        new NpgsqlParameter("telefone", gabinete.telefone));

                                    id_cf_gabinete = Convert.ToInt32(gabinete.sala);
                                }
                                else
                                {
                                    id_cf_gabinete = Convert.ToInt32(id);

                                    if (leg == legislaturaAtual)
                                    {
                                        await context.Database.ExecuteSqlRawAsync(@"UPDATE cf_gabinete SET
                                                nome = @nome,
                                                predio = @predio,
                                                andar = @andar,
                                                telefone = @telefone
                                                WHERE sala = @sala
                                            ",
                                            new NpgsqlParameter("nome", gabinete.nome),
                                            new NpgsqlParameter("predio", gabinete.predio),
                                            new NpgsqlParameter("andar", gabinete.andar),
                                            new NpgsqlParameter("telefone", gabinete.telefone),
                                            new NpgsqlParameter("sala", gabinete.sala));
                                    }
                                }
                        }

                        if (leg == legislaturaAtual)
                        {
                            situacao = deputadoDetalhes.ultimoStatus.situacao;
                        }
                        else
                        {
                            situacao = "Fim de Mandato";
                        }


                        var siglaPartido = deputadoDetalhes.ultimoStatus.siglaPartido;

                        if (Convert.ToInt32(count) == 0)
                        {
                            await context.Database.ExecuteSqlRawAsync(@"INSERT INTO cf_deputado (
                                    id,
                                    id_partido, 
                                    id_estado,
                                    id_cf_gabinete,
                                    cpf, 
                                    nome_civil, 
                                    nome_parlamentar, 
                                    sexo, 
                                    condicao, 
                                    situacao, 
                                    email, 
                                    nascimento, 
                                    falecimento, 
                                    id_estado_nascimento,
                                    municipio, 
                                    website,
                                    escolaridade
                                ) VALUES (
                                    @id,
                                    (SELECT id FROM partido where sigla like @sigla_partido OR nome like @sigla_partido), 
                                    (SELECT id FROM estado where sigla like @sigla_estado), 
                                    @id_cf_gabinete,
                                    @cpf, 
                                    @nome_civil, 
                                    @nome_parlamentar, 
                                    @sexo, 
                                    @condicao, 
                                    @situacao, 
                                    @email, 
                                    @nascimento, 
                                    @falecimento, 
                                    (SELECT id FROM estado where sigla like @sigla_estado_nascimento), 
                                    @municipio, 
                                    @website,
                                    @escolaridade
                                )",
                                new NpgsqlParameter("id", deputado.id),
                                new NpgsqlParameter("sigla_partido", siglaPartido),
                                new NpgsqlParameter("sigla_estado", deputadoDetalhes.ultimoStatus.siglaUf),
                                new NpgsqlParameter("id_cf_gabinete", id_cf_gabinete),
                                new NpgsqlParameter("cpf", deputadoDetalhes.cpf),
                                new NpgsqlParameter("nome_civil", deputadoDetalhes.nomeCivil.ToTitleCase()),
                                new NpgsqlParameter("nome_parlamentar", (deputadoDetalhes.ultimoStatus.nomeEleitoral ?? deputadoDetalhes.nomeCivil).ToTitleCase()),
                                new NpgsqlParameter("sexo", deputadoDetalhes.sexo),
                                new NpgsqlParameter("condicao", deputadoDetalhes.ultimoStatus.condicaoEleitoral),
                                new NpgsqlParameter("situacao", situacao),
                                new NpgsqlParameter("email", deputadoDetalhes.ultimoStatus.gabinete.email?.ToLower()),
                                new NpgsqlParameter("nascimento", deputadoDetalhes.dataNascimento),
                                new NpgsqlParameter("falecimento", deputadoDetalhes.dataFalecimento),
                                new NpgsqlParameter("sigla_estado_nascimento", deputadoDetalhes.ufNascimento),
                                new NpgsqlParameter("municipio", deputadoDetalhes.municipioNascimento),
                                new NpgsqlParameter("website", deputadoDetalhes.urlWebsite),
                                new NpgsqlParameter("escolaridade", deputadoDetalhes.escolaridade));

                            novos++;
                        }
                        else
                        {
                            await context.Database.ExecuteSqlRawAsync(@"
                                    UPDATE cf_deputado 
                                    SET 
                                        id_partido = (SELECT id FROM partido where sigla like @sigla_partido OR nome like @sigla_partido), 
                                        id_estado = (SELECT id FROM estado where sigla like @sigla_estado),
                                        id_cf_gabinete = @id_cf_gabinete,
                                        cpf = @cpf, 
                                        nome_civil = @nome_civil, 
                                        nome_parlamentar = @nome_parlamentar, 
                                        sexo = @sexo, 
                                        condicao = @condicao, 
                                        situacao = @situacao, 
                                        email = @email, 
                                        nascimento = @nascimento, 
                                        falecimento = @falecimento, 
                                        id_estado_nascimento = (SELECT id FROM estado where sigla like @sigla_estado_nascimento),
                                        municipio = @municipio, 
                                        website = @website,
                                        escolaridade = @escolaridade
                                    WHERE id = @id
                            ",
                            new NpgsqlParameter("sigla_partido", siglaPartido),
                            new NpgsqlParameter("sigla_estado", deputadoDetalhes.ultimoStatus.siglaUf),
                            new NpgsqlParameter("id_cf_gabinete", id_cf_gabinete),
                            new NpgsqlParameter("cpf", deputadoDetalhes.cpf),
                            new NpgsqlParameter("nome_civil", deputadoDetalhes.nomeCivil.ToTitleCase()),
                            new NpgsqlParameter("nome_parlamentar", (deputadoDetalhes.ultimoStatus.nomeEleitoral ?? deputadoDetalhes.nomeCivil).ToTitleCase()),
                            new NpgsqlParameter("sexo", deputadoDetalhes.sexo),
                            new NpgsqlParameter("condicao", deputadoDetalhes.ultimoStatus.condicaoEleitoral),
                            new NpgsqlParameter("situacao", situacao),
                            new NpgsqlParameter("email", deputadoDetalhes.ultimoStatus.gabinete.email?.ToLower()),
                            new NpgsqlParameter("nascimento", deputadoDetalhes.dataNascimento.NullIfEmpty()),
                            new NpgsqlParameter("falecimento", deputadoDetalhes.dataFalecimento.NullIfEmpty()),
                            new NpgsqlParameter("sigla_estado_nascimento", deputadoDetalhes.ufNascimento),
                            new NpgsqlParameter("municipio", deputadoDetalhes.municipioNascimento),
                            new NpgsqlParameter("website", deputadoDetalhes.urlWebsite),
                            new NpgsqlParameter("escolaridade", deputadoDetalhes.escolaridade),
                            new NpgsqlParameter("id", deputado.id));
                        }

                        if (deputadoDetalhes.ultimoStatus.idLegislatura == leg)
                        {
                            var countMandato = context.Database.GetDbConnection().ExecuteScalar<int>(
                                    "SELECT 1 FROM cf_mandato WHERE id_cf_deputado = @id_cf_deputado AND id_legislatura = @id_legislatura",
                                    new { id_cf_deputado = deputado.id, id_legislatura = leg });

                            if (countMandato == 0)
                            {
                                await context.Database.ExecuteSqlRawAsync(@"INSERT INTO cf_mandato (
                                        id_cf_deputado,
                                        id_legislatura,
                                        id_estado,
                                        id_partido,
                                        condicao
                                    ) VALUES (
                                        @id_cf_deputado,
                                        @id_legislatura,
                                        (SELECT id FROM estado where sigla like @sigla_estado), 
                                        (SELECT id FROM partido where sigla like @sigla_partido OR nome like @sigla_partido), 
                                        @condicao
                                    )",
                                    new NpgsqlParameter("id_cf_deputado", deputado.id),
                                    new NpgsqlParameter("id_legislatura", leg),
                                    new NpgsqlParameter("sigla_estado", deputadoDetalhes.ultimoStatus.siglaUf),
                                    new NpgsqlParameter("sigla_partido", siglaPartido),
                                    new NpgsqlParameter("condicao", deputadoDetalhes.ultimoStatus.condicaoEleitoral));
                            }
                        }
                    }
                } while (links.Any(x => x.rel == "next"));

            }
        }

        if (novos > 0)
        {
            logger.LogInformation("Novos Deputados: " + novos.ToString());
        }
    }

    /// <summary>
    /// Baixa as imagens dos deputados novos (imagens que ainda não foram baixadas)
    /// </summary>
    /// <param name="dirRaiz"></param>
    public async Task DownloadFotos()
    {
        var sDeputadosImagesPath = System.IO.Path.Combine(rootPath, @"public\img\depfederal\");

        var httpClient = new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 20
        });

        {
            string sql = "SELECT id, nome_parlamentar FROM cf_deputado where id > 100 order by id -- AND situacao = 'Exercício'";
            var deputados = connection.Query<(int id, string nome_parlamentar)>(sql);

            foreach (var deputado in deputados)
            {
                string id = deputado.id.ToString();
                string src = sDeputadosImagesPath + id + ".jpg";
                if (File.Exists(src))
                {
                    if (!File.Exists(sDeputadosImagesPath + id + "_120x160.jpg"))
                        ImportacaoUtils.CreateImageThumbnail(src);

                    if (!File.Exists(sDeputadosImagesPath + id + "_240x300.jpg"))
                        ImportacaoUtils.CreateImageThumbnail(src, 240, 300);

                    continue;
                }

                try
                {
                    try
                    {
                        await httpClient.DownloadFile("https://www.camara.leg.br/internet/deputado/bandep/" + id + ".jpgmaior.jpg", src);
                    }
                    catch (Exception)
                    {
                        await httpClient.DownloadFile("http://www.camara.gov.br/internet/deputado/bandep/" + id + ".jpg", src);
                    }

                    ImportacaoUtils.CreateImageThumbnail(src);
                    ImportacaoUtils.CreateImageThumbnail(src, 240, 300);


                    logger.LogDebug("Atualizado imagem do parlamentar {Parlamentar}.", deputado.nome_parlamentar);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("404"))
                    {
                        logger.LogInformation(ex.GetBaseException(), "Imagem do parlamentar {Parlamentar} inexistente.", deputado.nome_parlamentar);
                    }
                }
            }
        }
    }

    public void AtualizarDatasImportacaoParlamentar(DateTime? pInicio = null, DateTime? pFim = null)
    {
        var importacao = connection.GetList<Importacao>(new { chave = "Camara Federal" }).FirstOrDefault();
        if (importacao == null)
        {
            importacao = new Importacao()
            {
                Chave = "Camara Federal"
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
