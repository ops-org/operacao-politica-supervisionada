using Microsoft.Extensions.Configuration;
using OPS.Core;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OPS.Importador
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

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
                //new Core.DAO.ParametrosDao().CarregarPadroes();

                ////ImportacaoDadosCompleto(configuration).Wait();
                //Camara.ImportaPresencasDeputados();

                //var TelegramApiToken = configuration["AppSettings:TelegramApiToken"];
                //var ReceitaWsApiToken = configuration["AppSettings:ReceitaWsApiToken"];
                //var sb = new StringBuilder();
                //var rootPath = configuration["AppSettings:SiteRootFolder"];
                //var tempPath = System.IO.Path.Combine(rootPath, "static/temp");
                //var sDeputadosImagesPath = System.IO.Path.Combine(rootPath, "static/img/depfederal/");
                //var sSenadoressImagesPath = System.IO.Path.Combine(rootPath, "static/img/senador/");
                //Camara.DownloadFotosDeputados(sDeputadosImagesPath);


                //Senado.ImportarDespesas(tempPath, DateTime.Now.Year - 1, false);
                //Senado.ImportarDespesas(tempPath, DateTime.Now.Year, false);
                //Senado.AtualizaCadastroSenadores();

                ////var tempPath = configuration["AppSettings:SiteTempFolder"];
                //sb.Append(Senado.ImportarDespesas(tempPath, DateTime.Now.Year - 1, false));
                //sb.Append(Senado.ImportarDespesas(tempPath, DateTime.Now.Year, false));

                //Camara.ImportarMandatos();
                //ImportarPartidos().Wait();

                //var data = new DateTime(2012, 9, 1);
                //do
                //{
                //    sb.Append(Camara.ImportarRemuneracao(tempPath, data.Year, data.Month));

                //    data = data.AddMonths(1);
                //} while (data < DateTime.Now.Date);

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
                //Camara.ImportaPresencasDeputados();

                //Camara.AtualizaDeputadoValores();
                //Camara.AtualizaCampeoesGastos();
                //Camara.AtualizaResumoMensal();
                //Camara.ImportarMandatos();

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

                //Fornecedor.ConsultarReceitaWS(ReceitaWsApiToken, TelegramApiToken).Wait();

                //Camara.ColetaDadosFuncionarios().Wait();
                //Camara.ColetaDadosDeputados();
                //Camara.ColetaRemuneracaoSecretarios();

                //var s = Camara.ImportarDeputados(
                //    @"D:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\DEPFEDERAL\");

                //      Console.Write(s);

                ////Fornecedor.AtualizaFornecedorDoador();
                //Fornecedor.ConsultarCNPJ();
                //var s = Senado.ImportarDespesas(tempPath, DateTime.Now.Year, false);
                //var s = Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year - 1);


                //Senado.CarregaSenadoresAtuais();

                //var d = Camara.ImportarDeputados();
                //var a = Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year - 1);
                //var b = Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year);

                //Camara.ImportarDeputados();



                //Senado.ImportarDespesas(tempPath, DateTime.Now.Year - 1, false);

                //Console.WriteLine(sb.ToString());
                Console.WriteLine("Concluido! Tecle [ENTER] para sair.");
                Console.ReadKey();
            }
        }

        private static async Task ImportacaoDadosCompleto(IConfiguration configuration)
        {
            var rootPath = configuration["AppSettings:SiteRootFolder"];
            var TelegramApiToken = configuration["AppSettings:TelegramApiToken"];
            var ReceitaWsApiToken = configuration["AppSettings:ReceitaWsApiToken"];
            var tempPath = System.IO.Path.Combine(rootPath, "temp");
            var sDeputadosImagesPath = System.IO.Path.Combine(rootPath, "img/depfederal/");
            var sSenadoressImagesPath = System.IO.Path.Combine(rootPath, "img/senador/");

            try
            {
                var sb = new StringBuilder();
                TimeSpan t;
                Stopwatch sw = Stopwatch.StartNew();
                Stopwatch swGeral = Stopwatch.StartNew();
                new Core.DAO.ParametrosDao().CarregarPadroes();
                var inicioImportacao = DateTime.UtcNow.AddHours(-3).ToString("dd/MM/yyyy HH:mm");

                sb.Append("<div style='font-weight: bold;'>-- Importar Deputados :: @duracao --</div>");
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

                sb.Append("<div style='font-weight: bold;'>-- Importar Fotos Deputados :: @duracao --</div>");
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

                sb.AppendFormat("<div style='font-weight: bold;'>-- Importar Despesas Deputados {0} :: @duracao --</div>", DateTime.Now.Year - 1);
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

                sb.AppendFormat("<div style='font-weight: bold;'>-- Importar Despesas Deputados {0} :: @duracao --</div>", DateTime.Now.Year);
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

                sb.AppendLine("<div style='font-weight: bold;'>-- Importar Presenças Deputados :: @duracao --</div>");
                sw.Restart();
                try
                {
                    sb.Append(Camara.ImportaPresencasDeputados());
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString());
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));


                sb.AppendFormat("<div style='font-weight: bold;'>-- Importar Senadores {0} :: @duracao --</div>", DateTime.Now.Year);
                sw.Restart();
                try
                {
                    sb.Append(Senado.AtualizaCadastroSenadores());
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString());
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                sb.AppendFormat("<div style='font-weight: bold;'>-- Importar Imagens Senadores {0} :: @duracao --</div>", DateTime.Now.Year);
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

                sb.AppendFormat("<div style='font-weight: bold;'>-- Importar Despesas Senado {0} :: @duracao --</div>", DateTime.Now.Year - 1);
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

                sb.AppendFormat("<div style='font-weight: bold;'>-- Importar Despesas Senado {0} :: @duracao --</div>", DateTime.Now.Year);
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

                sb.Append("<div style='font-weight: bold;'>-- Consultar Receita WS :: @duracao --</div>");
                sw.Restart();
                try
                {
                    sb.Append(await Fornecedor.ConsultarReceitaWS(ReceitaWsApiToken, TelegramApiToken));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.ToFullDescriptionString());
                }
                t = sw.Elapsed;
                sb = sb.Replace("@duracao", string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds));

                t = swGeral.Elapsed;
                sb.AppendFormat("<div style='font-weight: bold;'>Inicio da importação: {0} - Duração Total: {1:D2}h:{2:D2}m:{3:D2}s</div>", inicioImportacao, t.Hours, t.Minutes, t.Seconds);

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
    }
}