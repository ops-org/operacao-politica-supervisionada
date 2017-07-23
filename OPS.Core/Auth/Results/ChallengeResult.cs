using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace OPS.Core.Auth.Results
{
	public class ChallengeResult : IHttpActionResult
    {        
        public string LoginProvider { get; set; }
        public HttpRequestMessage Request { get; set; }

        public ChallengeResult(string loginProvider, ApiController controller)
        {
            LoginProvider = loginProvider;
            Request = controller.Request;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
			//TODO
            //Request.GetOwinContext().Authentication.Challenge(LoginProvider);
	        HttpContext.Current.GetOwinContext().Authentication.Challenge(LoginProvider);

			HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
		    {
			    RequestMessage = Request
		    };

	        return Task.FromResult(response);
        }
    }
}