using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OPS.Models;
using OPS.Results;

namespace OPS.WebApi
{
	[RoutePrefix("api/Account")]
	public class AccountController : ApiController
	{
		private readonly AuthRepository _repo = new AuthRepository();

		private IAuthenticationManager Authentication => Request.GetOwinContext().Authentication;

		// POST api/Account/Register
		[AllowAnonymous]
		[Route("Register")]
		public async Task<IHttpActionResult> Register(UserModel userModel)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var sBaseUrl = Url.Content("~/");
			var result = await _repo.RegisterUser(userModel, sBaseUrl);
			if (!result.Succeeded)
				return GetErrorResult(result);

			return Ok();
		}

		// GET api/Account/ExternalLogin
		[OverrideAuthentication]
		[HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
		[AllowAnonymous]
		[Route("ExternalLogin", Name = "ExternalLogin")]
		public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
		{
			var redirectUri = string.Empty;

			if (error != null)
				return BadRequest(Uri.EscapeDataString(error));

			if (User?.Identity == null || !User.Identity.IsAuthenticated)
				return new ChallengeResult(provider, this);

			var redirectUriValidationResult = ValidateClientAndRedirectUri(Request, ref redirectUri);

			if (!string.IsNullOrWhiteSpace(redirectUriValidationResult))
				return BadRequest(redirectUriValidationResult);

			var externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

			if (externalLogin == null)
				return InternalServerError();

			if (externalLogin.LoginProvider != provider)
			{
				Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);
				return new ChallengeResult(provider, this);
			}

			IdentityUser user = await _repo.FindAsync(new UserLoginInfo(externalLogin.LoginProvider, externalLogin.ProviderKey));

			var hasRegistered = user != null;
			if (!hasRegistered)
			{
				var model = new RegisterExternalBindingModel
				{
					UserName = externalLogin.UserName,
					Provider = externalLogin.LoginProvider,
					ExternalAccessToken = externalLogin.ExternalAccessToken,
					Email = externalLogin.Email
				};

				return await RegisterExternal(model);
			}
			redirectUri = string.Format(
				"{0}#external_access_token={1}&provider={2}&haslocalaccount={3}&external_user_name={4}&external_email={5}",
				redirectUri,
				externalLogin.ExternalAccessToken,
				externalLogin.LoginProvider,
				hasRegistered,
				externalLogin.UserName,
				externalLogin.Email);

			return Redirect(redirectUri);
		}

		// POST api/Account/RegisterExternal
		[AllowAnonymous]
		[Route("RegisterExternal")]
		public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var verifiedAccessToken = await VerifyExternalAccessToken(model.Provider, model.ExternalAccessToken);
			if (verifiedAccessToken == null)
				return BadRequest("Invalid Provider or External Access Token");

			var user = await _repo.FindAsync(new UserLoginInfo(model.Provider, verifiedAccessToken.user_id));

			var hasRegistered = user != null;

			if (hasRegistered)
				return BadRequest("External user is already registered");

			user = new ApplicationUser
			{
				UserName = model.Email,
				Email = model.Email,
				FullName = model.UserName
			};

			var sBaseUrl = Url.Content("~/");
			var result = await _repo.CreateAsync(user, sBaseUrl);
			if (!result.Succeeded)
				return GetErrorResult(result);

			var info = new ExternalLoginInfo
			{
				DefaultUserName = model.UserName,
				Login = new UserLoginInfo(model.Provider, verifiedAccessToken.user_id)
			};

			result = await _repo.AddLoginAsync(user.Id, info.Login);
			if (!result.Succeeded)
				return GetErrorResult(result);

			//generate access token response
			var accessTokenResponse = GenerateLocalAccessTokenResponse(user);

			return Ok(accessTokenResponse);
		}

		[AllowAnonymous]
		[HttpGet]
		[Route("ObtainLocalAccessToken")]
		public async Task<IHttpActionResult> ObtainLocalAccessToken(string provider, string externalAccessToken)
		{
			if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(externalAccessToken))
				return BadRequest("Provider or external access token is not sent");

			var verifiedAccessToken = await VerifyExternalAccessToken(provider, externalAccessToken);
			if (verifiedAccessToken == null)
				return BadRequest("Invalid Provider or External Access Token");

			var user = await _repo.FindAsync(new UserLoginInfo(provider, verifiedAccessToken.user_id));

			var hasRegistered = user != null;

			if (!hasRegistered)
				return BadRequest("External user is not registered");

			//generate access token response
			var accessTokenResponse = GenerateLocalAccessTokenResponse(user);

			return Ok(accessTokenResponse);
		}

		[AllowAnonymous]
		[HttpGet]
		[Route("ResetPassword")]
		public async Task<IHttpActionResult> ResetPassword(string value)
		{
			if (string.IsNullOrEmpty(value))
				return BadRequest("E-mail não informado.");

			var user = await _repo.FindByEmailAsync(value);
			if (user == null)
				return BadRequest("Usuário não localidado.");

			var sBaseUrl = Url.Content("~/");
			await _repo.ResetPassword(user, sBaseUrl);

			return Ok();
		}

		[AllowAnonymous]
		[HttpPost]
		[Route("SetPassword")]
		public async Task<IHttpActionResult> SetPassword(PasswordRecoverModel user)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _repo.SetPassword(user);
			if (!result.Succeeded)
				return GetErrorResult(result);

			return Ok();
		}

		[AllowAnonymous]
		[HttpPost]
		[Route("VerifyEmail")]
		public async Task<IHttpActionResult> VerifyEmail(VerifyEmailModel user)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _repo.VerifyEmail(user);
			if (!result.Succeeded)
				return GetErrorResult(result);

			return Ok();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				_repo.Dispose();

			base.Dispose(disposing);
		}

		#region Helpers

		private IHttpActionResult GetErrorResult(IdentityResult result)
		{
			if (result == null)
				return InternalServerError();

			if (!result.Succeeded)
			{
				if (result.Errors != null)
					foreach (var error in result.Errors)
						ModelState.AddModelError("", error);

				if (ModelState.IsValid)
					return BadRequest();

				return BadRequest(ModelState);
			}

			return null;
		}

		private string ValidateClientAndRedirectUri(HttpRequestMessage request, ref string redirectUriOutput)
		{
			Uri redirectUri;

			var redirectUriString = GetQueryString(Request, "redirect_uri");

			if (string.IsNullOrWhiteSpace(redirectUriString))
				return "redirect_uri is required";

			var validUri = Uri.TryCreate(redirectUriString, UriKind.Absolute, out redirectUri);

			if (!validUri)
				return "redirect_uri is invalid";

			var clientId = GetQueryString(Request, "client_id");

			if (string.IsNullOrWhiteSpace(clientId))
				return "client_Id is required";

			var client = _repo.FindClient(clientId);

			if (client == null)
				return string.Format("Client_id '{0}' is not registered in the system.", clientId);

			if (!string.Equals(client.AllowedOrigin, redirectUri.GetLeftPart(UriPartial.Authority),
				StringComparison.OrdinalIgnoreCase))
				return string.Format("The given URL is not allowed by Client_id '{0}' configuration.", clientId);

			redirectUriOutput = redirectUri.AbsoluteUri;

			return string.Empty;
		}

		private string GetQueryString(HttpRequestMessage request, string key)
		{
			var queryStrings = request.GetQueryNameValuePairs();

			if (queryStrings == null) return null;

			var match = queryStrings.FirstOrDefault(keyValue => string.Compare(keyValue.Key, key, true) == 0);

			if (string.IsNullOrEmpty(match.Value)) return null;

			return match.Value;
		}

		private async Task<ParsedExternalAccessToken> VerifyExternalAccessToken(string provider, string accessToken)
		{
			ParsedExternalAccessToken parsedToken = null;

			var verifyTokenEndPoint = "";

			if (provider == "Facebook")
			{
				//You can get it from here: https://developers.facebook.com/tools/accesstoken/
				//More about debug_tokn here: http://stackoverflow.com/questions/16641083/how-does-one-get-the-app-access-token-for-debug-token-inspection-on-facebook
				var appToken = WebConfigurationManager.AppSettings["FacebookClientToken"];
				verifyTokenEndPoint = string.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}",
					accessToken, appToken);
			}
			else if (provider == "Google")
			{
				verifyTokenEndPoint = string.Format("https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={0}", accessToken);
			}
			else
			{
				return null;
			}

			var client = new HttpClient();
			var uri = new Uri(verifyTokenEndPoint);
			var response = await client.GetAsync(uri);

			if (response.IsSuccessStatusCode)
			{
				var content = await response.Content.ReadAsStringAsync();

				dynamic jObj = (JObject) JsonConvert.DeserializeObject(content);

				parsedToken = new ParsedExternalAccessToken();

				if (provider == "Facebook")
				{
					parsedToken.user_id = jObj["data"]["user_id"];
					parsedToken.app_id = jObj["data"]["app_id"];

					if (!string.Equals(Startup.FacebookAuthOptions.AppId, parsedToken.app_id, StringComparison.OrdinalIgnoreCase))
						return null;
				}
				else if (provider == "Google")
				{
					parsedToken.user_id = jObj["user_id"];
					parsedToken.app_id = jObj["audience"];

					if (!string.Equals(Startup.GoogleAuthOptions.ClientId, parsedToken.app_id, StringComparison.OrdinalIgnoreCase))
						return null;
				}
			}

			return parsedToken;
		}

		private JObject GenerateLocalAccessTokenResponse(ApplicationUser user)
		{
			var tokenExpiration = TimeSpan.FromDays(1);

			var identity = new ClaimsIdentity(OAuthDefaults.AuthenticationType);

			identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
			identity.AddClaim(new Claim("FullNane", user.FullName));
			identity.AddClaim(new Claim("UserId", user.Id));

			var props = new AuthenticationProperties
			{
				IssuedUtc = DateTime.UtcNow,
				ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration)
			};

			var ticket = new AuthenticationTicket(identity, props);

			var accessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
			var userRoles = "";
			if (user.Roles.Any())
				userRoles = string.Join(",", user.Roles.Select(x => x.RoleId));

			var tokenResponse = new JObject(
				new JProperty("userName", user.UserName),
				new JProperty("firstName", user.FullName.Split(' ')[0]),
				new JProperty("userRoles", userRoles),
				new JProperty("access_token", accessToken),
				new JProperty("token_type", "bearer"),
				new JProperty("expires_in", tokenExpiration.TotalSeconds.ToString()),
				new JProperty(".issued", ticket.Properties.IssuedUtc.ToString()),
				new JProperty(".expires", ticket.Properties.ExpiresUtc.ToString())
			);


			return tokenResponse;
		}

		private class ExternalLoginData
		{
			public string LoginProvider { get; set; }
			public string ProviderKey { get; set; }
			public string UserName { get; set; }
			public string ExternalAccessToken { get; set; }
			public string Email { get; set; }
			public string ExternalAccessTokenSecret { get; set; }

			public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
			{
				if (identity == null)
					return null;

				var providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

				if (providerKeyClaim == null || string.IsNullOrEmpty(providerKeyClaim.Issuer) ||
				    string.IsNullOrEmpty(providerKeyClaim.Value))
					return null;

				if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
					return null;

				return new ExternalLoginData
				{
					LoginProvider = providerKeyClaim.Issuer,
					ProviderKey = providerKeyClaim.Value,
					UserName = identity.FindFirstValue(ClaimTypes.Name),
					ExternalAccessToken = identity.FindFirstValue(GetAccessTokenKey(providerKeyClaim.Issuer)),
					Email = identity.FindFirstValue(GetEmail(providerKeyClaim.Issuer)),
					ExternalAccessTokenSecret = identity.FindFirstValue(GetAccessTokenSecret(providerKeyClaim.Issuer))
				};
			}

			private static string GetAccessTokenKey(string provider)
			{
				switch (provider)
				{
					case "Facebook":
						return "urn:facebook:access_token";
					case "Twitter":
						return "urn:twitter:access_token";
					default:
						return "ExternalAccessToken";
				}
			}

			private static string GetAccessTokenSecret(string provider)
			{
				switch (provider)
				{
					case "Facebook":
						return "ExternalAccessToken";
					case "Twitter":
						return "urn:twitter:access_token_secret";
					default:
						return "ExternalAccessToken";
				}
			}

			private static string GetEmail(string provider)
			{
				switch (provider)
				{
					case "Facebook":
						return "urn:facebook:email";
					case "Twitter":
						return "urn:twitter:email";
					default:
						return "ExternalEmail";
				}
			}
		}

		#endregion
	}
}