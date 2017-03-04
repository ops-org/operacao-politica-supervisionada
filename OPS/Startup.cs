using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
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
        private const string XmlSchemaString = "http://www.w3.org/2001/XMLSchema#string";
        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
        public static GoogleOAuth2AuthenticationOptions googleAuthOptions { get; private set; }
        public static FacebookAuthenticationOptions facebookAuthOptions { get; private set; }
        public static Func<UserManager<AppUser>> UserManagerFactory { get; private set; }

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

            //Configure Google External Login
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings.Get("GoogleClientId")))
            {
                googleAuthOptions = new GoogleOAuth2AuthenticationOptions
                {
                    ClientId = WebConfigurationManager.AppSettings["GoogleClientId"],
                    ClientSecret = WebConfigurationManager.AppSettings["GoogleClientSecret"],
                    Provider = new GoogleAuthProvider()
                };

                app.UseGoogleAuthentication(googleAuthOptions);
            }

            // Facebook : Create New App
            // https://developers.facebook.com/apps
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings.Get("FacebookAppId")))
            {
                var facebookOptions = new FacebookAuthenticationOptions
                {
                    AppId = WebConfigurationManager.AppSettings.Get("FacebookAppId"),
                    AppSecret = WebConfigurationManager.AppSettings.Get("FacebookAppSecret"),
                    Scope = {"email"},
                    Provider = new FacebookAuthProvider(),
                    BackchannelHttpHandler = new FacebookBackChannelHandler(),
                    UserInformationEndpoint = "https://graph.facebook.com/v2.4/me?fields=id,name,email"
                };

                app.UseFacebookAuthentication(facebookOptions);
            }

            // Twitter : Create a new application
            // https://dev.twitter.com/apps
            if (!string.IsNullOrEmpty(WebConfigurationManager.AppSettings.Get("TwitterConsumerKey")))
            {
                var twitterOptions = new TwitterAuthenticationOptions
                {
                    ConsumerKey = WebConfigurationManager.AppSettings.Get("TwitterConsumerKey"),
                    ConsumerSecret = WebConfigurationManager.AppSettings.Get("TwitterConsumerSecret"),
                    Provider = new TwitterAuthenticationProvider
                    {
                        OnAuthenticated = context =>
                        {
                            context.Identity.AddClaim(new Claim("urn:twitter:access_token", context.AccessToken,
                                XmlSchemaString, "Twitter"));
                            return Task.FromResult(0);
                        }
                    }
                };

                app.UseTwitterAuthentication(twitterOptions);
            }

            UserManagerFactory = () =>
            {
                var usermanager = new UserManager<AppUser>(new UserStore<AppUser>(new AuthContext()));
                // allow alphanumeric characters in username
                usermanager.UserValidator = new UserValidator<AppUser>(usermanager)
                {
                    AllowOnlyAlphanumericUserNames = false
                };

                return usermanager;
            };
        }
    }
}