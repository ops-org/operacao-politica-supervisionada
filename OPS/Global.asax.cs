using Newtonsoft.Json;
using OPS.Core;
using OPS.Core.DAO;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace OPS
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			Core.Padrao.ConnectionString =
				System.Configuration.ConfigurationManager.ConnectionStrings["AuditoriaContext"].ToString();

			//AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
			//register your filter with Web API pipeline
			GlobalConfiguration.Configuration.Filters.Add(new ExceptionHandlingAttribute());

			new ParametrosDao().CarregarPadroes();

			ViewEngines.Engines.Clear();
			ViewEngines.Engines.Add(new RazorViewEngine());
		}


		protected void Application_Error(object sender, EventArgs e)
		{
#if !DEBUG
			try
			{
				string message = Server.GetLastError().Message;

				//Ignorar OperationCanceledException e HttpException
				if (!message.Contains("This is an invalid webresource request.") && !message.Contains("The operation was canceled."))
				{
					string infoAdicional = JsonConvert.SerializeObject(Context.Request,
					Formatting.Indented, new JsonSerializerSettings
					{
						ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
						ContractResolver = new IgnoreErrorPropertiesResolver()
					});

					var exBase = Server.GetLastError().GetBaseException();
					HttpUnhandledException httpUnhandledException = new HttpUnhandledException(
						exBase.Message + Environment.NewLine + "<code>" + infoAdicional + "</code>",
						exBase.GetBaseException());

					Task.Run(async () =>
					{
						await Utils.SendMailAsync(new MailAddress("suporte@ops.net.br"), "OPS :: " + exBase.Message,
							httpUnhandledException.GetHtmlErrorMessage());
					}).Wait();
				}
			}
			catch
			{
				// ignored
			}
#endif
		}
	}
}
