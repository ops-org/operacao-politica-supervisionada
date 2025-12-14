using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.Utilities;
using RestSharp;

namespace OPS.Importador.CamaraFederal;

public class ImportadorParlamentarCamaraFederal : IImportadorParlamentar
{
    protected readonly ILogger<ImportadorParlamentarCamaraFederal> logger;
    protected readonly IDbConnection connection;

    public string rootPath { get; set; }
    public string tempPath { get; set; }

    private const short legislaturaAtual = 57;

    public HttpClient httpClient { get; }

    public ImportadorParlamentarCamaraFederal(IServiceProvider serviceProvider)
    {
        logger = serviceProvider.GetService<ILogger<ImportadorParlamentarCamaraFederal>>();
        connection = serviceProvider.GetService<IDbConnection>();

        var configuration = serviceProvider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
        rootPath = configuration["AppSettings:SiteRootFolder"];
        tempPath = configuration["AppSettings:SiteTempFolder"];

        httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient("DefaultClient");
    }

    public Task Importar()
    {
        ImportarDeputados(legislaturaAtual - 1);

        return Task.CompletedTask;
    }

    public void ImportarDeputados(int legislaturaInicial)
    {
        int novos = 0;
        int pagina;
        var restClient = new RestClient(httpClient, new RestClientOptions()
        {
            BaseUrl = new Uri("https://dadosabertos.camara.leg.br/api/v2/deputados")
        });

        List<Dado> deputados;
        List<Link> links;

        using (var banco = new AppDb())
        {
            banco.ExecuteNonQuery(@"UPDATE cf_deputado SET id_cf_gabinete=NULL");

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

                        banco.AddParameter("id", deputado.id);
                        var count = banco.ExecuteScalar(@"SELECT COUNT(1) FROM cf_deputado WHERE id=@id");

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
                            banco.AddParameter("sala", deputadoDetalhes.ultimoStatus.gabinete.sala.PadLeft(3, '0'));
                            var id = banco.ExecuteScalar(@"SELECT id from cf_gabinete where sala = @sala");

                            var gabinete = deputadoDetalhes.ultimoStatus.gabinete;

                            if (!string.IsNullOrEmpty(gabinete.sala))
                                if (id == null || Convert.IsDBNull(id))
                                {
                                    banco.AddParameter("id", gabinete.sala);
                                    banco.AddParameter("nome", gabinete.nome);
                                    banco.AddParameter("predio", gabinete.predio);
                                    banco.AddParameter("andar", gabinete.andar);
                                    banco.AddParameter("sala", gabinete.sala);
                                    banco.AddParameter("telefone", gabinete.telefone);

                                    banco.ExecuteNonQuery(@"INSERT INTO cf_gabinete (
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
                                        )");

                                    id_cf_gabinete = Convert.ToInt32(gabinete.sala);
                                }
                                else
                                {
                                    id_cf_gabinete = Convert.ToInt32(id);

                                    if (leg == legislaturaAtual)
                                    {
                                        banco.AddParameter("nome", gabinete.nome);
                                        banco.AddParameter("predio", gabinete.predio);
                                        banco.AddParameter("andar", gabinete.andar);
                                        banco.AddParameter("telefone", gabinete.telefone);
                                        banco.AddParameter("sala", gabinete.sala);

                                        banco.ExecuteNonQuery(@"UPDATE cf_gabinete SET
                                                nome = @nome,
                                                predio = @predio,
                                                andar = @andar,
                                                telefone = @telefone
                                                WHERE sala = @sala
                                            ");
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
                            banco.AddParameter("id", deputado.id);
                            banco.AddParameter("sigla_partido", siglaPartido);
                            banco.AddParameter("sigla_estado", deputadoDetalhes.ultimoStatus.siglaUf);
                            banco.AddParameter("id_cf_gabinete", id_cf_gabinete);
                            banco.AddParameter("cpf", deputadoDetalhes.cpf);
                            banco.AddParameter("nome_civil", deputadoDetalhes.nomeCivil.ToTitleCase());
                            banco.AddParameter("nome_parlamentar", (deputadoDetalhes.ultimoStatus.nomeEleitoral ?? deputadoDetalhes.nomeCivil).ToTitleCase());
                            banco.AddParameter("sexo", deputadoDetalhes.sexo);
                            banco.AddParameter("condicao", deputadoDetalhes.ultimoStatus.condicaoEleitoral);
                            banco.AddParameter("situacao", situacao);
                            banco.AddParameter("email", deputadoDetalhes.ultimoStatus.gabinete.email?.ToLower());
                            banco.AddParameter("nascimento", deputadoDetalhes.dataNascimento);
                            banco.AddParameter("falecimento", deputadoDetalhes.dataFalecimento);
                            banco.AddParameter("sigla_estado_nascimento", deputadoDetalhes.ufNascimento);
                            banco.AddParameter("municipio", deputadoDetalhes.municipioNascimento);
                            banco.AddParameter("website", deputadoDetalhes.urlWebsite);
                            banco.AddParameter("escolaridade", deputadoDetalhes.escolaridade);

                            banco.ExecuteNonQuery(@"INSERT INTO cf_deputado (
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
                                )");

                            novos++;
                        }
                        else
                        {
                            banco.AddParameter("sigla_partido", siglaPartido);
                            banco.AddParameter("sigla_estado", deputadoDetalhes.ultimoStatus.siglaUf);
                            banco.AddParameter("id_cf_gabinete", id_cf_gabinete);
                            banco.AddParameter("cpf", deputadoDetalhes.cpf);
                            banco.AddParameter("nome_civil", deputadoDetalhes.nomeCivil.ToTitleCase());
                            banco.AddParameter("nome_parlamentar", (deputadoDetalhes.ultimoStatus.nomeEleitoral ?? deputadoDetalhes.nomeCivil).ToTitleCase());
                            banco.AddParameter("sexo", deputadoDetalhes.sexo);
                            banco.AddParameter("condicao", deputadoDetalhes.ultimoStatus.condicaoEleitoral);
                            banco.AddParameter("situacao", situacao);
                            banco.AddParameter("email", deputadoDetalhes.ultimoStatus.gabinete.email?.ToLower());
                            banco.AddParameter("nascimento", deputadoDetalhes.dataNascimento.NullIfEmpty());
                            banco.AddParameter("falecimento", deputadoDetalhes.dataFalecimento.NullIfEmpty());
                            banco.AddParameter("sigla_estado_nascimento", deputadoDetalhes.ufNascimento);
                            banco.AddParameter("municipio", deputadoDetalhes.municipioNascimento);
                            banco.AddParameter("website", deputadoDetalhes.urlWebsite);
                            banco.AddParameter("escolaridade", deputadoDetalhes.escolaridade);
                            banco.AddParameter("id", deputado.id);

                            banco.ExecuteNonQuery(@"
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
                            ");
                        }

                        if (deputadoDetalhes.ultimoStatus.idLegislatura == leg)
                        {
                            banco.AddParameter("id_cf_deputado", deputado.id);
                            banco.AddParameter("id_legislatura", leg);
                            var countMandato = banco.ExecuteScalar(@"
                                    SELECT 1 FROM cf_mandato WHERE id_cf_deputado = @id_cf_deputado AND id_legislatura = @id_legislatura");

                            if (Convert.ToInt32(countMandato) == 0)
                            {
                                banco.AddParameter("id_cf_deputado", deputado.id);
                                banco.AddParameter("id_legislatura", leg);
                                banco.AddParameter("sigla_estado", deputadoDetalhes.ultimoStatus.siglaUf);
                                banco.AddParameter("sigla_partido", siglaPartido);
                                banco.AddParameter("condicao", deputadoDetalhes.ultimoStatus.condicaoEleitoral);

                                banco.ExecuteNonQuery(@"INSERT INTO cf_mandato (
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
                                    )");
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

        using (var banco = new AppDb())
        {
            string sql = "SELECT id, nome_parlamentar FROM cf_deputado where id > 100 order by id -- AND situacao = 'Exercício'";
            DataTable table = banco.GetTable(sql, 0);

            foreach (DataRow row in table.Rows)
            {
                string id = row["id"].ToString();
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


                    logger.LogDebug("Atualizado imagem do parlamentar {Parlamentar}.", row["nome_parlamentar"].ToString());
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("404"))
                    {
                        logger.LogInformation(ex.GetBaseException(), "Imagem do parlamentar {Parlamentar} inexistente.", row["nome_parlamentar"].ToString());
                    }
                }
            }
        }
    }

    public void AtualizarDatasImportacaoParlamentar(DateTime? pInicio = null, DateTime? pFim = null)
    {
        var importacao = connection.GetList<Importacao>(new { nome = "Camara Federal" }).FirstOrDefault();
        if (importacao == null)
        {
            importacao = new Importacao()
            {
                Nome = "Camara Federal"
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
