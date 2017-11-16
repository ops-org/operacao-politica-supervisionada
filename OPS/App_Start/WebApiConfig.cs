using Microsoft.Owin.Security.OAuth;
using OPS.Core;
using System.Web.Http;

namespace OPS
{
	public static class WebApiConfig
	{
		//http://www.asp.net/web-api/overview/web-api-routing-and-actions/routing-in-aspnet-web-api
		public static void Register(HttpConfiguration config)
		{
			// Web API configuration and services
			// Configure Web API to use only bearer token authentication.
			config.SuppressDefaultHostAuthentication();

#if DEBUG
			// To disable tracing in your application, please comment out or remove the following line of code
			// For more information, refer to: http://www.asp.net/web-api
			config.EnableSystemDiagnosticsTracing();
#endif

			//config.Filters.Add(new HostAuthenticationFilter(new OAuthBearerAuthenticationOptions().AuthenticationType));
			config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

			//config.Filters.Add(new HostAuthenticationFilter(Startup.OAuthBearerOptions.AuthenticationType));

			config.Filters.Add(new LoggingFilterAttribute());

			// Web API routes
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);

			
			var json = config.Formatters.JsonFormatter;
			config.Formatters.Clear();
			config.Formatters.Add(json);
			json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;

			config.Filters.Add(new ExceptionHandlingAttribute());
		}
	}
}
