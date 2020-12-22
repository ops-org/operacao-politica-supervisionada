using Microsoft.Extensions.Configuration;
using OPS.Core;
using OPS.ImportacaoDados;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OPS.ImportacaoManual
{
    internal class Program
    {
        //public static void ConverterXmlParaCsvDespesasCamara(string tempPath)
        //{
        //	string[] ColunasXmlDespesasCamara = new string[]
        //   {
        //	"txNomeParlamentar",
        //	"ideCadastro",
        //	"nuCarteiraParlamentar",
        //	"nuLegislatura",
        //	"sgUF",
        //	"sgPartido",
        //	"codLegislatura",
        //	"numSubCota",
        //	"txtDescricao",
        //	"numEspecificacaoSubCota",
        //	"txtDescricaoEspecificacao",
        //	"txtFornecedor",
        //	"txtCNPJCPF",
        //	"txtNumero",
        //	"indTipoDocumento",
        //	"datEmissao",
        //	"vlrDocumento",
        //	"vlrGlosa",
        //	"vlrLiquido",
        //	"numMes",
        //	"numAno",
        //	"numParcela",
        //	"txtPassageiro",
        //	"txtTrecho",
        //	"numLote",
        //	"numRessarcimento",
        //	"ideDocumento",
        //	"vlrRestituicao",
        //	"nuDeputadoId"
        //   };

        //	string fullFileNameXml = tempPath + @"\AnoAtual.xml";
        //	StreamReader stream = null;

        //	try
        //	{
        //		if (fullFileNameXml.EndsWith("AnoAnterior.xml"))
        //			stream = new StreamReader(fullFileNameXml, Encoding.GetEncoding(850)); //"ISO-8859-1"
        //		else
        //			stream = new StreamReader(fullFileNameXml, Encoding.GetEncoding("ISO-8859-1"));

        //		using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings() { IgnoreComments = true }))
        //		{
        //			reader.ReadToDescendant("DESPESAS");
        //			reader.ReadToDescendant("DESPESA");

        //			using (StreamWriter outputFile = new StreamWriter(fullFileNameXml.Replace(".xml", ".csv")))
        //			{
        //				do
        //				{
        //					var lstCsv = new List<string>();
        //					var strXmlNodeDespeza = reader.ReadOuterXml();
        //					if (string.IsNullOrEmpty(strXmlNodeDespeza))
        //						break;

        //					XmlDocument doc = new XmlDocument();
        //					doc.LoadXml(strXmlNodeDespeza);
        //					XmlNodeList files = doc.DocumentElement.SelectNodes("*");

        //					int indexXml = 0;
        //					for (int i = 0; i < 29; i++)
        //					{
        //						if (files[indexXml].Name == ColunasXmlDespesasCamara[i])
        //						{
        //							lstCsv.Add(files[indexXml++].InnerText);
        //						}
        //						else
        //						{
        //							lstCsv.Add("");
        //						}

        //					}

        //					outputFile.WriteLine(string.Join(";", lstCsv));
        //				}
        //				while (true);
        //			}

        //			reader.Close();
        //		}
        //	}
        //	catch (Exception)
        //	{
        //		throw;
        //	}
        //	finally
        //	{
        //		stream.Close();
        //		stream.Dispose();
        //	}
        //}

        public static void Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(environmentName)) environmentName = "Development";

            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            CultureInfo ci = new CultureInfo("pt-BR");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            Padrao.ConnectionString = configuration.GetConnectionString("AuditoriaContext");
            Console.WriteLine(environmentName);


            if (environmentName == "Production")
            {
                ImportacaoDadosCompleto(configuration).Wait();
            }
            else
            {
                //ImportacaoDadosCompleto(configuration).Wait();


                var sb = new StringBuilder();
                var rootPath = configuration["AppSettings:SiteRootFolder"];
                var tempPath = System.IO.Path.Combine(rootPath, "static/temp");
                //var sDeputadosImagesPath = System.IO.Path.Combine(rootPath, "static/img/depfederal/");
                //var sSenadoressImagesPath = System.IO.Path.Combine(rootPath, "static/img/senador/");
                //Camara.DownloadFotosDeputados(sDeputadosImagesPath);

                ////var tempPath = configuration["AppSettings:SiteTempFolder"];
                //sb.Append(Senado.ImportarDespesas(tempPath, DateTime.Now.Year - 1, false));
                //sb.Append(Senado.ImportarDespesas(tempPath, DateTime.Now.Year, false));


                var data = new DateTime(2012, 9, 1);
                do
                {
                    sb.Append(Camara.ImportarRemuneracao(tempPath, data.Year, data.Month));

                    data = data.AddMonths(1);
                } while (data < DateTime.Now.Date);

                //try
                //{
                //    var inicio = new DateTime(2018, 01, 01);
                //    var fim = new DateTime(2020, 12, 01);
                //    while (true)
                //    {
                //        sb.Append(Senado.ImportarRemuneracao(tempPath, Convert.ToInt32(inicio.ToString("yyyyMM"))));

                //        inicio = inicio.AddMonths(1);
                //        if (inicio > fim) break;
                //    }
                //}
                //catch (Exception ex)
                //{
                //    sb.Append(ex.ToFullDescriptionString());
                //}
                //t = sw.Elapsed;
                //sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));


                ////ConverterXmlParaCsvDespesasCamara(tempPath);

                //#region Camara
                //Camara.AtualizaInfoDeputados();
                //Camara.AtualizaInfoDeputadosCompleto();

                //Camara.ImportarMandatos();
                //Camara.DownloadFotosDeputados(@"D:\GitHub\OPS\OPS\wwwroot\images\Parlamentares\DEPFEDERAL\");

                //Importação na nova estrutura
                //for (int ano = 2009; ano <= 2018; ano++)
                //{
                //    Camara.ImportarDespesas(tempPath, ano, true);
                //}
                //Camara.ImportarDespesas(tempPath, 2017, false);

                //Console.WriteLine(Camara.ImportarDespesasXml(tempPath, 2016));
                //Console.WriteLine(Camara.ImportarDespesasXml(tempPath, 2017));
                //Console.WriteLine(Camara.ImportarDespesasXml(tempPath, 2018));
                ////Camara.AtualizaDeputadoValores();
                //Camara.ImportaPresencasDeputados();

                //Camara.AtualizaDeputadoValores();
                //Camara.AtualizaCampeoesGastos();
                //Camara.AtualizaResumoMensal();
                //Camara.ImportarMandatos();
                //Camara.ImportaPresencasDeputados();

                //Camara.AtualizaInfoDeputadosCompleto();
                //Camara.ImportarDeputados(52);

                ////Camara.ValidarLinkRecibos();
                //Camara.AtualizaResumoMensal();
                ////#endregion Camara

                ////#region Senado
                ////Senado.CarregaSenadores();
                //Senado.DownloadFotosSenadores(@"D:\GitHub\OPS\OPS\wwwroot\images\Parlamentares\SENADOR\");
                //Senado.CarregaSenadoresAtuais();

                //for (int ano = 2008; ano <= 2017; ano++)
                //{
                //	Senado.ImportarDespesas(tempPath, ano, true);
                //}
                //Senado.ImportarDespesas(tempPath, 2017, false);
                //#endregion Senado

                //Fornecedor.ConsultarReceitaWS();

                //var s = Camara.ImportarDeputados(
                //    @"D:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\DEPFEDERAL\");

                //      Console.Write(s);

                ////Fornecedor.AtualizaFornecedorDoador();
                //Fornecedor.ConsultarCNPJ();
                //Fornecedor.ConsultarReceitaWS();
                //var s = Senado.ImportarDespesas(tempPath, DateTime.Now.Year, false);
                //var s = Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year - 1);


                //Senado.CarregaSenadoresAtuais();

                //var d = Camara.ImportarDeputados();
                //var a = Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year - 1);
                //var b = Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year);

                //Fornecedor.ConsultarReceitaWS();
                //Camara.ImportarDeputados();

                //tempPath = System.AppDomain.CurrentDomain.BaseDirectory;
                //using (var banco = new Banco())
                //{
                //    string sql = @"
                //        SELECT ds.id, dp.id_deputado, ds.id_documento, ds.ano 
                //        FROM cf_despesa ds
                //        INNER JOIN cf_deputado dp on dp.id = ds.id_cf_deputado
                //        WHERE ds.ano >= 2015
                //        and ds.id_fornecedor IN(78823, 89903, 102626) 
                //        and id_documento is not null
                //        order by ds.id DESC";

                //    DataTable table = banco.GetTable(sql, 0);

                //    if (table.Rows.Count > 0)
                //    {
                //        Console.WriteLine("Total: " + table.Rows.Count);

                //        var ilegivel = 0;
                //        foreach (DataRow dr in table.Rows)
                //        {
                //            string downloadUrl = "http://www.camara.gov.br/cota-parlamentar/documentos/publ/" + dr["id_deputado"].ToString() + "/" + dr["ano"].ToString() + "/" + dr["id_documento"].ToString() + ".pdf";
                //            string fileName = tempPath + "\\pdf\\" + dr["id"].ToString() + ".pdf";
                //            string fileNameOK = tempPath + "\\pdf_ok\\" + dr["id"].ToString() + ".pdf";
                //            string fileNameSuspeito = tempPath + "\\pdf_suspeito\\" + dr["id"].ToString() + ".pdf";

                //            if (!File.Exists(fileName) && !File.Exists(fileNameOK) && !File.Exists(fileNameSuspeito))
                //            {
                //                try
                //                {
                //                    using (var client = new WebClient())
                //                    {
                //                        client.Headers.Add("User-Agent: Other");
                //                        client.DownloadFile(downloadUrl, fileName);
                //                    }
                //                }
                //                catch (WebException e)
                //                {
                //                    if (e.Message.Contains("404")) continue;

                //                    Console.WriteLine(e);
                //                    throw;
                //                }
                //            }

                //            try
                //            {
                //                var lstUber = ReadPdfFile(fileName, "Uber");
                //                if (!lstUber.Any())
                //                {
                //                    ilegivel++; //Console.WriteLine("Ilegivel:" + dr["id"].ToString() + " - " + downloadUrl);
                //                    continue;
                //                }

                //                var lstExtra = ReadPdfFile(fileName, "Extra");
                //                if (lstExtra.Any())
                //                {
                //                    File.Move(fileName, fileNameSuspeito);
                //                    //Console.WriteLine(dr["id"].ToString() + " - " + downloadUrl);
                //                    continue;
                //                }

                //                File.Move(fileName, fileNameOK);
                //            }
                //            catch (Exception e)
                //            {
                //                //Console.WriteLine(e.Message);
                //            }
                //        }

                //        Console.WriteLine("Ilegivel: " + ilegivel);
                //    }
                //}

                //Senado.ImportarDespesas(tempPath, DateTime.Now.Year - 1, false);

                Console.WriteLine(sb.ToString());
                Console.WriteLine("Concluido! Tecle [ENTER] para sair.");
                Console.ReadKey();
            }
        }

        private static async Task ImportacaoDadosCompleto(IConfiguration configuration)
        {
            var rootPath = configuration["AppSettings:SiteRootFolder"];
            var tempPath = System.IO.Path.Combine(rootPath, "static/temp");
            var sDeputadosImagesPath = System.IO.Path.Combine(rootPath, "static/img/depfederal/");
            var sSenadoressImagesPath = System.IO.Path.Combine(rootPath, "static/img/senador/");

            try
            {
                var sb = new StringBuilder();
                TimeSpan t;
                Stopwatch sw = Stopwatch.StartNew();
                Stopwatch swGeral = Stopwatch.StartNew();
                new Core.DAO.ParametrosDao().CarregarPadroes();
                var inicioImportacao = DateTime.UtcNow.AddHours(-3).ToString("dd/MM/yyyy HH:mm");

                sb.Append("<strong>-- Importar Deputados :: @duracao --</strong>");
                sw.Restart();
                try
                {
                    sb.Append(Camara.ImportarDeputados());
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString());
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                sb.Append("<strong>-- Importar Fotos Deputados :: @duracao --</strong>");
                sw.Restart();
                try
                {
                    sb.Append(Camara.DownloadFotosDeputados(sDeputadosImagesPath));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString());
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                sb.AppendFormat("<strong>-- Importar Despesas Deputados {0} :: @duracao --</strong>", DateTime.Now.Year - 1);
                //sb.AppendFormat("<p>" + DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm") + "</p>");
                sw.Restart();
                try
                {
                    sb.Append(Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year - 1));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString() + ex.GetBaseException().StackTrace);
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                sb.AppendFormat("<strong>-- Importar Despesas Deputados {0} :: @duracao --</strong>", DateTime.Now.Year);
                //sb.AppendFormat("<p>" + DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm") + "</p>");
                sw.Restart();
                try
                {
                    sb.Append(Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString());
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                sb.Append("<strong>-- Importar Presenças Deputados :: @duracao --</strong>");
                //sw.Restart();
                //try
                //{
                //    sb.Append(Camara.ImportaPresencasDeputados());
                //}
                //catch (Exception ex)
                //{
                //    sb.Append(ex.ToFullDescriptionString());
                //}
                //t = sw.Elapsed;
                //sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));



                sb.AppendFormat("<strong>-- Importar Secretários parlamentares {0} :: @duracao --</strong>", DateTime.Now.Year);
                sw.Restart();

                ////Instantiate DI container for the application  
                //var serviceCollection = new ServiceCollection();

                ////Register NodeServices  
                //serviceCollection.AddNodeServices();

                ////Request the DI container to supply the shared INodeServices instance  
                //var nodeService = serviceCollection.BuildServiceProvider().GetRequiredService<INodeServices>();

                ////Invoke the javascript module with parameters to execute in Node environment.  
                //var taskResult = await nodeService.InvokeAsync<string>(@"D:\GitHub\operacao-politica-supervisionada\OPS.ImportacaoNodejs\app.js");

                //sb.AppendFormat(taskResult);
                //t = sw.Elapsed;
                //sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                sb.AppendLine();
                sb.AppendFormat("<strong>-- Importar Senadores {0} :: @duracao --</strong>", DateTime.Now.Year);
                sw.Restart();
                try
                {
                    sb.Append(Senado.CarregaSenadoresAtuais());
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString());
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                sb.AppendLine();
                sb.AppendFormat("<strong>-- Importar Imagens Senadores {0} :: @duracao --</strong>", DateTime.Now.Year);
                sw.Restart();
                try
                {
                    sb.Append(Senado.DownloadFotosSenadores(sSenadoressImagesPath));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString());
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                sb.AppendLine();
                sb.AppendFormat("<strong>-- Importar Despesas Senado {0} :: @duracao --</strong>", DateTime.Now.Year - 1);
                sw.Restart();
                try
                {
                    sb.Append(Senado.ImportarDespesas(tempPath, DateTime.Now.Year - 1, false));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString());
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                sb.AppendFormat("<strong>-- Importar Despesas Senado {0} :: @duracao --</strong>", DateTime.Now.Year);
                sw.Restart();
                try
                {
                    sb.Append(Senado.ImportarDespesas(tempPath, DateTime.Now.Year, false));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString());
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                try
                {
                    var data = new DateTime(DateTime.Now.Year, DateTime.Now.Month - (DateTime.Now.Day < 10 ? 1 : 0), 01);
                    sb.Append(Senado.ImportarRemuneracao(tempPath, Convert.ToInt32(data.ToString("yyyyMM"))));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString());
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                sb.Append("<strong>-- Consultar Receita WS :: @duracao --</strong>");
                sw.Restart();
                try
                {
                    sb.Append(Fornecedor.ConsultarReceitaWS());
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString());
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                t = swGeral.Elapsed;
                sb.AppendFormat("<strong>Inicio da importação: {0} - Duração Total: {1:D2}h:{2:D2}m:{3:D2}s</strong>", inicioImportacao, t.Hours, t.Minutes, t.Seconds);

                using (WebClient client = new WebClient())
                {
                    await client.DownloadDataTaskAsync("http://127.0.0.1:5200/tarefa/limparcache");
                }

                var lstEmails = Padrao.EmailEnvioResumoImportacao.Split(';');
                var lstEmailTo = new MailAddressCollection();
                foreach (string email in lstEmails)
                {
                    lstEmailTo.Add(email);
                }

                Console.WriteLine(sb.ToString());
                await Utils.SendMailAsync(configuration["AppSettings:SendGridAPIKey"], lstEmailTo, "OPS :: Resumo da Importação - " + DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"), sb.ToString());
            }
            catch (Exception ex)
            {
                var ex1 = ex.GetBaseException();
                var message = ex1.Message;
                if (ex1.StackTrace != null)
                    message += ex1.StackTrace;

                await Utils.SendMailAsync(configuration["AppSettings:SendGridAPIKey"], new MailAddress(Padrao.EmailEnvioErros), "OPS :: Informe de erro na Importação", message);
            }
        }

        //public static List<int> ReadPdfFile(string fileName, String searthText)
        //{
        //    List<int> pages = new List<int>();
        //    if (File.Exists(fileName))
        //    {
        //        PdfReader pdfReader = new PdfReader(fileName);
        //        for (int page = 1; page <= pdfReader.NumberOfPages; page++)
        //        {
        //            ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

        //            string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
        //            if (currentPageText.Contains(searthText))
        //            {
        //                pages.Add(page);
        //            }
        //        }
        //        pdfReader.Close();
        //    }
        //    return pages;
        //}
    }
}