using System;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mail;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using OPS.Core;

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

            new Dao.ParametrosDao().CarregarPadroes();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            try
            {
                HttpUnhandledException httpUnhandledException = new HttpUnhandledException(
                    Server.GetLastError().Message, Server.GetLastError());

                Task.Run(async () =>
                {
                    await Utils.SendMailAsync(new MailAddress("suporte@ops.net.br"), "OPS :: Informe de erro",
                        httpUnhandledException.GetHtmlErrorMessage());
                }).Wait();
            }
            catch
            {
                // ignored
            }
        }
    }
}
