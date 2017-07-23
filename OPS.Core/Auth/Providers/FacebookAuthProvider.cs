using System.Threading.Tasks;
using Microsoft.Owin.Security.Facebook;

namespace OPS.Core.Auth.Providers
{
	public class FacebookAuthProvider : FacebookAuthenticationProvider
    {
	    public const string XmlSchemaString = "http://www.w3.org/2001/XMLSchema#string";

		public override Task Authenticated(FacebookAuthenticatedContext context)
        {
            context.Identity.AddClaim(new System.Security.Claims.Claim("urn:facebook:access_token", context.AccessToken, XmlSchemaString, "Facebook"));
            foreach (var x in context.User)
            {
                var claimType = string.Format("urn:facebook:{0}", x.Key);
                string claimValue = x.Value.ToString();
                if (!context.Identity.HasClaim(claimType, claimValue))
                    context.Identity.AddClaim(new System.Security.Claims.Claim(claimType, claimValue, XmlSchemaString, "Facebook"));

            }
            return Task.FromResult(0);
        }
    }
}