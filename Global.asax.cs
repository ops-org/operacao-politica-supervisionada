using OPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);

			new Dao.ParametrosDao().CarregarPadroes();
		}

		//protected void Session_Start(object sender, EventArgs e)
		//{
		//	if (Padrao.DeputadoFederalMenorAno == 0)
		//	{
		//		new Dao.ParametrosDao().CarregarPadroes();
		//	}
		//}

		protected void Application_PostAuthorizeRequest()
		{
			//if (IsWebApiRequest())
			//{
			//	//Habilitar o SessionState na WebAPI
			//	HttpContext.Current.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.Required);
			//}
		}

		//private bool IsWebApiRequest()
		//{
		//	return HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith(WebApiConfig.UrlPrefixRelative);
		//}
	}
}
