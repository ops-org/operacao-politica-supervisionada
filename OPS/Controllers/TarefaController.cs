using AspNetCore.CacheOutput;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OPS.Core;
using OPS.Core.DAO;
using OPS.ImportacaoDados;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace OPS.WebApi
{
    [ApiController]
    [Route("api/[controller]")]
    public class TarefaController : Controller
    {

        private IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }
        public IApiCacheOutput Cache { get; }

        public TarefaController(IConfiguration configuration, IWebHostEnvironment env, IApiCacheOutput cache)
        {
            Environment = env;
            Configuration = configuration;
            Cache = cache;
        }

        [HttpGet]
        [Route("LimparCache")]
        public async void LimparCache()
        {
            new ParametrosDao().CarregarPadroes();

            await Cache.RemoveStartsWithAsync("*");
        }

        [HttpGet]
        [Route("ImportarDados/{value}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public async Task<IActionResult> ImportarDados(string value)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            if ((DateTime.UtcNow.Hour != 3 || value != Configuration["AppSettings:TaskKey"]) &&
                value != "m" + Configuration["AppSettings:TaskKey"]) return BadRequest("NOPS - " + DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm", cultureInfo));

            var tempPath = System.IO.Path.Combine(Environment.WebRootPath, "temp");
            var sDeputadosImagesPath = System.IO.Path.Combine(Environment.WebRootPath, "images/Parlamentares/DEPFEDERAL/");

            try
            {
                var sb = new StringBuilder();
                TimeSpan t;
                Stopwatch sw = Stopwatch.StartNew();
                Stopwatch swGeral = Stopwatch.StartNew();

                sb.AppendFormat(cultureInfo, "<p>Iniciando Importação: {0:dd/MM/yyyy HH:mm}</p>", DateTime.UtcNow);

                sb.Append("<br/><h3>-- Importar Deputados --</h3>");
                sw.Restart();
                try
                {
                    sb.Append(Camara.ImportarDeputados());
                }
                catch (Exception ex)
                {
                    sb.Append(ex.GetBaseException().Message);
                }
                t = sw.Elapsed;
                sb.AppendFormat(cultureInfo, "<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

                sb.Append("<br/><h3>-- Importar Fotos Deputados --</h3>");
                sw.Restart();
                try
                {
                    sb.Append(Camara.DownloadFotosDeputados(sDeputadosImagesPath));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.GetBaseException().Message);
                }
                t = sw.Elapsed;
                sb.AppendFormat(cultureInfo, "<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

                sb.AppendFormat(cultureInfo, "<br/><h3>-- Importar Despesas Deputados {0} --</h3>", DateTime.Now.Year - 1);
                
                sw.Restart();
                try
                {
                    sb.Append(Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year - 1));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.GetBaseException().Message + ex.GetBaseException().StackTrace);
                }
                t = sw.Elapsed;
                sb.AppendFormat(cultureInfo, "<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

                sb.AppendFormat(cultureInfo, "<br/><h3>-- Importar Despesas Deputados {0} --</h3>", DateTime.Now.Year);
                sw.Restart();
                try
                {
                    sb.Append(Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.GetBaseException().Message);
                }
                t = sw.Elapsed;
                sb.AppendFormat(cultureInfo, "<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

                sb.Append("<br/><h3>-- Importar Presenças Deputados --</h3>");
                sw.Restart();
                try
                {
                    sb.Append(Camara.ImportaPresencasDeputados());
                }
                catch (Exception ex)
                {
                    sb.Append(ex.GetBaseException().Message);
                }
                t = sw.Elapsed;
                sb.AppendFormat(cultureInfo, "<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);


                sb.AppendFormat(cultureInfo, "<br/><h3>-- Importar Senadores {0} --</h3>", DateTime.Now.Year);
                sw.Restart();
                try
                {
                    sb.Append(Senado.CarregaSenadoresAtuais());
                }
                catch (Exception ex)
                {
                    sb.Append(ex.GetBaseException().Message);
                }
                t = sw.Elapsed;
                sb.AppendFormat(cultureInfo, "<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

                sb.AppendFormat(cultureInfo, "<br/><h3>-- Importar Despesas Senado {0} --</h3>", DateTime.Now.Year - 1);
                sw.Restart();
                try
                {
                    sb.Append(Senado.ImportarDespesas(tempPath, DateTime.Now.Year - 1, false));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.GetBaseException().Message);
                }
                t = sw.Elapsed;
                sb.AppendFormat(cultureInfo, "<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

                sb.AppendFormat(cultureInfo, "<br/><h3>-- Importar Despesas Senado {0} --</h3>", DateTime.Now.Year);
                sw.Restart();
                try
                {
                    sb.Append(Senado.ImportarDespesas(tempPath, DateTime.Now.Year, false));
                }
                catch (Exception ex)
                {
                    sb.Append(ex.GetBaseException().Message);
                }
                t = sw.Elapsed;
                sb.AppendFormat(cultureInfo, "<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

                sb.Append("<br/><h3>-- Consultar Receita WS --</h3>");
                sw.Restart();
                try
                {
                    sb.Append(Fornecedor.ConsultarReceitaWS());
                }
                catch (Exception ex)
                {
                    sb.Append(ex.GetBaseException().Message);
                }
                t = sw.Elapsed;
                sb.AppendFormat(cultureInfo, "<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

                t = swGeral.Elapsed;
                sb.AppendFormat(cultureInfo, "<br/><h3>Duração Total: {0:D2}h:{1:D2}m:{2:D2}s</h3>", t.Hours, t.Minutes, t.Seconds);

                new ParametrosDao().CarregarPadroes();

                //var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
                //var allKeys = cache.AllKeys;
                //foreach (var cacheKey in allKeys)
                //{
                //    cache.Remove(cacheKey);
                //}
                await Cache.RemoveStartsWithAsync("*");

                var lstEmails = Padrao.EmailEnvioResumoImportacao.Split(';');
                var lstEmailTo = new MailAddressCollection();
                foreach (string email in lstEmails)
                {
                    lstEmailTo.Add(email);
                }

                Console.WriteLine(sb.ToString());
                await Utils.SendMailAsync(Configuration, lstEmailTo, "OPS :: Resumo da Importação", sb.ToString());
            }
            catch (Exception ex)
            {
                var ex1 = ex.GetBaseException();
                var message = ex1.Message;
                if (ex1.StackTrace != null)
                    message += ex1.StackTrace;

                await Utils.SendMailAsync(Configuration, new MailAddress(Padrao.EmailEnvioErros), "OPS :: Informe de erro na Importação", message);
            }

            return Ok(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm", cultureInfo));
        }
    }
}