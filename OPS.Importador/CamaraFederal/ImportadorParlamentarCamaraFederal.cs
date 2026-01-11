using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.CamaraFederal.Entities;
using OPS.Importador.Utilities;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.CamaraFederal;
using RestSharp;

namespace OPS.Importador.CamaraFederal;

public class ImportadorParlamentarCamaraFederal : IImportadorParlamentar
{
    protected readonly ILogger<ImportadorParlamentarCamaraFederal> logger;
    protected readonly IDbConnection connection;
    protected readonly AppDbContext dbContext;

    public string rootPath { get; set; }
    public string tempPath { get; set; }

    private const short legislaturaAtual = 57;

    public HttpClient httpClient { get; }

    public ImportadorParlamentarCamaraFederal(IServiceProvider serviceProvider)
    {
        logger = serviceProvider.GetService<ILogger<ImportadorParlamentarCamaraFederal>>();
        connection = serviceProvider.GetService<IDbConnection>();
        dbContext = serviceProvider.GetService<AppDbContext>();

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

        {
            await dbContext.Database.ExecuteSqlRawAsync(@"UPDATE camara.cf_deputado SET id_cf_gabinete=NULL");

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

                        var deputadoDb = dbContext.DeputadosFederais.FirstOrDefault(x => x.Id == deputado.id);

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
                            var sala = deputadoDetalhes.ultimoStatus.gabinete.sala;
                            var gabineteDb = dbContext.GabinetesCamaraFederal.FirstOrDefault(x => x.Id == Convert.ToInt32(sala));
                            var gabinete = deputadoDetalhes.ultimoStatus.gabinete;

                            if (!string.IsNullOrEmpty(gabinete.sala))
                                if (gabineteDb == null)
                                {
                                    var gabineteNew = new Infraestrutura.Entities.CamaraFederal.Gabinete()
                                    {
                                        Id = Convert.ToInt16(gabinete.sala),
                                        Nome = gabinete.nome,
                                        Predio = gabinete.predio,
                                        Andar = (byte)Convert.ToInt16(gabinete.andar),
                                        Sala = gabinete.sala,
                                        Telefone = gabinete.telefone,
                                    };

                                    dbContext.GabinetesCamaraFederal.Add(gabineteNew);
                                    dbContext.SaveChanges();

                                    id_cf_gabinete = Convert.ToInt32(gabinete.sala);
                                }
                                else if (leg == legislaturaAtual)
                                {

                                    gabineteDb.Nome = gabinete.nome;
                                    gabineteDb.Predio = gabinete.predio;
                                    gabineteDb.Andar = (byte)Convert.ToInt16(gabinete.andar);
                                    gabineteDb.Telefone = gabinete.telefone;
                                    gabineteDb.Sala = gabinete.sala;

                                    dbContext.SaveChanges();
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

                        var idPartido = dbContext.Partidos.FirstOrDefault(x => EF.Functions.ILike(x.Sigla, siglaPartido) || EF.Functions.ILike(x.Nome, siglaPartido))?.Id;
                        var idEstado = dbContext.Estados.FirstOrDefault(x => EF.Functions.ILike(x.Sigla, deputadoDetalhes.ultimoStatus.siglaUf))?.Id;
                        var idEstadoNascimento = dbContext.Estados.FirstOrDefault(x => EF.Functions.ILike(x.Sigla, deputadoDetalhes.ufNascimento))?.Id;

                        if (deputadoDb == null)
                        {

                            var deputadoNovo = new Deputado()
                            {
                                Id = deputado.id,
                                IdPartido = idPartido ?? 0,
                                IdEstado = idEstado,
                                IdGabinete = (short?)id_cf_gabinete,
                                Cpf = deputadoDetalhes.cpf,
                                NomeCivil = deputadoDetalhes.nomeCivil.ToTitleCase(),
                                NomeParlamentar = (deputadoDetalhes.ultimoStatus.nomeEleitoral ?? deputadoDetalhes.nomeCivil).ToTitleCase(),
                                Sexo = deputadoDetalhes.sexo,
                                Condicao = deputadoDetalhes.ultimoStatus.condicaoEleitoral,
                                Situacao = situacao,
                                Email = deputadoDetalhes.ultimoStatus.gabinete.email?.ToLower(),
                                Nascimento = deputadoDetalhes.dataNascimento.ToDate(),
                                Falecimento = deputadoDetalhes.dataFalecimento.ToDate(),
                                IdEstadoNascimento = idEstadoNascimento,
                                Municipio = deputadoDetalhes.municipioNascimento,
                                Website = deputadoDetalhes.urlWebsite,
                                Escolaridade = deputadoDetalhes.escolaridade,
                            };

                            dbContext.DeputadosFederais.Add(deputadoNovo);
                            dbContext.SaveChanges();
                            novos++;
                        }
                        else
                        {
                            deputadoDb.IdPartido = idPartido ?? 0;
                            deputadoDb.IdEstado = idEstado;
                            deputadoDb.IdGabinete = (short?)id_cf_gabinete;
                            deputadoDb.Cpf = deputadoDetalhes.cpf;
                            deputadoDb.NomeCivil = deputadoDetalhes.nomeCivil.ToTitleCase();
                            deputadoDb.NomeParlamentar = (deputadoDetalhes.ultimoStatus.nomeEleitoral ?? deputadoDetalhes.nomeCivil).ToTitleCase();
                            deputadoDb.Sexo = deputadoDetalhes.sexo;
                            deputadoDb.Condicao = deputadoDetalhes.ultimoStatus.condicaoEleitoral;
                            deputadoDb.Situacao = situacao;
                            deputadoDb.Email = deputadoDetalhes.ultimoStatus.gabinete.email?.ToLower();
                            deputadoDb.Nascimento = deputadoDetalhes.dataNascimento.ToDate();
                            deputadoDb.Falecimento = deputadoDetalhes.dataFalecimento.ToDate();
                            deputadoDb.IdEstadoNascimento = idEstadoNascimento;
                            deputadoDb.Municipio = deputadoDetalhes.municipioNascimento;
                            deputadoDb.Website = deputadoDetalhes.urlWebsite;
                            deputadoDb.Escolaridade = deputadoDetalhes.escolaridade;

                            dbContext.SaveChanges();
                        }

                        if (deputadoDetalhes.ultimoStatus.idLegislatura == leg)
                        {
                            var possuiMandatosNaLagislatura = dbContext.MandatosCamaraFederal.Any(x => x.IdDeputado == deputado.id && x.IdLegislatura == leg);
                            if (!possuiMandatosNaLagislatura)
                            {
                                var mandato = new MandatoCamara()
                                {
                                    IdDeputado = deputado.id,
                                    IdLegislatura = (byte)leg,
                                    IdEstado = idEstado,
                                    IdPartido = idPartido,
                                    Condicao = deputadoDetalhes.ultimoStatus.condicaoEleitoral
                                };

                                dbContext.MandatosCamaraFederal.Add(mandato);
                                dbContext.SaveChanges();
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
            var deputados = dbContext.DeputadosFederais.Where(x => x.Id > 100 && x.Situacao == "Exercício").OrderBy(x => x.Id).ToList();

            foreach (var deputado in deputados)
            {
                string id = deputado.Id.ToString();
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


                    logger.LogDebug("Atualizado imagem do parlamentar {Parlamentar}.", deputado.NomeParlamentar);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("404"))
                    {
                        logger.LogInformation(ex.GetBaseException(), "Imagem do parlamentar {Parlamentar} inexistente.", deputado.NomeParlamentar);
                    }
                }
            }
        }
    }

    public void AtualizarDatasImportacaoParlamentar(DateTime? pInicio = null, DateTime? pFim = null)
    {
        var importacao = dbContext.Importacoes.FirstOrDefault(x => x.Chave == "Camara Federal");
        if (importacao == null)
        {
            importacao = new Importacao()
            {
                Chave = "Camara Federal"
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
