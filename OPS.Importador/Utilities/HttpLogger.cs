using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;

namespace OPS.Importador.Utilities
{
    public class HttpLogger : IHttpClientLogger
    {
        private readonly ILogger<HttpLogger> _logger;

        public HttpLogger(ILogger<HttpLogger> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public object? LogRequestStart(HttpRequestMessage request)
        {
            //var headers = new StringBuilder();
            //foreach (var (key, value) in request.Headers)
            //    headers.AppendLine($"{key} = {string.Join(",", value)}");

            string content = "";
            if (request.Content != null)
            {
                content = "?" + request.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                //foreach (var (key, value) in request.Content.Headers)
                //    headers.AppendLine($"{key} = {string.Join(",", value)}");
            }

            //if (!_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Trace))
            //    _logger.LogDebug("""
            //            Request {Method} {URI} 
            //            Headers
            //            {Headers}Content 
            //            {Content}
            //            """,
            //    request.Method,
            //    request.RequestUri,
            //    headers.ToString(),
            //    content);
            //else
            _logger.LogDebug("Request {Method} {URI}{Content}", request.Method, request.RequestUri, content);

            return null;
        }

        public void LogRequestStop(object? context, HttpRequestMessage request, HttpResponseMessage response, TimeSpan elapsed)
        {
            var logLevel = LogLevel.Debug;
            if (elapsed.TotalMilliseconds > 10_000) logLevel = LogLevel.Warning;
            else if (elapsed.TotalMilliseconds > 5_000) logLevel = LogLevel.Information;

            string content = "";
            if (request.Content != null)
            {
                content = "?" + request.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            }

            _logger.Log(logLevel, "Received {StatusCode} {ReasonPhrase} after {TotalMilliseconds} ms from {Method} {URI}{Content}",
                response.StatusCode.GetHashCode(), response.ReasonPhrase, elapsed.TotalMilliseconds.ToString("F1"), request.Method, request.RequestUri, content);

            //if (response.Content != null)
            //    _logger.LogInformation("""
            //        Response {Code} {Status} 
            //        {Content}
            //        """,
            //       (int)response.StatusCode,
            //       response.ReasonPhrase,
            //       ReadableResponse(response).GetAwaiter().GetResult());
        }

        static async Task<string> ReadableResponse(HttpResponseMessage response)
        {
            string contentType = response.Content.Headers.GetValues("Content-Type")?.FirstOrDefault() ?? string.Empty;
            if (contentType == "application/zip")
                return $"ZIP file {response.Content.Headers.GetValues("Content-Length").FirstOrDefault()} bytes";

            return await response.Content.ReadAsStringAsync();
        }

        public void LogRequestFailed(
            object? context,
            HttpRequestMessage request,
            HttpResponseMessage? response,
            Exception exception,
            TimeSpan elapsed)
        {
            _logger.LogError(
                exception,
                "Request towards '{RequestUri}/{PathAndQuery}' failed after {TotalMilliseconds} ms",
                request.RequestUri?.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped),
                request.RequestUri!.PathAndQuery,
                elapsed.TotalMilliseconds.ToString("F1"));
        }
    }
}
