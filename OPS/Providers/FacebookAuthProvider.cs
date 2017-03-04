using System;
using System.Diagnostics.Contracts;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.Facebook;

namespace OPS.Providers
{
    public class FacebookAuthProvider : FacebookAuthenticationProvider
    {
        private const string XmlSchemaString = "http://www.w3.org/2001/XMLSchema#string";

        public override Task Authenticated(FacebookAuthenticatedContext context)
        {
            Contract.Requires<ArgumentNullException>(context != null, "context is null");
            Contract.Requires<ArgumentException>(context.AccessToken != null, "context.AccessToken  is null");
            Contract.Requires<ArgumentException>(context.Email != null, "context.Email is null");

            context.Identity.AddClaim(new Claim("access_token", context.AccessToken));
            context.Identity.AddClaim(new Claim(ClaimTypes.Email, context.Email));

            //context.Identity.AddClaim(new System.Security.Claims.Claim("urn:facebook:access_token", context.AccessToken, XmlSchemaString, "Facebook"));
            //foreach (var x in context.User)
            //{
            //    var claimType = string.Format("urn:facebook:{0}", x.Key);
            //    string claimValue = x.Value.ToString();
            //    if (!context.Identity.HasClaim(claimType, claimValue))
            //        context.Identity.AddClaim(new System.Security.Claims.Claim(claimType, claimValue, XmlSchemaString, "Facebook"));

            //}
            return Task.FromResult<object>(null);
        }
    }
}