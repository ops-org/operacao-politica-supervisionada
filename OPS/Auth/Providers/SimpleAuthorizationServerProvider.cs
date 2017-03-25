using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using OPS.Entities;

namespace OPS.Providers
{
	public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
	{
		public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
		{

			string clientId = string.Empty;
			string clientSecret = string.Empty;
			Client client = null;

			if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
			{
				context.TryGetFormCredentials(out clientId, out clientSecret);
			}

			if (context.ClientId == null)
			{
				//Remove the comments from the below line context.SetError, and invalidate context 
				//if you want to force sending clientId/secrects once obtain access tokens. 
				context.Validated();
				//context.SetError("invalid_clientId", "ClientId should be sent.");
				return Task.FromResult<object>(null);
			}

			using (AuthRepository _repo = new AuthRepository())
			{
				client = _repo.FindClient(context.ClientId);
			}

			if (client == null)
			{
				context.SetError("invalid_clientId", string.Format("Client '{0}' is not registered in the system.", context.ClientId));
				return Task.FromResult<object>(null);
			}

			if (!client.Active)
			{
				context.SetError("invalid_clientId", "Client is inactive.");
				return Task.FromResult<object>(null);
			}

			context.OwinContext.Set<string>("as:clientAllowedOrigin", client.AllowedOrigin);
			context.OwinContext.Set<string>("as:clientRefreshTokenLifeTime", client.RefreshTokenLifeTime.ToString());

			context.Validated();
			return Task.FromResult<object>(null);
		}

		public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
		{
			var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");

			if (allowedOrigin == null) allowedOrigin = "*";

			context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });

			ApplicationUser user = null;
			using (AuthRepository _repo = new AuthRepository())
			{
				user = await _repo.FindUser(context.UserName, context.Password);
				if (user == null)
				{
					context.SetError("invalid_grant", "O e-mail ou senha esta incorreto.");
					return;
				}
			}

			var identity = new ClaimsIdentity(context.Options.AuthenticationType);
			identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
			identity.AddClaim(new Claim("FullNane", user.FullName));
			identity.AddClaim(new Claim("UserId", user.Id));

			var props = new AuthenticationProperties(new Dictionary<string, string>
					 {
						  { "as:client_id", (context.ClientId == null) ? string.Empty : context.ClientId },
						  { "userName", context.UserName },
						  { "firstName", user.FullName.Split(' ')[0] }
					 });

			var ticket = new AuthenticationTicket(identity, props);
			context.Validated(ticket);

		}

		public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
		{
			var originalClient = context.Ticket.Properties.Dictionary["as:client_id"];
			var currentClient = context.ClientId;

			if (originalClient != currentClient)
			{
				context.SetError("invalid_clientId", "Refresh token is issued to a different clientId.");
				return Task.FromResult<object>(null);
			}

			// Change auth ticket for refresh token requests
			var newIdentity = new ClaimsIdentity(context.Ticket.Identity);

			var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
			context.Validated(newTicket);

			return Task.FromResult<object>(null);
		}

		public override Task TokenEndpoint(OAuthTokenEndpointContext context)
		{
			foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
			{
				context.AdditionalResponseParameters.Add(property.Key, property.Value);
			}

			return Task.FromResult<object>(null);
		}

	}
}

