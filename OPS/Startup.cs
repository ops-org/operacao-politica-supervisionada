using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.Twitter;
using OPS;
using OPS.Providers;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace OPS
{
	public class Startup
	{
		public const string XmlSchemaString = "http://www.w3.org/2001/XMLSchema#string";
		public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
		public static GoogleOAuth2AuthenticationOptions GoogleAuthOptions { get; private set; }
		public static FacebookAuthenticationOptions FacebookAuthOptions { get; private set; }
		public static IDataProtectionProvider DataProtectionProvider { get; set; }

		public void Configuration(IAppBuilder app)
		{
			var config = new HttpConfiguration();

			ConfigureOAuth(app);

			WebApiConfig.Register(config);
			app.UseCors(CorsOptions.AllowAll);
			app.UseWebApi(config);
			//Database.SetInitializer(new MigrateDatabaseToLatestVersion<AuthContext, OPS.Migrations.Configuration>());
		}

		public void ConfigureOAuth(IAppBuilder app)
		{
			DataProtectionProvider = app.GetDataProtectionProvider();

			//use a cookie to temporarily store information about a user logging in with a third party login provider
			app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
			OAuthBearerOptions = new OAuthBearerAuthenticationOptions();

			var OAuthServerOptions = new OAuthAuthorizationServerOptions
			{
				AllowInsecureHttp = true,
				TokenEndpointPath = new PathString("/token"),
				AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
				Provider = new SimpleAuthorizationServerProvider(),
				RefreshTokenProvider = new SimpleRefreshTokenProvider()
			};

			// Token Generation
			app.UseOAuthAuthorizationServer(OAuthServerOptions);
			app.UseOAuthBearerAuthentication(OAuthBearerOptions);

			// Google : Create New App
			// https://console.developers.google.com/projectselector/apis/library
			if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings.Get("GoogleClientId")))
			{
				GoogleAuthOptions = new GoogleOAuth2AuthenticationOptions
				{
					ClientId = WebConfigurationManager.AppSettings.Get("GoogleClientId"),
					ClientSecret = WebConfigurationManager.AppSettings.Get("GoogleClientSecret"),
					Provider = new GoogleAuthProvider(),
					CallbackPath = new PathString("/authComplete.html"),
					AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
				};

				GoogleAuthOptions.Scope.Add("email");
				app.UseGoogleAuthentication(GoogleAuthOptions);
			}

			// Facebook : Create New App
			// https://developers.facebook.com/apps
			if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings.Get("FacebookAppId")))
			{
				FacebookAuthOptions = new FacebookAuthenticationOptions
				{
					AppId = WebConfigurationManager.AppSettings.Get("FacebookAppId"),
					AppSecret = WebConfigurationManager.AppSettings.Get("FacebookAppSecret"),
					Scope = { "email" },
					Provider = new FacebookAuthProvider(),
					BackchannelHttpHandler = new FacebookBackChannelHandler(),
					UserInformationEndpoint = "https://graph.facebook.com/v2.8/me?fields=id,name,email"
				};

				app.UseFacebookAuthentication(FacebookAuthOptions);
			}

			// Twitter : Create a new application
			// https://dev.twitter.com/apps
			if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings.Get("TwitterConsumerKey")))
			{
				var twitterOptions = new TwitterAuthenticationOptions
				{
					ConsumerKey = WebConfigurationManager.AppSettings.Get("TwitterConsumerKey"),
					ConsumerSecret = WebConfigurationManager.AppSettings.Get("TwitterClientSecret"),
					Provider = new TwitterAuthenticationProvider
					{
						OnAuthenticated = context =>
						{
							context.Identity.AddClaim(new Claim("urn:twitter:access_token", context.AccessToken, XmlSchemaString, "Twitter"));

							return Task.FromResult(0);
						}
					}
				};

				app.UseTwitterAuthentication(twitterOptions);
			}
		}
	}
}