using System.Net;

namespace OPS.Importador.Comum.Utilities
{
    public class RestrictedRedirectFollowingHttpClientHandler : HttpClientHandler
    {
        private static readonly HttpStatusCode[] redirectStatusCodes = new[] {
                     HttpStatusCode.Moved,
                     HttpStatusCode.Redirect,
                     HttpStatusCode.RedirectMethod,
                     HttpStatusCode.TemporaryRedirect,
                     HttpStatusCode.PermanentRedirect
                 };

        private readonly Predicate<HttpResponseMessage> isRedirectAllowed;

        public override bool SupportsRedirectConfiguration { get; }

        public RestrictedRedirectFollowingHttpClientHandler(Predicate<HttpResponseMessage> isRedirectAllowed)
        {
            AllowAutoRedirect = false;
            SupportsRedirectConfiguration = false;
            this.isRedirectAllowed = response =>
            {
                return Array.BinarySearch(redirectStatusCodes, response.StatusCode) >= 0 && isRedirectAllowed.Invoke(response);
            };
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            int redirectCount = 0;
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            while (isRedirectAllowed.Invoke(response)
                && (response.Headers.Location != request.RequestUri || response.StatusCode == HttpStatusCode.RedirectMethod && request.Method != HttpMethod.Get)
                && redirectCount < this.MaxAutomaticRedirections)
            {
                if (response.StatusCode == HttpStatusCode.RedirectMethod)
                {
                    request.Method = HttpMethod.Get;
                }
                request.RequestUri = response.Headers.Location;
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
                ++redirectCount;
            }
            return response;
        }
    }
}
