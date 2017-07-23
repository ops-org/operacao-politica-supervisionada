using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using OPS;
using OPS.Core.Auth;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace OPS
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			var config = new HttpConfiguration();

			var oAuth = new ConfigureStartup();
			oAuth.ConfigureOAuth(app);

			WebApiConfig.Register(config);
			app.UseCors(CorsOptions.AllowAll);
			app.UseWebApi(config);
		}
	}
}