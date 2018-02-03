using OPS.Core;
using OPS.Core.DAO;
using OPS.ImportacaoDados;
using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebApi.OutputCache.V2;

namespace OPS.WebApi
{
	[RoutePrefix("Api/Tarefa")]
	public class TarefaController : ApiController
	{
		[HttpGet]
		[Route("ImportarDados/{value}")]
		public async Task<IHttpActionResult> ImportarDados(string value)
		{
			if ((DateTime.UtcNow.Hour != 3 || value != System.Web.Configuration.WebConfigurationManager.AppSettings.Get("TaskKey")) &&
				value != "m" + System.Web.Configuration.WebConfigurationManager.AppSettings.Get("TaskKey")) return BadRequest("NOPS - " + DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"));

			var tempPath = System.Web.Hosting.HostingEnvironment.MapPath("~/temp/");

			try
			{
				var sb = new StringBuilder();
				TimeSpan t;
				Stopwatch sw = Stopwatch.StartNew();
				Stopwatch swGeral = Stopwatch.StartNew();

				sb.AppendFormat("<br/><h3>-- Importar Despesas Deputados {0} --</h3>", DateTime.Now.Year - 1);
				sb.AppendFormat("<br/><p>" + DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm") + "</p>");
				sw.Restart();
				try
				{
					sb.Append(Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year - 1));
				}
				catch (Exception ex)
				{
					sb.Append(ex.Message);
				}
				t = sw.Elapsed;
				sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

				sb.AppendFormat("<br/><h3>-- Importar Despesas Deputados {0} --</h3>", DateTime.Now.Year);
				sb.AppendFormat("<br/><p>" + DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm") + "</p>");
				sw.Restart();
				try
				{
					sb.Append(Camara.ImportarDespesasXml(tempPath, DateTime.Now.Year));
				}
				catch (Exception ex)
				{
					sb.Append(ex.Message);
				}
				t = sw.Elapsed;
				sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

				sb.AppendFormat("<br/><h3>-- Importa Presenças Deputados --</h3>");
				sw.Restart();
				try
				{
					sb.Append(Camara.ImportaPresencasDeputados());
				}
				catch (Exception ex)
				{
					sb.Append(ex.Message);
				}
				t = sw.Elapsed;
				sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

				sb.AppendFormat("<br/><h3>-- Importar Despesas Senado {0} --</h3>", DateTime.Now.Year - 1);
				sw.Restart();
				try
				{
					sb.Append(Senado.ImportarDespesas(tempPath, DateTime.Now.Year - 1, false));
				}
				catch (Exception ex)
				{
					sb.Append(ex.Message);
				}
				t = sw.Elapsed;
				sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

				sb.AppendFormat("<br/><h3>-- Importar Despesas Senado {0} --</h3>", DateTime.Now.Year);
				sw.Restart();
				try
				{
					sb.Append(Senado.ImportarDespesas(tempPath, DateTime.Now.Year, false));
				}
				catch (Exception ex)
				{
					sb.Append(ex.Message);
				}
				t = sw.Elapsed;
				sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

				sb.AppendFormat("<br/><h3>-- Consultar Receita WS --</h3>");
				sw.Restart();
				try
				{
					sb.Append(Fornecedor.ConsultarReceitaWS());
				}
				catch (Exception ex)
				{
					sb.Append(ex.Message);
				}
				t = sw.Elapsed;
				sb.AppendFormat("<p>Duração: {0:D2}h:{1:D2}m:{2:D2}s</p>", t.Hours, t.Minutes, t.Seconds);

				t = swGeral.Elapsed;
				sb.AppendFormat("<br/><h3>Duração Total: {0:D2}h:{1:D2}m:{2:D2}s</h3>", t.Hours, t.Minutes, t.Seconds);

				new ParametrosDao().CarregarPadroes();

				var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
				var allKeys = cache.AllKeys;
				foreach (var cacheKey in allKeys)
				{
					cache.Remove(cacheKey);
				}

				await Utils.SendMailAsync(new MailAddress(Padrao.EmailEnvioErros), "OPS :: Resumo da Importação", sb.ToString());
			}
			catch (Exception ex)
			{
				HttpUnhandledException httpUnhandledException = new HttpUnhandledException(
				   ex.GetBaseException().Message, ex.GetBaseException());

				await Utils.SendMailAsync(new MailAddress(Padrao.EmailEnvioErros), "OPS :: Informe de erro na Importação",
						httpUnhandledException.GetHtmlErrorMessage());
			}

			return Ok(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"));
		}
	}
}