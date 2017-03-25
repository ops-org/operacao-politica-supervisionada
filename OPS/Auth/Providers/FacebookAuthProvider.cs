using System.Threading.Tasks;
using Microsoft.Owin.Security.Facebook;

namespace OPS.Providers
{
	public class FacebookAuthProvider : FacebookAuthenticationProvider
    {
        public override Task Authenticated(FacebookAuthenticatedContext context)
        {
            context.Identity.AddClaim(new System.Security.Claims.Claim("urn:facebook:access_token", context.AccessToken, Startup.XmlSchemaString, "Facebook"));
            foreach (var x in context.User)
            {
                var claimType = string.Format("urn:facebook:{0}", x.Key);
                string claimValue = x.Value.ToString();
                if (!context.Identity.HasClaim(claimType, claimValue))
                    context.Identity.AddClaim(new System.Security.Claims.Claim(claimType, claimValue, Startup.XmlSchemaString, "Facebook"));

            }
            return Task.FromResult(0);
        }
    }
}