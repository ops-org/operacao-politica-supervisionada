using OPS.Core;
using OPS.ImportacaoDados;
using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace OPS.WebApi
{
	[RoutePrefix("Api/Tarefa")]
	public class TarefaController : ApiController
	{
		[HttpGet]
		[Route("ImportarDados/{value}")]
		public void ImportarDados(string value)
		{
			if (DateTime.Now.Hour != 3 || value != System.Web.Configuration.WebConfigurationManager.AppSettings.Get("TaskKey")) return;

			Task.Run(async () =>
			{
				var tempPath = System.Web.Hosting.HostingEnvironment.MapPath("~/temp/");

				try
				{
					var sb = new StringBuilder();
					Stopwatch sw = Stopwatch.StartNew();
					TimeSpan t;

					Camara.ImportaPresencasDeputados();
					t = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
					sb.AppendFormat("<p>ImportaPresencasDeputados: {0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms</p>", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
					sw.Restart();


					sw.Stop();

					await Utils.SendMailAsync(new MailAddress("suporte@ops.net.br"), "OPS :: Resumo da Importação", sb.ToString());
				}
				catch (Exception ex)
				{
					HttpUnhandledException httpUnhandledException = new HttpUnhandledException(
					   ex.GetBaseException().Message, ex.GetBaseException());

					await Utils.SendMailAsync(new MailAddress("suporte@ops.net.br"), "OPS :: Informe de erro na Importação",
							httpUnhandledException.GetHtmlErrorMessage());
				}
			});
		}
	}
}