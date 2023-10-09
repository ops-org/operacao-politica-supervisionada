using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AngleSharp;
using AngleSharp.Html.Dom;
using CsvHelper;
using Dapper;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;
using OPS.Importador.Utilities;
using RestSharp;

namespace OPS.Importador;

public class CamaraFederal : ImportadorBase
{
    public CamaraFederal(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarCamaraFederal(serviceProvider);
        importadorDespesas = new ImportadorDespesasCamaraFederal(serviceProvider);
    }
}

public class ImportadorParlamentarCamaraFederal : IImportadorParlamentar
{
    protected readonly ILogger<ImportadorParlamentarCamaraFederal> logger;
    protected readonly IDbConnection connection;

    public ImportadorParlamentarCamaraFederal(IServiceProvider serviceProvider)
    {
        logger = serviceProvider.GetService<ILogger<ImportadorParlamentarCamaraFederal>>();
        connection = serviceProvider.GetService<IDbConnection>();
    }

    public Task Importar()
    {
        ImportarDeputados(56);

        return Task.CompletedTask;
    }

    public void ImportarDeputados(int legislaturaInicial)
    {
        logger.LogWarning($"Importar Parlamentares do Camara Federal");

        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
        int novos = 0;
        int pagina;
        var restClient = new RestClient("https://dadosabertos.camara.leg.br/api/v2/deputados/");
        //restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

        List<Dado> deputados;
        List<Link> links;

        using (var banco = new AppDb())
        {
            banco.ExecuteNonQuery(@"UPDATE cf_deputado SET id_cf_gabinete=NULL");

            for (int leg = legislaturaInicial; leg <= 56; leg++)
            {
                pagina = 1;
                do
                {
                    var uri = string.Format("?idLegislatura={0}&pagina={1}&itens=100", leg, pagina++);
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
                                    logger.LogError("Erro ao consultar deputado: {@Response}", response);
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                logger.LogInformation("Erro ao consultar deputado: {Id} {ErrorMessage}. Nova tentativa em 30s.", deputado.id, ex.Message);
                                Thread.Sleep(TimeSpan.FromSeconds(30));

                                var x = restClient.Execute<DeputadoDetalhes>(request);
                                deputadoDetalhes = x.Data.dados;

                                if (deputadoDetalhes == null)
                                {
                                    logger.LogError("Erro ao consultar deputado: {ErrorMessage}", deputado.id, ex.Message);
                                    continue;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError("Erro ao consultar deputado: {ErrorMessage}", deputado.id, ex.Message);
                            continue;
                        }


                        logger.LogDebug("Sincronizando dados do Deputado: {IdDeputado} - {NomeDeputado} ({SiglaPartido})",
                            deputado.id, textInfo.ToTitleCase(deputadoDetalhes.ultimoStatus.nomeEleitoral ?? deputadoDetalhes.nomeCivil), deputadoDetalhes.ultimoStatus.siglaPartido);

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

                                    if (leg == 56)
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

                        if (leg == 56)
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
                            banco.AddParameter("nome_civil", textInfo.ToTitleCase(deputadoDetalhes.nomeCivil.ToLower()).Replace("De", "de").Replace("Da", "da"));
                            banco.AddParameter("nome_parlamentar", textInfo.ToTitleCase((deputadoDetalhes.ultimoStatus.nomeEleitoral ?? deputadoDetalhes.nomeCivil).ToLower()).Replace("De", "de").Replace("Da", "da"));
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
                            banco.AddParameter("nome_civil", textInfo.ToTitleCase(deputadoDetalhes.nomeCivil));
                            banco.AddParameter("nome_parlamentar", textInfo.ToTitleCase(deputadoDetalhes.ultimoStatus.nomeEleitoral ?? deputadoDetalhes.nomeCivil));
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
}

public class ImportadorDespesasCamaraFederal : IImportadorDespesas
{
    protected readonly ILogger<ImportadorDespesasCamaraFederal> logger;
    protected readonly IDbConnection connection;

    public string rootPath { get; set; }
    public string tempPath { get; set; }

    private int linhasProcessadasAno { get; set; }

    public ImportadorDespesasCamaraFederal(IServiceProvider serviceProvider)
    {
        logger = serviceProvider.GetService<ILogger<ImportadorDespesasCamaraFederal>>();
        connection = serviceProvider.GetService<IDbConnection>();

        var configuration = serviceProvider.GetService<Microsoft.Extensions.Configuration.IConfiguration>();
        rootPath = configuration["AppSettings:SiteRootFolder"];
        tempPath = configuration["AppSettings:SiteTempFolder"];
    }

    #region Importação Deputados

    /// <summary>
    /// Importar mandatos
    /// </summary>
    public void ImportarMandatos()
    {
        // http://www2.camara.leg.br/transparencia/dados-abertos/dados-abertos-legislativo/webservices/deputados
        // http://www.camara.leg.br/internet/deputado/DeputadosXML_52a55.zip

        var doc = new XmlDocument();
        doc.Load(@"C:\Users\Lenovo\Downloads\Deputados.xml");
        XmlNode deputados = doc.DocumentElement;

        var deputado = deputados.SelectNodes("Deputados/Deputado");
        var sqlFields = new StringBuilder();
        var sqlValues = new StringBuilder();

        using (var banco = new AppDb())
        {
            banco.ExecuteNonQuery("TRUNCATE TABLE cf_mandato_temp");

            foreach (XmlNode fileNode in deputado)
            {
                sqlFields.Clear();
                sqlValues.Clear();

                foreach (XmlNode item in fileNode.SelectNodes("*"))
                {
                    sqlFields.Append($",{item.Name}");

                    sqlValues.Append($",@{item.Name}");
                    banco.AddParameter(item.Name, item.InnerText);
                }

                banco.ExecuteNonQuery("INSERT cf_mandato_temp (" + sqlFields.ToString().Substring(1) + ")  values (" +
                                      sqlValues.ToString().Substring(1) + ")");
            }

            banco.ExecuteNonQuery(@"
                    SET SQL_BIG_SELECTS=1;

					INSERT INTO cf_mandato (
	                    id_cf_deputado,
	                    id_legislatura,
	                    id_carteira_parlamantar,
	                    id_estado, 
	                    id_partido,
                        condicao
                    )
                    select 
	                    mt.ideCadastro,
	                    mt.numLegislatura,
	                    mt.Matricula,
	                    e.id, 
	                    p.id,
	                    mt.Condicao
                    FROM cf_mandato_temp mt
                    left join partido p on p.sigla = mt.LegendaPartidoEleito
                    left join estado e on e.sigla = mt.UFEleito
                    /* Inserir apenas faltantes */
                    LEFT JOIN cf_mandato m ON m.id_cf_deputado = mt.ideCadastro
	                    AND m.id_legislatura = mt.numLegislatura 
	                    AND m.id_carteira_parlamantar = mt.Matricula
                    WHERE m.id is null;
                    
                    SET SQL_BIG_SELECTS=0;
				");

            //banco.ExecuteNonQuery("TRUNCATE TABLE cf_mandato_temp;");
        }
    }

    //    /// <summary>
    //    /// Atualiza informações dos deputados em exercício na Câmara dos Deputados (1)
    //    /// CREATE TABLE `cf_deputado_temp` (
    //    ///	`ideCadastro` BIGINT(19) NULL DEFAULT NULL,
    //    ///	`codOrcamento` VARCHAR(10) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    ///	`condicao` VARCHAR(50) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    ///	`matricula` INT(10) NULL DEFAULT NULL,
    //    ///	`idParlamentar` INT(10) NULL DEFAULT NULL,
    //    ///	`nome` VARCHAR(255) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    ///	`nomeParlamentar` VARCHAR(100) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    ///	`urlFoto` VARCHAR(255) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    ///	`sexo` VARCHAR(10) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    ///	`uf` VARCHAR(2) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    ///	`partido` VARCHAR(50) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    ///	`gabinete` VARCHAR(20) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    ///	`anexo` VARCHAR(50) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    ///	`fone` VARCHAR(100) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    ///	`email` VARCHAR(100) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci'
    //    /// )
    // COLLATE='utf8mb4_0900_ai_ci'
    // ENGINE=InnoDB
    //    /// </summary>
    //    public  string AtualizaInfoDeputados()
    //    {
    //        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
    //        string retorno = string.Empty;
    //        // http://www2.camara.leg.br/transparencia/dados-abertos/dados-abertos-legislativo/webservices/deputados

    //        var doc = new XmlDocument();
    //        var client = new RestClient("http://www.camara.leg.br/SitCamaraWS/Deputados.asmx");
    //        var request = new RestRequest("ObterDeputados", Method.GET);
    //        var response = client.Execute(request);
    //        doc.LoadXml(response.Content);
    //        XmlNode deputados = doc.DocumentElement;

    //        var deputado = deputados.SelectNodes("*");
    //        var sqlFields = new StringBuilder();
    //        var sqlValues = new StringBuilder();

    //        using (var banco = new AppDb())
    //        {
    //            banco.ExecuteNonQuery("TRUNCATE TABLE cf_deputado_temp");

    //            foreach (XmlNode fileNode in deputado)
    //            {
    //                sqlFields.Clear();
    //                sqlValues.Clear();

    //                foreach (XmlNode item in fileNode.SelectNodes("*"))
    //                {
    //                    if (item.Name != "comissoes")
    //                    {
    //                        sqlFields.Append(string.Format(",{0}", item.Name));

    //                        sqlValues.Append(string.Format(",@{0}", item.Name));
    //                        if (item.Name == "nome" || item.Name == "nomeParlamentar")
    //                        {
    //                            banco.AddParameter(item.Name, textInfo.ToTitleCase(item.InnerText));
    //                        }
    //                        else
    //                        {
    //                            banco.AddParameter(item.Name, item.InnerText);
    //                        }

    //                    }
    //                }

    //                banco.ExecuteNonQuery("INSERT cf_deputado_temp (" + sqlFields.ToString().Substring(1) +
    //                                      ")  values (" + sqlValues.ToString().Substring(1) + ")");
    //            }


    //            banco.ExecuteNonQuery(@"
    //                SET SQL_BIG_SELECTS=1;

    //	update cf_deputado d
    //	left join cf_deputado_temp dt on dt.numeroDeputadoID = d.id_deputado
    //	left join partido p on p.sigla = dt.partido
    //	left join estado e on e.sigla = dt.uf
    //	set
    //                    -- d.condicao = dt.condicao,
    //		-- d.nome_civil = dt.nome,
    //		-- d.nome_parlamentar = dt.nomeParlamentar,
    //		-- d.url_foto = dt.urlFoto,
    //		-- d.sexo = LEFT(dt.sexo, 1),
    //		-- d.id_estado = e.id,
    //		-- d.id_partido = p.id,
    //		-- d.gabinete = dt.gabinete,
    //		-- d.anexo = dt.anexo,
    //		-- d.fone = dt.fone,
    //		 d.email = dt.email
    //	where dt.nomeParlamentar is not null;

    //                SET SQL_BIG_SELECTS=0;
    //");

    //            if (banco.Rows > 0)
    //            {
    //                retorno = "<p>" + banco.Rows + "+ Mandato<p>";
    //            }

    //            //banco.ExecuteNonQuery("TRUNCATE TABLE cf_deputado_temp");

    //            return retorno;
    //        }
    //    }

    //    /// <summary>
    //    /// Atualiza informações dos deputados em exercício na Câmara dos Deputados (2)
    //    /// CREATE TABLE `cf_deputado_temp_detalhes` (
    //    /// 	`numLegislatura` INT(10) NULL DEFAULT NULL,
    //    /// 	`ideCadastro` BIGINT(19) NULL DEFAULT NULL,
    //    /// 	`idParlamentarDeprecated` INT(10) NULL DEFAULT NULL,
    //    /// 	`nomeCivil` VARCHAR(255) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    /// 	`nomeParlamentarAtual` VARCHAR(100) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    /// 	`sexo` VARCHAR(10) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    /// 	`ufRepresentacaoAtual` VARCHAR(2) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    /// 	`sigla` VARCHAR(50) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    /// 	`numero` VARCHAR(20) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    /// 	`anexo` VARCHAR(50) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    /// 	`telefone` VARCHAR(100) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    /// 	`email` VARCHAR(100) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    /// 	`nomeProfissao` VARCHAR(255) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci',
    //    /// 	`dataNascimento` DATE NULL DEFAULT NULL,
    //    /// 	`dataFalecimento` DATE NULL DEFAULT NULL,
    //    /// 	`situacaoNaLegislaturaAtual` VARCHAR(30) NULL DEFAULT NULL COLLATE 'utf8mb4_0900_ai_ci'
    //    /// )
    //    /// COLLATE='utf8mb4_0900_ai_ci'
    //    /// ENGINE=InnoDB
    //    /// </summary>
    //    public  string AtualizaInfoDeputadosCompleto(bool cargaInicial = false)
    //    {
    //        string retorno = string.Empty;
    //        // http://www2.camara.leg.br/transparencia/dados-abertos/dados-abertos-legislativo/webservices/deputados

    //        var sqlFields = new StringBuilder();
    //        var sqlValues = new StringBuilder();

    //        using (var banco = new AppDb())
    //        {
    //            banco.ExecuteNonQuery("TRUNCATE TABLE cf_deputado_temp_detalhes");

    //            var dt = banco.GetTable("SELECT id from cf_deputado" + (!cargaInicial ? " WHERE situacao <> 'Fim de Mandato'" : ""));

    //            foreach (DataRow dr in dt.Rows)
    //            {
    //                // Retorna detalhes dos deputados com histórico de participação em comissões, períodos de exercício, filiações partidárias e lideranças.
    //                var doc = new XmlDocument();
    //                var client = new RestClient("http://www.camara.leg.br/SitCamaraWS/Deputados.asmx/");
    //                var request = new RestRequest("ObterDetalhesDeputado?numLegislatura=" + (!cargaInicial ? "56" : "") + "&numeroDeputadoID=" + dr["id"].ToString(), Method.GET);

    //                try
    //                {
    //                    var response = client.Execute(request);
    //                    doc.LoadXml(response.Content);
    //                }
    //                catch (Exception)
    //                {
    //                    Thread.Sleep(5000);

    //                    try
    //                    {
    //                        var response = client.Execute(request);
    //                        doc.LoadXml(response.Content);
    //                    }
    //                    catch (Exception)
    //                    {
    //                        continue;
    //                    }
    //                }

    //                XmlNode deputados = doc.DocumentElement;
    //                var deputado = deputados.SelectNodes("*");

    //                foreach (XmlNode fileNode in deputado)
    //                {
    //                    sqlFields.Clear();
    //                    sqlValues.Clear();

    //                    foreach (XmlNode item in fileNode.ChildNodes)
    //                    {
    //                        if (item.Name == "comissoes") break;
    //                        if ((item.Name != "partidoAtual") && (item.Name != "gabinete"))
    //                        {
    //                            object value;
    //                            if (item.Name.StartsWith("data"))
    //                                if (!string.IsNullOrEmpty(item.InnerText))
    //                                    value = DateTime.Parse(item.InnerText).ToString("yyyy-MM-dd");
    //                                else
    //                                    value = DBNull.Value;
    //                            else
    //                                value = item.InnerText;

    //                            sqlFields.Append($",{item.Name}");
    //                            sqlValues.Append($",@{item.Name}");
    //                            banco.AddParameter(item.Name, value);
    //                        }
    //                        else
    //                        {
    //                            foreach (XmlNode item2 in item.ChildNodes)
    //                            {
    //                                if ((item2.Name == "idPartido") || (item2.Name == "nome")) continue;


    //                                sqlFields.Append($",{item2.Name}");
    //                                sqlValues.Append($",@{item2.Name}");
    //                                banco.AddParameter(item2.Name, item2.InnerText);
    //                            }
    //                        }
    //                    }

    //                    try
    //                    {
    //                        banco.ExecuteNonQuery("INSERT cf_deputado_temp_detalhes (" +
    //                                               sqlFields.ToString().Substring(1) + ")  values (" +
    //                                               sqlValues.ToString().Substring(1) + ")");

    //                        break;
    //                    }
    //                    catch
    //                    {
    //                        // ignored
    //                    }
    //                }
    //            }

    //            banco.ExecuteNonQuery(@"
    //            	update cf_deputado d
    //            	left join (
    //            		select		
    //            			numeroDeputadoID,
    //            			idParlamentarDeprecated,
    //            			nomeCivil,
    //            			nomeParlamentarAtual,
    //            			sexo,
    //            			nomeProfissao,
    //            			dataNascimento,
    //            			ufRepresentacaoAtual,
    //            			max(email) as email,
    //            			max(sigla) as sigla,
    //            			max(numero) as numero,
    //            			max(anexo) as anexo,
    //            			max(telefone) as telefone,
    //            			max(dataFalecimento) as dataFalecimento
    //            		from cf_deputado_temp_detalhes
    //            		group by 1, 2, 3, 4, 5, 6, 7, 8
    //            	) dt on dt.numeroDeputadoID = d.id_cf_deputado
    //            	left join partido p on p.sigla = dt.sigla
    //            	left join estado e on e.sigla = dt.ufRepresentacaoAtual
    //            	set
    //            		d.nome_civil = dt.nomeCivil,
    //            		d.nome_parlamentar = dt.nomeParlamentarAtual,
    //            		d.sexo = LEFT(dt.sexo, 1),
    //            		d.id_estado = e.id,
    //            		d.id_partido = p.id,
    //            		d.gabinete = dt.numero,
    //            		d.anexo = dt.anexo,
    //            		d.fone = dt.telefone,
    //            		d.email = dt.email,
    //            		d.profissao = dt.nomeProfissao,
    //            		d.nascimento = dt.dataNascimento,
    //            		d.falecimento = dt.dataFalecimento
    //            	where dt.nomeParlamentarAtual is not null;
    //            ");

    //            if (banco.Rows > 0)
    //            {
    //                retorno = "<p>" + banco.Rows + "+ Mandato<p>";
    //            }

    //            //banco.ExecuteNonQuery("TRUNCATE TABLE cf_deputado_temp_detalhes;");
    //        }

    //        return retorno;
    //    }

    public string ImportaPresencasDeputados()
    {
        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
        var sb = new StringBuilder();
        Console.WriteLine("Iniciando Importação de presenças");


        string checksum = string.Empty;

        DataTable dtMandatos;
        DataTable dtSessoes;
        using (var banco = new AppDb())
        {
            dtMandatos = banco.GetTable("select id_cf_deputado, id_legislatura, id_carteira_parlamantar from cf_mandato");
            dtSessoes = banco.GetTable("select id, id_legislatura, data, inicio, tipo, numero, checksum from cf_sessao");
        }

        //Carregar a partir da legislatura 53 (2007/02/01). Existem dados desde 24/02/99
        DateTime dtPesquisa = Padrao.DeputadoFederalPresencaUltimaAtualizacao.Date; //new DateTime(2007, 2, 1);
        if (dtPesquisa == DateTime.MinValue) dtPesquisa = new DateTime(2016, 2, 9);
        int ano_inicio = dtPesquisa.Year;
        var dtNow = DateTime.Now.Date;

        if (dtPesquisa == dtNow)
        {
            Console.WriteLine("Presenças já importadas hoje!");
            sb.AppendFormat("Presenças já importadas hoje!");
            return sb.ToString();
        }

        //var dtUltimaIntegracao = dtPesquisa.AddDays(-7);
        int presencas_importadas = 0;

        dtPesquisa = dtPesquisa.AddDays(-30);

        while (true)
        {
            dtPesquisa = dtPesquisa.AddDays(1);
            if (dtPesquisa > dtNow) break;

            var doc = new XmlDocument();
            RestResponse response;
            var restClient = new RestClient("http://www.camara.leg.br/SitCamaraWS/sessoesreunioes.asmx/");
            //restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            var request =
                new RestRequest(
                   string.Format("ListarPresencasDia?data={0:dd/MM/yyyy}&numLegislatura=&numMatriculaParlamentar=&siglaPartido=&siglaUF=", dtPesquisa));

            //if (dtPesquisa < dtUltimaIntegracao && dtSessoes.Select().All(r => Convert.ToDateTime(r["data"]) != dtPesquisa))
            //{
            //    Console.WriteLine("Não Houve sessão no dia: " + dtPesquisa.ToShortDateString());
            //    sb.AppendFormat("<p>Não Houve sessão no dia: {0:dd/MM/yyyy}</p>", dtPesquisa);

            //    // Não houve sessão no dia.
            //    continue;
            //}

            try
            {
                response = restClient.GetWithAutoRetry(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro:" + ex.ToFullDescriptionString());

                if (ex.Message.Contains("timeout"))
                {
                    Thread.Sleep(5000);

                    dtPesquisa = dtPesquisa.AddDays(-1);
                    continue;
                }
                else
                {
                    sb.AppendFormat("<p>Erro:{0}</p>", ex.ToFullDescriptionString());

                    return sb.ToString();
                }
            }

            if (!response.Content.Contains("qtdeSessoesDia"))
            {
                if (response.Content.Contains(@"<?xml version=""1.0"" encoding=""utf-8""?>"))
                {
                    //sb.AppendFormat("<p>Não Houve sessão no dia: {0:dd/MM/yyyy}</p>", dtPesquisa);
                    continue;
                }
            }

            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                checksum = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(response.Content)));
                List<DataRow> drSessoes = dtSessoes.Select().Where(r => Convert.ToDateTime(r["data"]) == dtPesquisa).ToList();

                if (drSessoes.Count > 0)
                {
                    if (drSessoes[0]["checksum"].ToString() == checksum)
                    {
                        sb.AppendFormat("<p>Sem alterações na sessão no dia: {0:dd/MM/yyyy}</p>", dtPesquisa);
                        continue;
                    }
                    else
                    {
                        using (var banco = new AppDb())
                        {
                            foreach (DataRow dr in drSessoes)
                            {
                                var id_cf_sessao = Convert.ToInt32(dr["id"]);

                                banco.AddParameter("id", id_cf_sessao);
                                banco.ExecuteNonQuery("delete from cf_sessao_presenca where id_cf_sessao=@id");

                                banco.AddParameter("id", id_cf_sessao);
                                banco.ExecuteNonQuery("delete from cf_sessao where id=@id");
                            }
                        }
                    }
                }
            }

            doc.LoadXml(response.Content);

            Console.WriteLine("Importando presenças do dia: " + dtPesquisa.ToShortDateString());
            sb.AppendFormat("<p>Importando presenças do dia: {0:dd/MM/yyyy}</p>", dtPesquisa);
            XmlNode dia = doc.DocumentElement;

            var lstSessaoDia = new Dictionary<string, int>();

            using (var banco = new AppDb())
            {
                var count = Convert.ToInt32(dia["qtdeSessoesDia"].InnerText);
                for (var i = 0; i < count; i++)
                {
                    presencas_importadas++;
                    var temp = dia.SelectNodes("parlamentares/parlamentar/sessoesDia/sessaoDia").Item(i);

                    var inicio = temp["inicio"].InnerText;

                    if (lstSessaoDia.ContainsKey(inicio)) continue; // ignorar registro duplicado

                    var descricao =
                        temp["descricao"].InnerText
                            .Replace("N º", "").Replace("Nº", "")
                            .Replace("SESSÃO PREPARATÓRIA", "SESSÃO_PREPARATÓRIA")
                            .Split(new[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    banco.AddParameter("id_legislatura", dia["legislatura"].InnerText);
                    banco.AddParameter("data", dtPesquisa);
                    banco.AddParameter("inicio", Convert.ToDateTime(inicio, cultureInfo));

                    //1=> ORDINÁRIA, 2=> EXTRAORDINÁRIA, 3=> SESSÃO PREPARATÓRIA
                    var tipo = 0;
                    switch (descricao[0])
                    {
                        case "ORDINÁRIA":
                        case "ORDINARIA":
                            tipo = 1;
                            break;
                        case "EXTRAORDINÁRIA":
                        case "EXTRAORDINARIA":
                            tipo = 2;
                            break;
                        case "SESSÃO_PREPARATÓRIA":
                        case "SESSÃO_PREPARATORIA":
                        case "PREPARATÓRIA":
                        case "PREPARATORIA":
                            tipo = 3;
                            break;
                        default:
                            throw new NotImplementedException("");
                    }

                    banco.AddParameter("tipo", tipo);
                    banco.AddParameter("numero", descricao[1]);
                    banco.AddParameter("checksum", checksum);

                    var id_cf_secao = Convert.ToInt32(banco.ExecuteScalar(
                        @"INSERT cf_sessao (
								id_legislatura, data, inicio, tipo, numero, checksum
							) values ( 
								@id_legislatura, @data, @inicio, @tipo, @numero, @checksum
							); 
							
							SELECT LAST_INSERT_ID();"));

                    lstSessaoDia.Add(inicio, id_cf_secao);
                }

                var parlamentares = dia.SelectNodes("parlamentares/parlamentar");

                foreach (XmlNode parlamentar in parlamentares)
                {
                    var sessoesDia = parlamentar.SelectNodes("sessoesDia/sessaoDia");

                    foreach (XmlNode sessaoDia in sessoesDia)
                    {
                        string id_cf_deputado;
                        var drMqandato = dtMandatos.Select(
                            string.Format("id_legislatura={0} and id_carteira_parlamantar={1}",
                                dia["legislatura"].InnerText,
                                parlamentar["carteiraParlamentar"].InnerText
                            ));

                        if (drMqandato.Length == 0)
                        {
                            try
                            {
                                var nome_parlamentar = parlamentar["nomeParlamentar"].InnerText.Split('-');
                                banco.AddParameter("nome_parlamentar", nome_parlamentar[0]);
                                id_cf_deputado = banco.ExecuteScalar("SELECT id FROM cf_deputado where IFNULL(nome_importacao_presenca, nome_parlamentar) like @nome_parlamentar").ToString();
                            }
                            catch (Exception)
                            {
                                sb.AppendFormat("<p>1. Parlamentar '{0}' não consta na base de dados, e foi ignorado na importação de presenças. Legislatura: {1}, Carteira: {2}.</p>",
                                    parlamentar["nomeParlamentar"].InnerText, dia["legislatura"].InnerText, parlamentar["carteiraParlamentar"].InnerText);
                                continue;
                            }

                            drMqandato = dtMandatos.Select(string.Format("id_legislatura={0} and id_cf_deputado={1}", dia["legislatura"].InnerText, id_cf_deputado));

                            if (drMqandato.Length > 0)
                            {
                                drMqandato[0]["id_carteira_parlamantar"] = parlamentar["carteiraParlamentar"].InnerText;

                                banco.AddParameter("id_carteira_parlamantar", parlamentar["carteiraParlamentar"].InnerText);
                                banco.AddParameter("id_legislatura", dia["legislatura"].InnerText);
                                banco.AddParameter("id_cf_deputado", id_cf_deputado);
                                banco.ExecuteNonQuery("UPDATE cf_mandato SET id_carteira_parlamantar=@id_carteira_parlamantar WHERE id_legislatura=@id_legislatura and id_cf_deputado=@id_cf_deputado");
                            }
                        }

                        if (drMqandato.Length > 0)
                        {
                            id_cf_deputado = drMqandato[0]["id_cf_deputado"].ToString();
                        }
                        else
                        {
                            var nome_parlamentar = parlamentar["nomeParlamentar"].InnerText.Split('-');
                            var sigla_partido = "";
                            var sigla_estado = "";
                            try
                            {
                                var temp = nome_parlamentar[1].Split('/');
                                sigla_partido = temp[0];
                                sigla_estado = temp[1];
                            }
                            catch (Exception e)
                            {
                                var x = e;
                            }

                            try
                            {
                                banco.AddParameter("nome_parlamentar", nome_parlamentar[0]);
                                id_cf_deputado = banco.ExecuteScalar("SELECT id FROM cf_deputado where IFNULL(nome_importacao_presenca, nome_parlamentar) like @nome_parlamentar").ToString();
                            }
                            catch (Exception)
                            {
                                sb.AppendFormat("<p>1. Parlamentar '{0}' não consta na base de dados, e foi ignorado na importação de presenças. Legislatura: {1}, Carteira: {2}.</p>",
                                    parlamentar["nomeParlamentar"].InnerText, dia["legislatura"].InnerText, parlamentar["carteiraParlamentar"].InnerText);
                                continue;
                            }

                            banco.AddParameter("id_cf_deputado", id_cf_deputado);
                            banco.AddParameter("id_legislatura", dia["legislatura"].InnerText);
                            banco.AddParameter("id_carteira_parlamantar", parlamentar["carteiraParlamentar"].InnerText);
                            banco.AddParameter("sigla_estado", sigla_estado);
                            banco.AddParameter("sigla_partido", sigla_partido);

                            try
                            {
                                banco.ExecuteScalar(
                                    @"INSERT INTO cf_mandato (
	                                    id_cf_deputado, id_legislatura, id_carteira_parlamantar, id_estado, id_partido
                                    ) VALUES (
	                                    @id_cf_deputado
                                        , @id_legislatura
                                        , @id_carteira_parlamantar
                                        , (SELECT id FROM estado where sigla like @sigla_estado)
                                        , (SELECT id FROM partido where sigla like @sigla_partido OR nome like @sigla_partido)
                                    );");

                                // generate the data you want to insert
                                var toInsert = dtMandatos.NewRow();
                                toInsert["id_cf_deputado"] = id_cf_deputado;
                                toInsert["id_legislatura"] = dia["legislatura"].InnerText;
                                toInsert["id_carteira_parlamantar"] = parlamentar["carteiraParlamentar"].InnerText;

                                // insert in the desired place
                                dtMandatos.Rows.Add(toInsert);
                            }
                            catch (Exception)
                            {
                                // parlamentar não existe na base
                                Console.WriteLine(parlamentar["nomeParlamentar"].InnerText + "/" + dia["legislatura"].InnerText + "/" + parlamentar["carteiraParlamentar"].InnerText);

                                sb.AppendFormat("<p>2. Parlamentar '{0}' não consta na base de dados, e foi ignorado na importação de presenças. Legislatura: {1}, Carteira: {2}.</p>",
                                    parlamentar["nomeParlamentar"].InnerText, dia["legislatura"].InnerText, parlamentar["carteiraParlamentar"].InnerText);
                            }
                        }


                        banco.AddParameter("id_cf_sessao", lstSessaoDia[sessaoDia["inicio"].InnerText]);
                        banco.AddParameter("id_cf_deputado", Convert.ToInt32(id_cf_deputado));

                        banco.AddParameter("presente", sessaoDia["frequencia"].InnerText == "Presença" ? 1 : 0);
                        banco.AddParameter("justificativa", parlamentar["justificativa"].InnerText);
                        banco.AddParameter("presenca_externa", parlamentar["presencaExterna"].InnerText);

                        banco.ExecuteNonQuery(
                            "INSERT cf_sessao_presenca (id_cf_sessao, id_cf_deputado, presente, justificativa, presenca_externa) values (@id_cf_sessao, @id_cf_deputado, @presente, @justificativa, @presenca_externa);");
                    }
                }
            }
        }

        Padrao.DeputadoFederalPresencaUltimaAtualizacao = DateTime.Now;

        using (var banco = new AppDb())
        {
            banco.AddParameter("cf_deputado_presenca_ultima_atualizacao", Padrao.DeputadoFederalPresencaUltimaAtualizacao);
            banco.ExecuteNonQuery(@"UPDATE parametros SET cf_deputado_presenca_ultima_atualizacao=@cf_deputado_presenca_ultima_atualizacao");

            banco.ExecuteNonQuery(@"	
UPDATE cf_sessao s
LEFT JOIN (
	SELECT
		id_cf_sessao
		, sum(IF(sp.presente = 1, 1, 0)) as presenca
		, sum(IF(sp.presente = 0 and sp.justificativa = '', 1, 0)) as ausencia
		, sum(IF(sp.presente = 0 and sp.justificativa <> '', 1, 0)) as ausencia_justificada
	FROM cf_sessao_presenca sp 
	GROUP BY id_cf_sessao
) sp on sp.id_cf_sessao = s.id
SET 
    s.presencas = IFNULL(sp.presenca, 0),
    s.ausencias = IFNULL(sp.ausencia, 0),
    s.ausencias_justificadas = IFNULL(sp.ausencia_justificada, 0)
WHERE presencas = 0");
        }

        //if (presencas_importadas > 0)
        //{
        //    using (var banco = new Banco())
        //    {
        //        for (int ano = ano_inicio; ano <= dtPesquisa.Year; ano++)
        //        {
        //            banco.AddParameter("ano", ano);
        //            var total_sessoes = Convert.ToInt32(banco.ExecuteScalar(@"SELECT COUNT(1) FROM cf_sessao WHERE year(data)=@ano"));

        //            banco.AddParameter("ano", ano);
        //            banco.AddParameter("total_sessoes", total_sessoes);
        //            banco.ExecuteNonQuery(@"UPDATE cf_legislatura SET total_sessoes=@total_sessoes WHERE ano=@ano");


        //            banco.ExecuteNonQuery(@"UPDATE cf_legislatura SET total_sessoes=@total_sessoes WHERE ano=@ano");
        //        }
        //    }
        //}
        return sb.ToString();
    }

    /// <summary>
    /// Baixa as imagens dos deputados novos (imagens que ainda não foram baixadas)
    /// </summary>
    /// <param name="dirRaiz"></param>
    public async void DownloadFotosParlamentares()
    {
        var sDeputadosImagesPath = System.IO.Path.Combine(rootPath, "public/img/depfederal/");

        var sb = new StringBuilder();
        using (var banco = new AppDb())
        {
            string sql = "SELECT id FROM cf_deputado where id_deputado is not null AND situacao = 'Exercício'";
            DataTable table = banco.GetTable(sql, 0);

            foreach (DataRow row in table.Rows)
            {
                string id = row["id"].ToString();
                string src = sDeputadosImagesPath + id + ".jpg";
                if (File.Exists(src)) continue;

                try
                {
                    using (HttpClient client = new())
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd(Utils.DefaultUserAgent);
                        try
                        {
                            await client.DownloadFile("https://www.camara.leg.br/internet/deputado/bandep/" + id + ".jpgmaior.jpg", src);
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Atualizado imagem do deputado 2 ---------- " + id);

                            await client.DownloadFile("http://www.camara.gov.br/internet/deputado/bandep/" + id + ".jpg", src);
                        }

                        ImportacaoUtils.CreateImageThumbnail(src);
                        ImportacaoUtils.CreateImageThumbnail(src, 240, 300);
                    }

                    Console.WriteLine("Atualizado imagem do deputado " + id);
                }
                catch (Exception ex)
                {
                    sb.AppendLine("Imagem do deputado " + id + " inexistente! Detalhes:" + ex.ToFullDescriptionString());
                    Console.WriteLine("Imagem do deputado " + id + " inexistente! Detalhes:" + ex.ToFullDescriptionString());
                }
            }
        }

        //return sb.ToString();
    }

    #endregion Importação Deputados

    #region Importação Dados CEAP CSV

    public void Importar(int ano)
    {
        logger.LogWarning($"Despesas do(a) Camara Federal de {ano}");

        Dictionary<string, string> arquivos = DefinirUrlOrigemCaminhoDestino(ano);

        foreach (var arquivo in arquivos)
        {
            var _urlOrigem = arquivo.Key;
            var caminhoArquivo = arquivo.Value;

            if (TentarBaixarArquivo(_urlOrigem, caminhoArquivo))
            {
                try
                {
                    if (caminhoArquivo.EndsWith(".zip"))
                    {
                        DescompactarArquivo(caminhoArquivo);
                        caminhoArquivo = caminhoArquivo.Replace(".zip", "");
                    }

                    ImportarDespesas(caminhoArquivo, ano);
                }
                catch (Exception ex)
                {

                    logger.LogError(ex, ex.Message);

#if !DEBUG
                        //Excluir o arquivo para tentar importar novamente na proxima execução
                        if(File.Exists(_caminhoArquivo))
                            File.Delete(_caminhoArquivo);
#endif

                }
            }
        }
    }

    protected void DescompactarArquivo(string caminhoArquivo)
    {
        logger.LogTrace("Descompactando Arquivo '{CaminhoArquivo}'", caminhoArquivo);

        var zip = new FastZip();
        zip.ExtractZip(caminhoArquivo, Path.GetDirectoryName(caminhoArquivo), null);
    }

    /// <summary>
    /// Baixa e Importa os Dados da CEAP
    /// </summary>
    /// <param name="atualDir"></param>
    /// <param name="ano"></param>
    /// <param name="completo"></param>
    public Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
    {
        var downloadUrl = "http://www.camara.leg.br/cotas/Ano-" + ano + ".csv.zip";
        var fullFileNameZip = tempPath + "/CF/Ano-" + ano + ".csv.zip";

        Dictionary<string, string> arquivos = new();
        arquivos.Add(downloadUrl, fullFileNameZip);

        return arquivos;
    }

    protected bool TentarBaixarArquivo(string urlOrigem, string caminhoArquivo)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            return BaixarArquivo(urlOrigem, caminhoArquivo);
        }
        catch (Exception ex)
        {
            logger.LogWarning($"Erro ao baixar arquivo: {ex.Message}");

            // Algumas vezes ocorre do arquivo não estar disponivel, precisamos aguardar alguns instantes e tentar novamente.
            // Isso pode ser causado por um erro de rede ou atualização do arquivo.
            Thread.Sleep((int)TimeSpan.FromMinutes(1).TotalMilliseconds);

            return BaixarArquivo(urlOrigem, caminhoArquivo);
        }
        finally
        {
            watch.Stop();
            logger.LogTrace($"Arquivo baixado em {watch.Elapsed.TotalSeconds} s");
        }
    }

    private bool BaixarArquivo(string urlOrigem, string caminhoArquivo)
    {
        logger.LogTrace($"Baixando arquivo '{caminhoArquivo}' a partir de '{urlOrigem}'");

        string diretorio = new FileInfo(caminhoArquivo).Directory.ToString();
        if (!Directory.Exists(diretorio))
        {
            Directory.CreateDirectory(diretorio);

            if (File.Exists(caminhoArquivo))
            {
                File.Delete(caminhoArquivo);
            }

            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd(Utils.DefaultUserAgent);
                client.Timeout = TimeSpan.FromMinutes(5);
                client.DownloadFile(urlOrigem, caminhoArquivo).Wait();
            }

            return true;
        }

        return false;
    }

    private readonly string[] ColunasCEAP = {
        "txNomeParlamentar","cpf","ideCadastro","nuCarteiraParlamentar","nuLegislatura","sgUF","sgPartido","codLegislatura",
        "numSubCota","txtDescricao","numEspecificacaoSubCota","txtDescricaoEspecificacao","txtFornecedor","txtCNPJCPF",
        "txtNumero","indTipoDocumento","datEmissao","vlrDocumento","vlrGlosa","vlrLiquido","numMes","numAno","numParcela",
        "txtPassageiro","txtTrecho","numLote","numRessarcimento","datPagamentoRestituicao", "vlrRestituicao","nuDeputadoId","ideDocumento","urlDocumento"
    };

    public void ImportarDespesas(string file, int ano)
    {
        var totalColunas = ColunasCEAP.Length;
        int lote = 0, linhaInserida = 0;
        var dc = new Dictionary<string, UInt64>();

        using (var banco = new AppDb())
        {
            using (var dReader = banco.ExecuteReader($"select id, hash from cf_despesa where ano={ano} and hash IS NOT NULL"))
                while (dReader.Read())
                    dc.Add(Convert.ToHexString((byte[])dReader["hash"]), (UInt64)dReader["id"]);

            var cultureInfo = new CultureInfo("en-US");

            using (var reader = new StreamReader(file, Encoding.GetEncoding("UTF-8")))
            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")))
            {
                int linha = 0;
                while (csv.Read())
                {
                    if (++linha == 1)
                    {
                        for (int i = 0; i < totalColunas - 1; i++)
                        {
                            if (csv[i] != ColunasCEAP[i])
                            {
                                throw new Exception("Mudança de integração detectada para o Câmara Federal");
                            }
                        }

                        // Pular linha de titulo
                        continue;
                    }

                    for (int i = 0; i < totalColunas; i++)
                    {
                        string coluna = ColunasCEAP[i];
                        dynamic valor = csv[i];

                        if (!string.IsNullOrEmpty(valor))
                        {
                            if (coluna == "dataEmissao")
                            {
                                valor = Convert.ToDateTime(valor).ToString("yyyy-MM-dd");
                            }
                            else if (coluna.StartsWith("vlr") || coluna.StartsWith("num"))
                            {
                                valor = Convert.ToDecimal(valor, cultureInfo);
                            }
                            else if (coluna.Contains("cpf", StringComparison.InvariantCultureIgnoreCase))
                                valor = new System.Text.RegularExpressions.Regex(@"[^\d]").Replace(valor.Trim(), "");
                            else if (coluna == "urlDocumento")
                            {
                                if (valor.Contains("/documentos/publ"))
                                    valor = "1"; // Ex: https://www.camara.leg.br/cota-parlamentar/documentos/publ/3453/2022/7342370.pdf
                                else if (valor.Contains("/nota-fiscal-eletronica"))
                                    valor = "2"; // Ex: https://www.camara.leg.br/cota-parlamentar/nota-fiscal-eletronica?ideDocumentoFiscal=7321395
                                else
                                {
                                    logger.LogError($"Documento '{valor}' não reconhecido!");
                                    valor = "3";
                                }

                            }
                            else if (coluna == "passageiro")
                            {
                                if (valor.ToString().Contains(";"))
                                {
                                    logger.LogInformation($"{valor} > {valor.ToString().Split(";")[0]}");
                                }

                                valor = valor.ToString().Split(";")[0];
                                string[] partes = valor.ToString().Split(new[] { '/', ';' });
                                if (partes.Length > 1)
                                {
                                    var antes = valor;
                                    valor = "";
                                    for (int y = partes.Length - 1; y >= 0; y--)
                                    {
                                        valor += " " + partes[y];
                                    }

                                    logger.LogInformation($"{antes} > {valor}");
                                }
                            }
                            else
                            {
                                valor = valor.ToString().Trim();
                            }
                        }
                        else
                        {
                            valor = null;
                        }

                        banco.AddParameter(coluna, valor);
                    }

                    byte[] hash = banco.ParametersHash();
                    var key = Convert.ToHexString(hash);
                    if (dc.Remove(key))
                    {
                        banco.ClearParameters();
                        continue;
                    }

                    banco.AddParameter("hash", hash);

                    banco.ExecuteNonQuery(@"
INSERT INTO ops_tmp.cf_despesa_temp (
    nomeParlamentar, cpf, idDeputado, numeroCarteiraParlamentar, legislatura, siglaUF, 
    siglaPartido, codigoLegislatura, numeroSubCota, descricao, numeroEspecificacaoSubCota, 
    descricaoEspecificacao, fornecedor, cnpjCPF, numero, tipoDocumento, 
    dataEmissao, valorDocumento, valorGlosa, valorLiquido, mes, 
    ano, parcela, passageiro, trecho, lote, 
    ressarcimento, restituicao, datPagamentoRestituicao, numeroDeputadoID, idDocumento, urlDocumento, hash
) VALUES (
    @txNomeParlamentar, @cpf, @ideCadastro, @nuCarteiraParlamentar, @nuLegislatura, @sgUF, 
    @sgPartido, @codLegislatura, @numSubCota, @txtDescricao, @numEspecificacaoSubCota, 
    @txtDescricaoEspecificacao, @txtFornecedor, @txtCNPJCPF, @txtNumero, @indTipoDocumento, 
    @datEmissao, @vlrDocumento, @vlrGlosa, @vlrLiquido, @numMes, 
    @numAno, @numParcela, @txtPassageiro, @txtTrecho, @numLote, 
    @numRessarcimento, @vlrRestituicao, @datPagamentoRestituicao, @nuDeputadoId, @ideDocumento, @urlDocumento, @hash
)");
                    if (linhaInserida++ == 10000)
                    {
                        logger.LogInformation("Processando lote {Lote}", ++lote);
                        ProcessarDespesasTemp(ano);
                        linhaInserida = 0;
                    }
                }
            }

            if (linhaInserida > 0)
            {
                logger.LogInformation("Processando lote {Lote}", ++lote);
                ProcessarDespesasTemp(ano);
            }

            foreach (var id in dc.Values)
            {
                banco.AddParameter("id", id);
                banco.ExecuteNonQuery("delete from cf_despesa where id=@id");
            }
        }



        if (ano == DateTime.Now.Year && (lote > 0 || dc.Count > 0))
        {
            InsereDespesaLegislatura();

            AtualizaParlamentarValores();
            AtualizaCampeoesGastos();
            AtualizaResumoMensal();

            connection.Execute(@"UPDATE parametros SET cf_deputado_ultima_atualizacao=NOW();");
        }
    }

    private void ProcessarDespesasTemp(int ano)
    {
        ValidaImportacao(ano);

        CorrigeDespesas();
        InsereDeputadoFaltante();
        InserePassageiroFaltante();
        InsereTrechoViagemFaltante();
        //InsereTipoDespesaFaltante();
        //InsereTipoEspecificacaoFaltante();
        InsereMandatoFaltante();
        InsereLegislaturaFaltante();
        InsereFornecedorFaltante();
        InsereDespesaFinal(ano);
        LimpaDespesaTemporaria();
    }

    public virtual void ValidaImportacao(int ano)
    {
        logger.LogTrace("Validar importação");

        var totalFinal = connection.ExecuteScalar<int>(@$"
select count(1) 
from cf_despesa d 
where d.ano_mes between {ano}01 and {ano}12");

        if (linhasProcessadasAno != totalFinal)
            logger.LogError("Totais divergentes! Arquivo: {LinhasArquivo} DB: {LinhasDB}",
                linhasProcessadasAno, totalFinal);

        var despesasSemParlamentar = connection.ExecuteScalar<int>(@$"
select count(1) from ops_tmp.cf_despesa_temp where idDeputado not in (select id from cf_deputado);");

        if (despesasSemParlamentar > 0)
            logger.LogError("Há deputados não identificados!");
    }

    private void CorrigeDespesas()
    {
        connection.Execute(@"
				UPDATE ops_tmp.cf_despesa_temp SET numero = NULL WHERE numero = 'S/N' OR numero = '';
				UPDATE ops_tmp.cf_despesa_temp SET numeroDeputadoID = NULL WHERE numeroDeputadoID = '';
				UPDATE ops_tmp.cf_despesa_temp SET cnpjCPF = NULL WHERE cnpjCPF = '';
				UPDATE ops_tmp.cf_despesa_temp SET fornecedor = 'CORREIOS' WHERE cnpjCPF is null and fornecedor LIKE 'CORREIOS%';

                UPDATE ops_tmp.cf_despesa_temp SET siglaUF = NULL WHERE siglaUF = 'NA';
                UPDATE ops_tmp.cf_despesa_temp SET numeroEspecificacaoSubCota = NULL WHERE numeroEspecificacaoSubCota = 0;
                UPDATE ops_tmp.cf_despesa_temp SET lote = NULL WHERE lote = 0;
                UPDATE ops_tmp.cf_despesa_temp SET ressarcimento = NULL WHERE ressarcimento = 0;
                UPDATE ops_tmp.cf_despesa_temp SET idDocumento = NULL WHERE idDocumento = 0;
                UPDATE ops_tmp.cf_despesa_temp SET parcela = NULL WHERE parcela = 0;
                UPDATE ops_tmp.cf_despesa_temp SET siglaPartido = 'PP' WHERE siglaPartido = 'PP**';
                UPDATE ops_tmp.cf_despesa_temp SET siglaPartido = null WHERE siglaPartido = 'NA';
			");
    }

    public void InsereDeputadoFaltante()
    {
        // Atualiza os deputados já existentes quando efetuarem os primeiros gastos com a cota
        var affected = connection.Execute(@"
                UPDATE cf_deputado d
                inner join ops_tmp.cf_despesa_temp dt on d.id = dt.idDeputado
                set d.id_deputado = dt.numeroDeputadoID
                where d.id_deputado is null;
            ");

        if (affected > 0)
        {
            //AtualizaInfoDeputados();
            //AtualizaInfoDeputadosCompleto();

            logger.LogInformation("{Itens} parlamentares atualizados!", affected);
        }

        // Insere um novo deputado ou liderança
        affected = connection.Execute(@"
        	    INSERT INTO cf_deputado (id, id_deputado, nome_parlamentar, cpf)
        	    SELECT idDeputado, numeroDeputadoID, nomeParlamentar, max(cpf)
        	    FROM ops_tmp.cf_despesa_temp
        	    WHERE numeroDeputadoID  not in (
        		    SELECT id_deputado 
				    FROM cf_deputado
        		    WHERE id_deputado IS NOT null
        	    )
				AND numeroDeputadoID IS NOT null
				GROUP BY idDeputado, numeroDeputadoID, nomeParlamentar;;
            ");

        //DownloadFotosDeputados(@"C:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\DEPFEDERAL\");

        if (affected > 0)
        {
            //AtualizaInfoDeputados();
            //AtualizaInfoDeputadosCompleto();

            logger.LogInformation("{Itens} parlamentares incluidos!", affected);
        }
    }

    private string InserePassageiroFaltante()
    {
        var affected = connection.Execute(@"
        	    INSERT INTO pessoa (nome)
        	    SELECT DISTINCT passageiro
        	    FROM ops_tmp.cf_despesa_temp
        	    WHERE ifnull(passageiro, '') <> ''
                AND passageiro not in (
        		    SELECT nome FROM pessoa
        	    );
            ");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} passageiros incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereTrechoViagemFaltante()
    {
        var affected = connection.Execute(@"
        	    INSERT INTO trecho_viagem (descricao)
        	    SELECT DISTINCT trecho
        	    FROM ops_tmp.cf_despesa_temp
        	    WHERE ifnull(trecho, '') <> ''
                AND trecho not in (
        		    SELECT descricao FROM trecho_viagem
        	    );
            ");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} trechos viagem incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereTipoDespesaFaltante(AppDb banco)
    {
        connection.Execute(@"
        	INSERT INTO cf_despesa_tipo (id, descricao)
        	select distinct numeroSubCota, descricao
        	from ops_tmp.cf_despesa_temp
        	where numeroSubCota  not in (
        		select id from cf_despesa_tipo
        	);
        ");

        if (banco.RowsAffected > 0)
        {
            logger.LogInformation("{Itens} tipos de despesa incluidos!", banco.RowsAffected);
        }

        return string.Empty;
    }

    private string InsereTipoEspecificacaoFaltante()
    {
        var affected = connection.Execute(@"
            	INSERT INTO cf_especificacao_tipo (id_cf_despesa_tipo, id_cf_especificacao, descricao)
            	select distinct numeroSubCota, numeroEspecificacaoSubCota, descricaoEspecificacao
            	from ops_tmp.cf_despesa_temp dt
            	left join cf_especificacao_tipo tp on tp.id_cf_despesa_tipo = dt.numeroSubCota 
            		and tp.id_cf_especificacao = dt.numeroEspecificacaoSubCota
            	where numeroEspecificacaoSubCota <> 0
            	AND tp.descricao = null;
            ");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} tipo especificação incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereMandatoFaltante()
    {
        var affected = connection.Execute(@"
                SET SQL_BIG_SELECTS=1;

				UPDATE cf_mandato m 
                INNER JOIN ( 
	                select distinct 
	                numeroDeputadoID, legislatura, numeroCarteiraParlamentar, codigoLegislatura, siglaUF, siglaPartido
	                from ops_tmp.cf_despesa_temp
                ) dt ON m.id_cf_deputado = dt.numeroDeputadoID 
	                AND m.id_legislatura = dt.codigoLegislatura 
                left join estado e on e.sigla = dt.siglaUF
                left join partido p on p.sigla = dt.siglaPartido
                SET m.id_carteira_parlamantar = numeroCarteiraParlamentar
                where dt.codigoLegislatura <> 0
                AND m.id_carteira_parlamantar IS NULL;

                SET SQL_BIG_SELECTS=0;
			");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} mandatos atualizados!", affected);
        }

        affected = connection.Execute(@"
                SET SQL_BIG_SELECTS=1;

				INSERT INTO cf_mandato (id_cf_deputado, id_legislatura, id_carteira_parlamantar, id_estado, id_partido)
				select distinct dt.numeroDeputadoID, codigoLegislatura, numeroCarteiraParlamentar, e.id, p.id 
				from ( 
					select distinct 
					numeroDeputadoID, legislatura, numeroCarteiraParlamentar, codigoLegislatura, siglaUF, siglaPartido
					from ops_tmp.cf_despesa_temp
				) dt
				left join estado e on e.sigla = dt.siglaUF
				left join partido p on p.sigla = dt.siglaPartido
				left join cf_mandato m on m.id_cf_deputado = dt.numeroDeputadoID 
					AND m.id_legislatura = dt.codigoLegislatura 
					-- AND m.id_carteira_parlamantar = numeroCarteiraParlamentar
				where dt.codigoLegislatura <> 0
				AND m.id is null;

                SET SQL_BIG_SELECTS=0;
			");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} mandatos incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereLegislaturaFaltante()
    {
        var affected = connection.Execute(@"
				INSERT INTO cf_legislatura (id, ano)
				select distinct codigoLegislatura, legislatura
				from ops_tmp.cf_despesa_temp dt
				where codigoLegislatura <> 0
				AND codigoLegislatura  not in (
					select id from cf_legislatura
				);
			");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} legislatura incluidos!", affected);
        }

        return string.Empty;
    }

    public void InsereFornecedorFaltante()
    {
        var affected = connection.Execute(@"
                SET SQL_BIG_SELECTS=1;

				INSERT INTO fornecedor (nome, cnpj_cpf)
				select MAX(dt.fornecedor), dt.cnpjCPF
				from ops_tmp.cf_despesa_temp dt
				left join fornecedor f on f.cnpj_cpf = dt.cnpjCPF
				where dt.cnpjCPF is not null
				and f.id is null
				GROUP BY dt.cnpjCPF;

				INSERT INTO fornecedor (nome, cnpj_cpf)
				select DISTINCT dt.fornecedor, dt.cnpjCPF
				from ops_tmp.cf_despesa_temp dt
				left join fornecedor f on f.nome = dt.fornecedor
				where dt.cnpjCPF is null
				and f.id is null;

                SET SQL_BIG_SELECTS=0;
			");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} fornecedores incluidos!", affected);
        }
    }

    public void InsereDespesaFinal(int ano)
    {
        var affected = connection.Execute(@$"
SET SQL_BIG_SELECTS=1;
ALTER TABLE cf_despesa DISABLE KEYS;

INSERT INTO cf_despesa (
	ano,
	mes,
	id_documento,
	id_cf_deputado,
	id_cf_legislatura,
	id_cf_mandato,
	id_cf_despesa_tipo,
	id_cf_especificacao,
	id_fornecedor,
	id_passageiro,
	numero_documento,
	tipo_documento,
	data_emissao,
	valor_documento,
	valor_glosa,
	valor_liquido,
	valor_restituicao,
	id_trecho_viagem,
    tipo_link,
    hash
)
SELECT 
	dt.ano,
	dt.mes,
	dt.idDocumento,
	d.id,
	dt.codigoLegislatura,
	m.id,
	numeroSubCota,
	numeroEspecificacaoSubCota,
	f.id,
	p.id,
	numero,
	tipoDocumento,
	dataEmissao,
	valorDocumento,
	valorGlosa,
	valorLiquido,
	restituicao,
	tv.id,
    IFNULL(urlDocumento, 0),
    dt.hash
from ops_tmp.cf_despesa_temp dt
LEFT JOIN cf_deputado d on d.id_deputado = dt.numeroDeputadoID
LEFT JOIN pessoa p on p.nome = dt.passageiro
LEFT JOIN trecho_viagem tv on tv.descricao = dt.trecho
LEFT join fornecedor f on f.cnpj_cpf = dt.cnpjCPF
	or (f.cnpj_cpf is null and dt.cnpjCPF is null and f.nome = dt.fornecedor)
left join cf_mandato m on m.id_cf_deputado = d.id
	and m.id_legislatura = dt.codigoLegislatura 
	and m.id_carteira_parlamantar = numeroCarteiraParlamentar;
    
ALTER TABLE cf_despesa ENABLE KEYS;
SET SQL_BIG_SELECTS=0;
			", 3600);

        if (affected > 0)
        {
            logger.LogInformation("{Itens} despesas incluidos!", affected);
        }
    }

    private void InsereDespesaLegislatura()
    {
        var Legislaturas = connection.Query<int>("SELECT DISTINCT codigoLegislatura FROM ops_tmp.cf_despesa_temp");
        if (!Legislaturas.Any())
        {
            Legislaturas = new List<int>() { 55, 56, 57 };
        };

        foreach (var legislatura in Legislaturas)
        {
            connection.Execute(@$"
TRUNCATE TABLE cf_despesa_{legislatura};

INSERT INTO cf_despesa_{legislatura} 
    (id, id_cf_deputado, id_cf_despesa_tipo, id_fornecedor, ano_mes, data_emissao, valor_liquido)
SELECT 
     id, id_cf_deputado, id_cf_despesa_tipo, id_fornecedor, concat(ano, LPAD(mes, 2, '0')), data_emissao, valor_liquido 
FROM cf_despesa
WHERE id_cf_legislatura = {legislatura};
			", 3600);
        }
    }

    public void LimpaDespesaTemporaria()
    {
        connection.Execute(@"truncate table ops_tmp.cf_despesa_temp;");
    }

    #endregion Importação Dados CEAP CSV


    #region Processar Resumo CEAP

    public void AtualizaParlamentarValores()
    {
        var dt = connection.Query("select id from cf_deputado");
        object valor_total_ceap;
        object secretarios_ativos = 0;
        object valor_total_remuneracao;

        foreach (var dr in dt)
        {
            try
            {
                var obj = new { id_cf_deputado = Convert.ToInt32(dr.id) };
                valor_total_ceap = connection.ExecuteScalar("select sum(valor_liquido) from cf_despesa where id_cf_deputado=@id_cf_deputado;", obj);
                if (Convert.IsDBNull(valor_total_ceap)) valor_total_ceap = 0;
                else if (valor_total_ceap == null || (decimal)valor_total_ceap < 0) valor_total_ceap = 0;

                secretarios_ativos = connection.ExecuteScalar("select count(1) from cf_funcionario_contratacao where id_cf_deputado=@id_cf_deputado and periodo_ate is null;", obj);
                valor_total_remuneracao = connection.ExecuteScalar("select sum(valor) from cf_deputado_verba_gabinete where id_cf_deputado=@id_cf_deputado;", obj);

                if (Convert.IsDBNull(valor_total_remuneracao) || Convert.ToDecimal(valor_total_remuneracao) == 0)
                    valor_total_remuneracao = connection.ExecuteScalar("select sum(valor_total) from cf_funcionario_remuneracao where id_cf_deputado=@id_cf_deputado;", obj);

                connection.Execute(@"
update cf_deputado set 
    valor_total_ceap = @valor_total_ceap,
    secretarios_ativos = @secretarios_ativos,
    valor_total_remuneracao = @valor_total_remuneracao
where id=@id_cf_deputado", new
                {
                    valor_total_ceap = valor_total_ceap,
                    secretarios_ativos = secretarios_ativos,
                    valor_total_remuneracao = valor_total_remuneracao,
                    id_cf_deputado = dr.id
                }
                );
            }
            catch (Exception ex)
            {
                if (!ex.GetBaseException().Message.Contains("Out of range"))
                    throw;
            }
        }
    }

    /// <summary>
    /// Atualiza indicador 'Campeões de gastos',
    /// Os 4 deputados que mais gastaram com a CEAP desde o ínicio do mandato 55 (02/2015)
    /// </summary>
    public void AtualizaCampeoesGastos()
    {
        var strSql =
            @"truncate table cf_deputado_campeao_gasto;
				insert into cf_deputado_campeao_gasto
				SELECT l1.id_cf_deputado, d.nome_parlamentar, l1.valor_total, p.sigla, e.sigla
				FROM (
					SELECT 
						l.id_cf_deputado,
						sum(l.valor_liquido) as valor_total
					FROM  cf_despesa_57 l
					GROUP BY l.id_cf_deputado
					order by valor_total desc 
					limit 4
				) l1 
				INNER JOIN cf_deputado d on d.id = l1.id_cf_deputado 
				LEFT JOIN partido p on p.id = d.id_partido
				LEFT JOIN estado e on e.id = d.id_estado;";

        connection.Execute(strSql);
    }

    public void AtualizaResumoMensal()
    {
        var strSql =
            @"truncate table cf_despesa_resumo_mensal;
				insert into cf_despesa_resumo_mensal
				(ano, mes, valor) (
					select ano, mes, sum(valor_liquido)
					from cf_despesa
					group by ano, mes
				);";

        connection.Execute(strSql);
    }

    #endregion Processar Resumo CEAP


    #region Importação Remuneração
    public void ConsultaRemuneracao(int ano, int mes)
    {
        var meses = new string[] { "janeiro", "fevereiro", "marco", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro" };

        string urlOrigem;
        if (ano == 2012)
            urlOrigem = string.Format("https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/{0}/{1}-{0}-csv.csv", ano, meses[mes - 1]);
        else if (ano == 2013 && mes == 2)
            urlOrigem = "https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/2013/fevereiro-de-2013-1";
        else if (ano == 2013 && mes == 3)
            urlOrigem = "https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/2013/Marco-2013-csv";
        else if (ano >= 2013 && mes == 5)
            urlOrigem = "https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/2013/RemuneracaoMensalServidores052013.csv";
        else if (ano == 2016 && mes == 7)
            urlOrigem = "https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/2016/copy_of_RemuneracaoMensalServidores072016.csv";
        else if (ano == 2015)
            urlOrigem = string.Format("https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/copy_of_2014/{1}-de-{0}-csv", ano, meses[mes - 1]);
        else if (ano > 2013 || (ano == 2013 && (mes >= 7 || mes == 4)))
            urlOrigem = string.Format("https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/{0}/{1}-de-{0}-csv", ano, meses[mes - 1]);
        else if (ano == 2022)
            urlOrigem = string.Format("https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/copy_of_2021/{1}-{0}-csv", ano, meses[mes - 1]);
        else
            urlOrigem = string.Format("https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/{0}/{1}-{0}-csv", ano, meses[mes - 1]);

        var caminhoArquivo = System.IO.Path.Combine(tempPath, $"CF-RM{ano}{mes:00}.csv");

        try
        {
            var possuiNovosItens = TentarBaixarArquivo(urlOrigem, caminhoArquivo);

            if (possuiNovosItens)
                CarregaRemuneracaoCsv(caminhoArquivo, Convert.ToInt32(ano.ToString() + mes.ToString("00")));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            if (File.Exists(caminhoArquivo))
                File.Delete(caminhoArquivo);
        }
    }

    private string CarregaRemuneracaoCsv(string file, int anomes)
    {
        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
        var sb = new StringBuilder();
        string sResumoValores = string.Empty;

        int indice = 0;
        int CargoIndividualizadodoServidor = indice++;
        int GrupoFuncional = indice++;
        int FolhadePagamento = indice++;
        int AnoIngresso = indice++;
        int RemuneraçãoFixa = indice++;
        int VantagensdeNaturezaPessoal = indice++;
        int FunçãoouCargoemComissão = indice++;
        int GratificaçãoNatalina = indice++;
        int Ferias = indice++;
        int OutrasRemuneraçõesEventuaisProvisórias = indice++;
        int AbonodePermanência = indice++;
        int RedutorConstitucional = indice++;
        int ConstribuiçãoPrevidenciária = indice++;
        int ImpostodeRenda = indice++;
        int RemuneraçãoApósDescontosObrigatórios = indice++;
        int Diárias = indice++;
        int Auxílios = indice++;
        int VantagensIndenizatórias = indice++;

        //int linhaAtual = 0;

        using (var banco = new AppDb())
        {
            //var lstHash = new List<string>();
            //using (var dReader = banco.ExecuteReader("select hash from sf_despesa where ano=" + ano))
            //{
            //    while (dReader.Read())
            //    {
            //        try
            //        {
            //            lstHash.Add(dReader["hash"].ToString());
            //        }
            //        catch (Exception)
            //        {
            //            // Vai ter duplicado mesmo
            //        }
            //    }
            //}

            //using (var dReader = banco.ExecuteReader("select sum(valor) as valor, count(1) as itens from sf_despesa where ano=" + ano))
            //{
            //    if (dReader.Read())
            //    {
            //        sResumoValores = string.Format("[{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
            //    }
            //}

            LimpaRemuneracaoTemporaria(banco);

            using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
            {
                short count = 0;

                using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")))
                {
                    while (csv.Read())
                    {
                        count++;
                        if (count == 1)
                        {
                            if (
                                    (csv[CargoIndividualizadodoServidor] != "Cargo Individualizado do Servidor") ||
                                    (csv[VantagensIndenizatórias].Trim() != "Vantagens Indenizatórias")
                                )
                            {
                                throw new Exception("Mudança de integração detectada para o CamaraFederal Federal");
                            }

                            // Pular linha de titulo
                            continue;
                        }

                        var cargo = csv[CargoIndividualizadodoServidor].Split(" ");
                        var id = cargo[cargo.Length - 1];
                        if (cargo[0].Trim() == "(*)")
                        {
                            break;
                        }

                        banco.AddParameter("id", Convert.ToInt32(id));
                        banco.AddParameter("ano_mes", anomes);
                        banco.AddParameter("cargo", csv[CargoIndividualizadodoServidor].Replace(id, "").Trim());
                        banco.AddParameter("grupo_funcional", csv[GrupoFuncional]);
                        banco.AddParameter("tipo_folha", csv[FolhadePagamento]);
                        banco.AddParameter("admissao", csv[AnoIngresso] != "null" && csv[AnoIngresso] != "" ? Convert.ToInt32(csv[AnoIngresso].Replace(",00", "")) : (int?)null);

                        banco.AddParameter("remun_basica", Convert.ToDouble(csv[RemuneraçãoFixa], cultureInfo));
                        banco.AddParameter("vant_pessoais", Convert.ToDouble(csv[VantagensdeNaturezaPessoal], cultureInfo));
                        banco.AddParameter("func_comissionada", Convert.ToDouble(csv[FunçãoouCargoemComissão], cultureInfo));
                        banco.AddParameter("grat_natalina", Convert.ToDouble(csv[GratificaçãoNatalina], cultureInfo));
                        banco.AddParameter("ferias", Convert.ToDouble(csv[Ferias], cultureInfo));
                        banco.AddParameter("outras_eventuais", Convert.ToDouble(csv[OutrasRemuneraçõesEventuaisProvisórias], cultureInfo));
                        banco.AddParameter("abono_permanencia", Convert.ToDouble(csv[AbonodePermanência], cultureInfo));
                        banco.AddParameter("reversao_teto_const", Convert.ToDouble(csv[RedutorConstitucional], cultureInfo));
                        banco.AddParameter("imposto_renda", Convert.ToDouble(csv[ImpostodeRenda], cultureInfo));
                        banco.AddParameter("previdencia", Convert.ToDouble(!string.IsNullOrEmpty(csv[ConstribuiçãoPrevidenciária]) ? csv[ConstribuiçãoPrevidenciária] : "0", cultureInfo));
                        banco.AddParameter("rem_liquida", Convert.ToDouble(!string.IsNullOrEmpty(csv[RemuneraçãoApósDescontosObrigatórios]) ? csv[RemuneraçãoApósDescontosObrigatórios] : "0", cultureInfo));
                        banco.AddParameter("diarias", Convert.ToDouble(!string.IsNullOrEmpty(csv[Diárias]) ? csv[Diárias] : "0", cultureInfo));
                        banco.AddParameter("auxilios", Convert.ToDouble(!string.IsNullOrEmpty(csv[Auxílios]) ? csv[Auxílios] : "0", cultureInfo));
                        banco.AddParameter("vant_indenizatorias", Convert.ToDouble(!string.IsNullOrEmpty(csv[VantagensIndenizatórias]) ? csv[VantagensIndenizatórias] : "0".Trim(), cultureInfo));


                        //string hash = banco.ParametersHash();
                        //if (lstHash.Remove(hash))
                        //{
                        //    banco.ClearParameters();
                        //    continue;
                        //}

                        //banco.AddParameter("hash", hash);

                        banco.ExecuteNonQuery(
                            @"INSERT INTO cf_remuneracao_temp (
								id, ano_mes, cargo,grupo_funcional,tipo_folha,admissao,
                                remun_basica ,vant_pessoais ,func_comissionada ,grat_natalina ,ferias ,outras_eventuais ,abono_permanencia ,
                                reversao_teto_const ,imposto_renda ,previdencia ,rem_liquida ,diarias ,auxilios ,vant_indenizatorias
							) VALUES (
								@id, @ano_mes,@cargo,@grupo_funcional,@tipo_folha,@admissao,
                                @remun_basica ,@vant_pessoais ,@func_comissionada ,@grat_natalina ,@ferias ,@outras_eventuais ,@abono_permanencia ,
                                @reversao_teto_const ,@imposto_renda ,@previdencia ,@rem_liquida ,@diarias ,@auxilios ,@vant_indenizatorias
							)");
                    }
                }

                //if (++linhaAtual % 100 == 0)
                //{
                //    Console.WriteLine(linhaAtual);
                //}
            }

            //if (lstHash.Count == 0 && linhaAtual == 0)
            //{
            //    sb.AppendFormat("<p>Não há novos itens para importar! #2</p>");
            //    return sb.ToString();
            //}

            //if (lstHash.Count > 0)
            //{
            //    foreach (var hash in lstHash)
            //    {
            //        banco.ExecuteNonQuery(string.Format("delete from sf_despesa where hash = '{0}'", hash));
            //    }


            //    Console.WriteLine("Registros para exluir: " + lstHash.Count);
            //    sb.AppendFormat("<p>{0} registros excluidos</p>", lstHash.Count);
            //}

            //sb.Append(ProcessarDespesasTemp(banco, completo));
        }

        //if (ano == DateTime.Now.Year)
        //{
        //    AtualizaCamaraFederalrValores();
        //    AtualizaCampeoesGastos();
        //    AtualizaResumoMensal();
        //}

        //using (var banco = new AppDb())
        //{
        //    using (var dReader = banco.ExecuteReader("select sum(valor) as valor, count(1) as itens from sf_despesa where ano=" + ano))
        //    {
        //        if (dReader.Read())
        //        {
        //            sResumoValores += string.Format(" -> [{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
        //        }
        //    }

        //    sb.AppendFormat("<p>Resumo atualização: {0}</p>", sResumoValores);
        //}

        return sb.ToString();
    }

    private void LimpaRemuneracaoTemporaria(AppDb banco)
    {
        banco.ExecuteNonQuery(@"
				truncate table cf_remuneracao_temp;
			");
    }

    public void ColetaDadosDeputados()
    {
        //            var sqlDeputados = @"
        //SELECT DISTINCT cd.id as id_cf_deputado
        //FROM cf_deputado cd 
        //JOIN cf_funcionario_contratacao c ON c.id_cf_deputado = cd.id AND c.periodo_ate IS NULL
        //-- WHERE id_legislatura = 56                                 
        //-- and cd.id >= 141428
        //and cd.processado = 0
        //order by cd.id
        //";

        var sqlDeputados = @"
SELECT DISTINCT cd.id as id_cf_deputado, cd.situacao
FROM cf_deputado cd 
JOIN cf_mandato m ON m.id_cf_deputado = cd.id
-- WHERE id_legislatura = 56                                 
-- and cd.id >= 141428
-- and cd.processado = 0
order by cd.situacao, cd.id
";

        ConcurrentQueue<Dictionary<string, object>> queue;
        using (var db = new AppDb())
        {
            db.ExecuteNonQuery("update cf_deputado set processado = 0;");

            var dcDeputado = db.ExecuteDict(sqlDeputados);
            queue = new ConcurrentQueue<Dictionary<string, object>>(dcDeputado);
        }

        int totalRecords = 20;
        Task[] tasks = new Task[totalRecords];
        for (int i = 0; i < totalRecords; ++i)
        {
            int j = i;
            tasks[j] = Task.Factory.StartNew(() =>
            {
                ProcessaFilaColetaDadosDeputados(queue).Wait();
            });
        }

        Task.WaitAll(tasks);
    }


    private readonly string[] meses = new string[] { "JAN", "FEV", "MAR", "ABR", "MAI", "JUN", "JUL", "AGO", "SET", "OUT", "NOV", "DEZ" };

    private async Task ProcessaFilaColetaDadosDeputados(ConcurrentQueue<Dictionary<string, object>> queue)
    {
        var cultureInfoBR = CultureInfo.CreateSpecificCulture("pt-BR");
        var cultureInfoUS = CultureInfo.CreateSpecificCulture("en-US");

        Dictionary<string, object> deputado = null;
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);

        var sqlInsertImovelFuncional = "insert ignore into cf_deputado_imovel_funcional (id_cf_deputado, uso_de, uso_ate, total_dias) values (@id_cf_deputado, @uso_de, @uso_ate, @total_dias)";

        using (var db = new AppDb())
        {
            while (queue.TryDequeue(out deputado))
            {
                var idDeputado = Convert.ToInt32(deputado["id_cf_deputado"]);
                Console.WriteLine($"Deputado: {idDeputado}");

                var address = $"https://www.camara.leg.br/deputados/{idDeputado}";
                var document = await context.OpenAsync(address);
                if (document.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"{address} {document.StatusCode}");
                };

                bool possuiPassaporteDiplimatico = false;
                int qtdSecretarios = 0;

                var anosmandatos = document.QuerySelectorAll(".linha-tempo li").Select(x => Convert.ToInt32(x.TextContent.Trim()));
                if (!anosmandatos.Any()) continue;

                foreach (var ano in anosmandatos)
                {
                    //if (ano != 2021 && ano != 2022) continue;

                    Console.WriteLine();
                    Console.WriteLine($"Ano: {ano}");
                    address = $"https://www.camara.leg.br/deputados/{idDeputado}?ano={ano}";
                    document = await context.OpenAsync(address);
                    if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
                    {
                        Console.WriteLine(document.StatusCode + ":" + document.Url);
                        continue;
                    }

                    var lstInfo = document.QuerySelectorAll(".recursos-beneficios-deputado-container .beneficio");
                    if (!lstInfo.Any())
                    {
                        logger.LogError("Nenhuma informação de beneficio: {address}", address);
                        continue;
                    }


                    var verbaGabineteMensal = document.QuerySelectorAll("#gastomensalverbagabinete tbody tr");
                    foreach (var item in verbaGabineteMensal)
                    {
                        var colunas = item.QuerySelectorAll("td");

                        var mes = Array.IndexOf(meses, colunas[0].TextContent) + 1;
                        var valor = Convert.ToDecimal(colunas[1].TextContent, cultureInfoBR);
                        var percentual = Convert.ToDecimal(colunas[2].TextContent.Replace("%", ""), cultureInfoUS);

                        db.AddParameter("@id_cf_deputado", idDeputado);
                        db.AddParameter("@ano", ano);
                        db.AddParameter("@mes", mes);
                        db.AddParameter("@valor", valor);
                        db.AddParameter("@percentual", percentual);
                        db.ExecuteNonQuery("insert into cf_deputado_verba_gabinete (id_cf_deputado, ano, mes, valor, percentual) values (@id_cf_deputado, @ano, @mes, @valor, @percentual);");
                    }

                    var cotaParlamentarMensal = document.QuerySelectorAll("#gastomensalcotaparlamentar tbody tr");
                    foreach (var item in cotaParlamentarMensal)
                    {
                        var colunas = item.QuerySelectorAll("td");

                        var mes = Array.IndexOf(meses, colunas[0].TextContent) + 1;
                        var valor = Convert.ToDecimal(colunas[1].TextContent, cultureInfoBR);
                        decimal? percentual = null;
                        if (!string.IsNullOrEmpty(colunas[2].TextContent.Replace("%", "")))
                            percentual = Convert.ToDecimal(colunas[2].TextContent.Replace("%", ""), cultureInfoUS);

                        db.AddParameter("@id_cf_deputado", idDeputado);
                        db.AddParameter("@ano", ano);
                        db.AddParameter("@mes", mes);
                        db.AddParameter("@valor", valor);
                        db.AddParameter("@percentual", percentual);
                        db.ExecuteNonQuery("insert into cf_deputado_cota_parlamentar (id_cf_deputado, ano, mes, valor, percentual) values (@id_cf_deputado, @ano, @mes, @valor, @percentual);");
                    }

                    foreach (var info in lstInfo)
                    {
                        try
                        {
                            var titulo = info.QuerySelector(".beneficio__titulo")?.TextContent?.Replace(" ?", "")?.Trim();
                            var valor = info.QuerySelector(".beneficio__info")?.TextContent?.Trim(); //Replace \n

                            switch (titulo)
                            {
                                case "Pessoal de gabinete":
                                    if (valor != "0 pessoas")
                                    {
                                        qtdSecretarios = Convert.ToInt32(valor.Split(' ')[0]);

                                        await ColetaPessoalGabinete(db, context, idDeputado, ano);
                                    }
                                    break;
                                case "Salário mensal bruto":
                                    // Nada
                                    break;
                                case "Imóvel funcional":
                                    if (!valor.StartsWith("Não fez") && !valor.StartsWith("Não faz"))
                                    {
                                        var lstImovelFuncional = valor.Split("\n");
                                        foreach (var item in lstImovelFuncional)
                                        {
                                            var periodos = item.Trim().Split(" ");

                                            var dataInicial = DateTime.Parse(periodos[3]);
                                            DateTime? dataFinal = null;
                                            int? dias = null;

                                            if (!item.Contains("desde"))
                                            {
                                                dataFinal = DateTime.Parse(periodos[5]);
                                                dias = (int)dataFinal.Value.Subtract(dataInicial).TotalDays;
                                            }

                                            db.AddParameter("@id_cf_deputado", idDeputado);
                                            db.AddParameter("@uso_de", dataInicial);
                                            db.AddParameter("@uso_ate", dataFinal);
                                            db.AddParameter("@total_dias", dias);
                                            db.ExecuteNonQuery(sqlInsertImovelFuncional);
                                        }
                                    }
                                    break;
                                case "Auxílio-moradia":
                                    if (valor != "Não recebe o auxílio")
                                    {
                                        await ColetaAuxilioMoradia(db, context, idDeputado, ano);
                                    }
                                    break;
                                case "Viagens em missão oficial":
                                    if (valor != "0")
                                    {
                                        await ColetaMissaoOficial(db, context, idDeputado, ano);
                                    }
                                    break;
                                case "Passaporte diplomático":
                                    if (valor != "Não possui")
                                    {
                                        possuiPassaporteDiplimatico = true;
                                    }
                                    break;
                                default:
                                    throw new NotImplementedException($"Vefificar beneficios do deputado {idDeputado} para o ano {ano}.");
                            }

                            // Console.WriteLine($"{titulo}: {valor}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

                db.AddParameter("@id_cf_deputado", idDeputado);
                db.AddParameter("@passaporte", possuiPassaporteDiplimatico);
                db.AddParameter("@secretarios_ativos", qtdSecretarios);
                db.ExecuteNonQuery("update cf_deputado set passaporte_diplomatico = @passaporte, secretarios_ativos = @secretarios_ativos, processado = 1 where id = @id_cf_deputado");
            }
        }
    }

    private async Task ColetaPessoalGabinete(AppDb db, IBrowsingContext context, int idDeputado, int ano)
    {
        var address = $"https://www.camara.leg.br/deputados/{idDeputado}/pessoal-gabinete?ano={ano}";
        var document = await context.OpenAsync(address);
        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
        {
            Console.WriteLine(document.StatusCode + ":" + document.Url);
            return;
        };

        var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
        foreach (var tabela in tabelas)
        {
            var linhas = tabela.QuerySelectorAll("tbody tr");
            foreach (var linha in linhas)
            {
                int idSecretario;
                var colunas = linha.QuerySelectorAll("td");
                var nome = colunas[0].TextContent.Trim();
                var grupo = colunas[1].TextContent.Trim();
                var cargo = colunas[2].TextContent.Trim();
                var periodo = colunas[3].TextContent.Trim();
                var link = (colunas[4].FirstElementChild as IHtmlAnchorElement).Href; // todo?
                var chave = link.Replace("https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/", "");

                var periodoPartes = periodo.Split(" ");
                DateTime? dataInicial = Convert.ToDateTime(periodoPartes[1]);
                DateTime? dataFinal = periodoPartes.Length == 4 ? Convert.ToDateTime(periodoPartes[3]) : null;

                var sqlSelectSecretario = "select id from cf_funcionario where chave = @chave";
                db.AddParameter("@chave", chave);
                var objId = db.ExecuteScalar(sqlSelectSecretario);
                if (objId is not null)
                {
                    idSecretario = Convert.ToInt32(objId);
                }
                else
                {
                    var sqlInsertSecretario = "insert into cf_funcionario (chave, nome, processado) values (@chave, @nome, 0);";
                    db.AddParameter("@chave", chave);
                    db.AddParameter("@nome", nome);
                    db.ExecuteNonQuery(sqlInsertSecretario);

                    idSecretario = Convert.ToInt32(db.ExecuteScalar("select LAST_INSERT_ID()"));
                }

                var sqlInsertSecretarioContratacao = @"
insert IGNORE into cf_funcionario_contratacao (id_cf_funcionario, id_cf_deputado, id_cf_funcionario_grupo_funcional, id_cf_funcionario_cargo, id_cf_funcionario_nivel, periodo_de, periodo_ate) 
values (@id_cf_funcionario, @id_cf_deputado, (select id from cf_funcionario_grupo_funcional where nome like @grupo), (select id from cf_funcionario_cargo where nome like @cargo), (select id from cf_funcionario_nivel where nome like @nivel), @periodo_de, @periodo_ate)
-- ON DUPLICATE KEY UPDATE periodo_ate = @periodo_ate";

                db.AddParameter("@id_cf_funcionario", idSecretario);
                db.AddParameter("@id_cf_deputado", idDeputado);
                db.AddParameter("@grupo", grupo); // Secretário Parlamentar
                db.AddParameter("@cargo", grupo); // Secretário Parlamentar
                db.AddParameter("@nivel", cargo);
                db.AddParameter("@periodo_de", dataInicial);
                db.AddParameter("@periodo_ate", dataFinal);
                db.ExecuteNonQuery(sqlInsertSecretarioContratacao);

                //if (objId is not null && (dataFinal == null || dataFinal >= new DateTime(2021, 6, 1)))
                //{
                //    var sqlUpdateSecretario = "UPDATE cf_funcionario set processado = 0 where id = @idSecretario;";
                //    db.AddParameter("@idSecretario", idSecretario);
                //    db.ExecuteNonQuery(sqlUpdateSecretario);
                //}
            }
        }
    }

    private async Task ColetaAuxilioMoradia(AppDb db, IBrowsingContext context, int idDeputado, int ano)
    {
        var sqlInsertAuxilioMoradia = "insert ignore into cf_deputado_auxilio_moradia (id_cf_deputado, ano, mes, valor) values (@id_cf_deputado, @ano, @mes, @valor)";
        var address = $"https://www.camara.leg.br/deputados/{idDeputado}/auxilio-moradia/?ano={ano}";
        var document = await context.OpenAsync(address);
        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
        {
            Console.WriteLine(document.StatusCode + ":" + document.Url);
            return;
        }

        var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
        foreach (var tabela in tabelas)
        {
            var linhas = tabela.QuerySelectorAll("tbody tr");
            foreach (var linha in linhas)
            {
                var colunas = linha.QuerySelectorAll("td");
                var mes = Convert.ToInt32(colunas[0].TextContent.Trim());
                var valor = Convert.ToDecimal(colunas[1].TextContent.Trim());

                db.AddParameter("@id_cf_deputado", idDeputado);
                db.AddParameter("@ano", ano);
                db.AddParameter("@mes", mes);
                db.AddParameter("@valor", valor);
                db.ExecuteNonQuery(sqlInsertAuxilioMoradia);
            }
        }
    }

    private async Task ColetaMissaoOficial(AppDb db, IBrowsingContext context, int idDeputado, int ano)
    {
        var sqlInsertAuxilioMoradia =
            "insert ignore into cf_deputado_missao_oficial (id_cf_deputado, periodo, assunto, destino, passagens, diarias, relatorio) values (@id_cf_deputado, @periodo, @assunto, @destino, @passagens, @diarias, @relatorio)";
        var address = $"https://www.camara.leg.br/deputados/{idDeputado}/missao-oficial/?ano={ano}";
        var document = await context.OpenAsync(address);
        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
        {
            Console.WriteLine(document.StatusCode + ":" + document.Url);
            return;
        }

        var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
        foreach (var tabela in tabelas)
        {
            var linhas = tabela.QuerySelectorAll("tbody tr");
            foreach (var linha in linhas)
            {
                var colunas = linha.QuerySelectorAll("td");
                var periodo = colunas[0].TextContent.Trim();
                var assunto = colunas[1].TextContent.Trim();
                var destino = colunas[2].TextContent.Trim();
                var passagens = Convert.ToDecimal(colunas[3].TextContent.Trim());
                var diarias = Convert.ToDecimal(colunas[4].TextContent.Trim());
                var relatorio = (colunas[5].FirstElementChild as IHtmlAnchorElement)?.Href;

                db.AddParameter("@id_cf_deputado", idDeputado);
                db.AddParameter("@periodo", periodo);
                db.AddParameter("@assunto", assunto);
                db.AddParameter("@destino", destino);
                db.AddParameter("@passagens", passagens);
                db.AddParameter("@diarias", diarias);
                db.AddParameter("@relatorio", relatorio);
                db.ExecuteNonQuery(sqlInsertAuxilioMoradia);
            }
        }
    }

    public void ColetaRemuneracaoSecretarios()
    {
        var sqlDeputados = @"
SELECT DISTINCT c.id as id_cf_funcionario_contratacao, c.id_cf_funcionario, c.id_cf_deputado, f.chave 
FROM cf_funcionario_contratacao c
JOIN cf_funcionario f ON f.id = c.id_cf_funcionario
WHERE f.processado = 0
order BY c.id_cf_funcionario
";

        ConcurrentQueue<Dictionary<string, object>> queue;
        using (var db = new AppDb())
        {
            var dcDeputado = db.ExecuteDict(sqlDeputados);
            queue = new ConcurrentQueue<Dictionary<string, object>>(dcDeputado);
        }

        int totalRecords = 25;
        Task[] tasks = new Task[totalRecords];
        for (int i = 0; i < totalRecords; ++i)
        {
            int j = i;
            tasks[j] = Task.Factory.StartNew(() =>
            {
                ProcessaFilaColetaRemuneracaoSecretarios(queue).Wait();
            });
        }

        Task.WaitAll(tasks);
    }

    private async Task ProcessaFilaColetaRemuneracaoSecretarios(ConcurrentQueue<Dictionary<string, object>> queue)
    {
        Dictionary<string, string> colunasBanco = new Dictionary<string, string>();
        // 1 - Remuneração Básica
        colunasBanco.Add("a - Remuneração Fixa", "remuneracao_fixa");
        colunasBanco.Add("b - Vantagens de Natureza Pessoal", "vantagens_natureza_pessoal");
        // 2 - Remuneração Eventual/Provisória
        colunasBanco.Add("a - Função ou Cargo em Comissão", "funcao_ou_cargo_em_comissao");
        colunasBanco.Add("b - Gratificação Natalina", "gratificacao_natalina");
        colunasBanco.Add("c - Férias (1/3 Constitucional)", "ferias");
        colunasBanco.Add("d - Outras Remunerações Eventuais/Provisórias(*)", "outras_remuneracoes");
        // 3 - Abono Permanência
        colunasBanco.Add("a - Abono Permanência", "abono_permanencia");
        // 4 - Descontos Obrigatórios(-)
        colunasBanco.Add("a - Redutor Constitucional", "redutor_constitucional");
        colunasBanco.Add("b - Contribuição Previdenciária", "contribuicao_previdenciaria");
        colunasBanco.Add("c - Imposto de Renda", "imposto_renda");
        // 5 - Remuneração após Descontos Obrigatórios
        colunasBanco.Add("a - Remuneração após Descontos Obrigatórios", "valor_liquido");
        // 6 - Outros
        colunasBanco.Add("a - Diárias", "valor_diarias");
        colunasBanco.Add("b - Auxílios", "valor_auxilios");
        colunasBanco.Add("c - Vantagens Indenizatórias", "valor_vantagens");

        Dictionary<string, object> secretario = null;
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);

        using (var db = new AppDb())
        {

            while (queue.TryDequeue(out secretario))
            {
                if (secretario is null) continue;

                try
                {
                    var erroColeta = false;
                    var idFuncionario = Convert.ToInt32(secretario["id_cf_funcionario"]);
                    var idFuncionarioContratacao = Convert.ToInt32(secretario["id_cf_funcionario_contratacao"]);
                    var idDeputado = Convert.ToInt32(secretario["id_cf_deputado"]);
                    var chave = secretario["chave"].ToString();
                    //Console.WriteLine($"Secretario: {idFuncionario}");

                    var sqlUltimaRemuneracao = @"SELECT MAX(referencia) FROM cf_funcionario_remuneracao WHERE id_cf_funcionario = @id_cf_funcionario";
                    db.AddParameter("@id_cf_funcionario", idFuncionario);
                    var objUltimaRemuneracaoColetada = db.ExecuteScalar(sqlUltimaRemuneracao);
                    var dtUltimaRemuneracaoColetada = !Convert.IsDBNull(objUltimaRemuneracaoColetada) ? Convert.ToDateTime(objUltimaRemuneracaoColetada) : DateTime.MinValue;

                    var address = $"https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/{chave}";
                    var document = await context.OpenAsync(address);
                    if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
                    {
                        Console.WriteLine(document.StatusCode + ":" + document.Url);
                        continue;
                    }
                    else
                    {
                        int anoSelecionado = 0, mesSelecionado = 0;
                        try
                        {
                            Regex myRegex = new Regex(@"ano=(\d{4})&mes=(\d{1,2})", RegexOptions.Singleline);
                            // window.location.href = 'https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/BDEdGyYrxGdG42MO7egk?ano=2021&mes=7';
                            var linkMatch = myRegex.Match(document.Scripts[1].InnerHtml);
                            if (linkMatch.Length > 0)
                            {
                                anoSelecionado = Convert.ToInt32(linkMatch.Groups[1].Value);
                                mesSelecionado = Convert.ToInt32(linkMatch.Groups[2].Value);
                            }
                            else
                            {
                                // MarcarComoProcessado(db, idFuncionario);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(idFuncionario + ":" + ex.Message);
                            if (document.QuerySelector(".remuneracao-funcionario") != null)
                                Console.WriteLine(document.QuerySelector(".remuneracao-funcionario").TextContent);

                            //MarcarComoProcessado(db, idFuncionario);
                            continue;
                        }

                        document = await context.OpenAsync(address + $"?ano={anoSelecionado}&mes={mesSelecionado}");
                        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
                        {
                            Console.WriteLine(document.StatusCode + ":" + document.Url);
                            continue;
                        }

                        var anos = (document.GetElementById("anoRemuneracao") as IHtmlSelectElement).Options.OrderByDescending(x => Convert.ToInt32(x.Text));
                        if (!anos.Any())
                        {
                            Console.WriteLine("Anos indisponiveis:" + document.Url);
                            continue;
                        }


                        foreach (var ano in anos)
                        {
                            if (ano.OuterHtml.Contains("display: none;")) continue;
                            if (Convert.ToInt32(ano.InnerHtml) < dtUltimaRemuneracaoColetada.Year) continue;

                            //Console.WriteLine();
                            //Console.WriteLine($"Ano: {ano.Text}");
                            if (!ano.IsSelected)
                            {
                                address = $"https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/{ano.Value}";
                                document = await context.OpenAsync(address);

                                if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
                                {
                                    Console.WriteLine(document.StatusCode + ":" + document.Url);
                                    erroColeta = true;
                                    continue;
                                }
                            }

                            var calendario = document.QuerySelectorAll(".linha-tempo li");
                            var meses = (document.GetElementById("mesRemuneracao") as IHtmlSelectElement).Options.OrderByDescending(x => Convert.ToInt32(x.Value));

                            if (!meses.Any())
                            {
                                Console.WriteLine("Meses indisponiveis:" + document.Url);
                                erroColeta = true;
                                continue;
                            }

                            foreach (var mes in meses)
                            {
                                if ((calendario[Convert.ToInt32(mes.Value) - 1] as IHtmlListItemElement).IsHidden || Convert.ToInt32(mes.Value) > mesSelecionado) continue;
                                if (new DateTime(Convert.ToInt32(ano.InnerHtml), Convert.ToInt32(mes.Value), 1) <= dtUltimaRemuneracaoColetada) continue;

                                //Console.WriteLine();
                                //Console.WriteLine($"Mes: {mes.Text}");
                                if (Convert.ToInt32(mes.Value) == mesSelecionado)
                                {
                                    address = $"https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/{chave}?ano={ano.Text}&mes={mes.Value}";
                                    document = await context.OpenAsync(address);

                                    if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
                                    {
                                        Console.WriteLine(document.StatusCode + ":" + document.Url);
                                        erroColeta = true;
                                        continue;
                                    }
                                }

                                var dadosIndisponiveis = document.QuerySelectorAll(".remuneracao-funcionario")[0].TextContent.Trim();
                                if (dadosIndisponiveis == "Ainda não há dados disponíveis.") continue;

                                var folhasPagamento = document.QuerySelectorAll(".remuneracao-funcionario__info");
                                if (!folhasPagamento.Any())
                                {
                                    Console.WriteLine("Dados indisponiveis:" + document.Url);
                                    erroColeta = true;
                                    continue;
                                }

                                foreach (var folhaPagamento in folhasPagamento)
                                {
                                    var titulo = folhaPagamento.QuerySelector(".remuneracao-funcionario__mes-ano").TextContent;
                                    var nivel = folhaPagamento.QuerySelectorAll(".remuneracao-funcionario__info-item")[3].TextContent.Replace("Função/cargo em comissão:", "").Trim();
                                    var linhas = folhaPagamento.QuerySelectorAll(".tabela-responsiva--remuneracao-funcionario tbody tr");
                                    foreach (var linha in linhas)
                                    {
                                        var colunas = linha.QuerySelectorAll("td");
                                        if (!colunas.Any()) continue;

                                        var descricao = colunas[0].TextContent.Trim();
                                        var valor = Convert.ToDecimal(colunas[1].TextContent.Trim());

                                        db.AddParameter(colunasBanco[descricao], valor);
                                    }

                                    int tipo = 0;
                                    switch (titulo.Split("–")[1].Trim())
                                    {
                                        case "FOLHA NORMAL":
                                            tipo = 1;
                                            break;
                                        case "FOLHA COMPLEMENTAR":
                                            tipo = 2;
                                            break;
                                        case "FOLHA COMPLEMENTAR ESPECIAL":
                                            tipo = 3;
                                            break;
                                        case "FOLHA DE ADIANTAMENTO GRATIFICAÇÃO NATALINA":
                                            tipo = 4;
                                            break;
                                        case "FOLHA DE GRATIFICAÇÃO NATALINA":
                                            tipo = 5;
                                            break;
                                        default:
                                            throw new NotImplementedException();
                                    }

                                    var sqlInsertRemuneracao = @"
INSERT IGNORE INTO cf_funcionario_remuneracao (
    id_cf_funcionario, id_cf_funcionario_contratacao, id_cf_deputado, referencia, tipo, remuneracao_fixa, vantagens_natureza_pessoal, 
    funcao_ou_cargo_em_comissao, gratificacao_natalina, ferias, outras_remuneracoes, abono_permanencia, valor_bruto, 
    redutor_constitucional, contribuicao_previdenciaria, imposto_renda, valor_liquido, 
    valor_diarias, valor_auxilios, valor_vantagens, valor_outros, valor_total, nivel
) VALUES (
    @id_cf_funcionario, @id_cf_funcionario_contratacao, @id_cf_deputado, @referencia, @tipo, @remuneracao_fixa, @vantagens_natureza_pessoal, 
    @funcao_ou_cargo_em_comissao, @gratificacao_natalina, @ferias, @outras_remuneracoes, @abono_permanencia, null, 
    @redutor_constitucional, @contribuicao_previdenciaria, @imposto_renda, @valor_liquido,
    @valor_diarias, @valor_auxilios, @valor_vantagens, null, null, @nivel
)";

                                    db.AddParameter("@id_cf_funcionario", idFuncionario);
                                    db.AddParameter("@id_cf_funcionario_contratacao", idFuncionarioContratacao);
                                    db.AddParameter("@id_cf_deputado", idDeputado);
                                    db.AddParameter("@tipo", tipo);
                                    db.AddParameter("@nivel", nivel);
                                    db.AddParameter("@referencia", new DateTime(Convert.ToInt32(ano.Text), Convert.ToInt32(mes.Value), 1));
                                    db.ExecuteNonQuery(sqlInsertRemuneracao);
                                }
                            }
                        }
                    }

                    if (!erroColeta)
                        MarcarComoProcessado(db, idFuncionario);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(secretario["id_cf_funcionario"].ToString() + ":" + ex.Message);
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                }
            }
        }
    }

    private void MarcarComoProcessado(AppDb db, int idFuncionario)
    {
        db.AddParameter("@id", idFuncionario);
        db.ExecuteNonQuery("update cf_funcionario set processado = 1 where id = @id");
    }

    public async Task ColetaDadosFuncionarios()
    {
        var addresses = new string[]
        {
            "https://www.camara.leg.br/transparencia/recursos-humanos/funcionarios?search=&areaDeAtuacao=&categoriaFuncional=&lotacao=&situacao=Em%20exerc%C3%ADcio&pagina=",
            "https://www.camara.leg.br/transparencia/recursos-humanos/funcionarios?search=&categoriaFuncional=&areaDeAtuacao=&situacao=Aposentado&pagina=",
            "https://www.camara.leg.br/transparencia/recursos-humanos/funcionarios?search=&categoriaFuncional=&areaDeAtuacao=&situacao=Licenciado&pagina=",
            "https://www.camara.leg.br/transparencia/recursos-humanos/funcionarios?search=&categoriaFuncional=&areaDeAtuacao=&situacao=Cedido&pagina="
        };

        var sqlInsert = @"
insert ignore into cf_funcionario_temp (chave, nome, categoria_funcional, area_atuacao, situacao{0})
values (@chave, @nome, @categoria_funcional, @area_atuacao, @situacao{1})
";
        var config = Configuration.Default.WithDefaultLoader();
        var context = BrowsingContext.New(config);

        using (var db = new AppDb())
        {
            db.ExecuteNonQuery("truncate table cf_funcionario_temp");

            foreach (var address in addresses)
            {
                var pagina = 0;
                while (true)
                {
                    var document = await context.OpenAsync(address + (++pagina).ToString());
                    if (document.StatusCode != HttpStatusCode.OK)
                    {
                        Console.WriteLine(document.StatusCode);
                        Thread.Sleep(TimeSpan.FromSeconds(30));

                        document = await context.OpenAsync(address + (++pagina).ToString());
                        if (document.StatusCode != HttpStatusCode.OK)
                        {
                            Console.WriteLine(document.StatusCode);
                            break;
                        };
                    };

                    var linhas = document.QuerySelectorAll(".l-busca__secao--resultados table.tabela-responsiva tbody tr");
                    foreach (var linha in linhas)
                    {
                        string colunasDB = "", valoresDB = "";

                        var colunas = linha.QuerySelectorAll("td");
                        var nome = colunas[0].TextContent.Trim();
                        var categoria = colunas[1].TextContent.Trim();
                        var area = colunas[2].TextContent.Trim();
                        var situacao = colunas[3].TextContent.Trim();

                        var chave = (colunas[0].FirstElementChild as IHtmlAnchorElement).GetAttribute("data-target");

                        if (categoria.StartsWith("Deputado")) categoria = "Deputado";
                        if (categoria.StartsWith("Ex-deputado")) categoria = "Ex-deputado";
                        if (area == "—") area = null;

                        Console.WriteLine();
                        Console.WriteLine($"Funcionario: {nome}");

                        var lstInfo = document.QuerySelectorAll(".modal--funcionario" + chave + " .lista-funcionario__item");
                        foreach (var info in lstInfo)
                        {
                            try
                            {
                                var titulo = info.QuerySelector(".lista-funcionario__titulo")?.TextContent.Trim();
                                var valor = info?.TextContent?.Replace(titulo, "").Trim();

                                switch (titulo.Replace(":", ""))
                                {
                                    case "Cargo":
                                        colunasDB += ", cargo";
                                        valoresDB += ", @cargo";
                                        db.AddParameter("@cargo", valor);
                                        break;
                                    case "Nível":
                                        colunasDB += ", nivel";
                                        valoresDB += ", @nivel";
                                        db.AddParameter("@nivel", valor);
                                        break;
                                    case "Função comissionada":
                                        colunasDB += ", funcao_comissionada";
                                        valoresDB += ", @funcao_comissionada";
                                        db.AddParameter("@funcao_comissionada", valor);
                                        break;
                                    case "Local de trabalho":
                                        colunasDB += ", local_trabalho";
                                        valoresDB += ", @local_trabalho";
                                        db.AddParameter("@local_trabalho", valor);
                                        break;
                                    case "Data da designação da função":
                                        colunasDB += ", data_designacao_funcao";
                                        valoresDB += ", @data_designacao_funcao";
                                        db.AddParameter("@data_designacao_funcao", Convert.ToDateTime(valor));
                                        break;
                                    case "Situação":
                                    case "Categoria funcional":
                                    case "Área de atuação":
                                        break;
                                    default:
                                        throw new NotImplementedException($"Vefificar beneficios do funcionario {nome}.");
                                }

                                Console.WriteLine($"{titulo}: {valor}");
                            }
                            catch (Exception ex)
                            {
                                logger.LogError(ex, ex.Message);
                            }
                        }

                        db.AddParameter("@chave", chave.Replace("#", ""));
                        db.AddParameter("@nome", nome);
                        db.AddParameter("@categoria_funcional", categoria);
                        db.AddParameter("@area_atuacao", area);
                        db.AddParameter("@situacao", situacao);
                        db.ExecuteNonQuery(string.Format(sqlInsert, colunasDB, valoresDB));
                    }

                    if (linhas.Count() < 20)
                    {
                        logger.LogInformation($"Parando na pagina {pagina}!");
                        break;
                    }
                }
            }



            db.ExecuteNonQuery(@"
INSERT ignore INTO cf_funcionario(chave, nome)
SELECT chave, nome FROM cf_funcionario_temp t");

            db.ExecuteNonQuery(@"
INSERT INTO cf_funcionario_contratacao 
	(id_cf_deputado, id_cf_funcionario, id_cf_funcionario_grupo_funcional, id_cf_funcionario_cargo, id_cf_funcionario_nivel, id_cf_funcionario_funcao_comissionada, 
		id_cf_funcionario_area_atuacao, id_cf_funcionario_local_trabalho, id_cf_funcionario_situacao, periodo_de, periodo_ate)
SELECT NULL, f.id, gf.id, c.id, n.id, fc.id, aa.id, lt.id, s.id, t.data_designacao_funcao, null 
FROM cf_funcionario_temp t
JOIN cf_funcionario f ON f.chave = t.chave
LEFT JOIN cf_funcionario_grupo_funcional gf ON gf.nome = t.categoria_funcional
LEFT JOIN cf_funcionario_cargo c ON c.nome = t.cargo
LEFT JOIN cf_funcionario_nivel n ON n.nome = t.nivel
LEFT JOIN cf_funcionario_funcao_comissionada fc ON fc.nome = t.funcao_comissionada
LEFT JOIN cf_funcionario_area_atuacao aa ON aa.nome = t.area_atuacao
LEFT JOIN cf_funcionario_local_trabalho lt ON lt.nome = t.local_trabalho
LEFT JOIN cf_funcionario_situacao s ON s.nome = t.situacao
LEFT JOIN cf_funcionario_contratacao ct ON ct.id_cf_funcionario = f.id
WHERE ct.id IS NULL");
        }
    }

    #endregion Importação Remuneração
}


public class Dado
{
    public int id { get; set; }
    public int idLegislatura { get; set; }
    public string nome { get; set; }
    public string siglaPartido { get; set; }
    public string siglaUf { get; set; }
    public string uri { get; set; }
    public string uriPartido { get; set; }
    public string urlFoto { get; set; }
}

public class Link
{
    public string href { get; set; }
    public string rel { get; set; }
}

public class Deputados
{
    public List<Dado> dados { get; set; }
    public List<Link> links { get; set; }
}

public class Gabinete
{
    public string andar { get; set; }
    public string email { get; set; }
    public string nome { get; set; }
    public string predio { get; set; }
    public string sala { get; set; }
    public string telefone { get; set; }
}

public class UltimoStatus
{
    public string condicaoEleitoral { get; set; }
    public string data { get; set; } // Datetime
    public string descricaoStatus { get; set; }
    public Gabinete gabinete { get; set; }
    public int id { get; set; }
    public int idLegislatura { get; set; }
    public string nome { get; set; }
    public string nomeEleitoral { get; set; }
    public string siglaPartido { get; set; }
    public string siglaUf { get; set; }
    public string situacao { get; set; }
    public string uri { get; set; }
    public string uriPartido { get; set; }
    public string urlFoto { get; set; }
}

public class Dados
{
    public string cpf { get; set; }
    public string dataFalecimento { get; set; } //DateTime?
    public string dataNascimento { get; set; } //DateTime?
    public string escolaridade { get; set; }
    public int id { get; set; }
    public string municipioNascimento { get; set; }
    public string nomeCivil { get; set; }
    public List<string> redeSocial { get; set; }
    public string sexo { get; set; }
    public string ufNascimento { get; set; }
    public UltimoStatus ultimoStatus { get; set; }
    public string uri { get; set; }
    public string urlWebsite { get; set; }
}

[XmlRoot(ElementName = "xml")]
public class DeputadoDetalhes
{
    public Dados dados { get; set; }
    public List<Link> links { get; set; }
}