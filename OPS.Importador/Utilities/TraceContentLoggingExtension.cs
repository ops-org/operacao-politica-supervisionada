using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace OPS.Importador.Utilities
{
    /// <summary>HTTP request wrapper that logs request/response body.
    /// Use this for debugging issues with third party services.</summary>
    class TraceContentLoggingHandler(HttpMessageHandler innerHandler, ILogger logger) : DelegatingHandler(innerHandler)
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (logger is null || !logger.IsEnabled(LogLevel.Trace))
                return await base.SendAsync(request, cancellationToken); // If not tracing, skip logging

            if (request.Content is not null)
                logger.LogTrace("""
                Request body {URI} 
                {Content}
                """,
                    request.RequestUri,
                    await request.Content.ReadAsStringAsync(cancellationToken));

            var response = await base.SendAsync(request, cancellationToken); // This is disposable, but if we dispose it the ultimate caller can't read the content

            if (response.Content is not null)
                logger.LogTrace("""
                Response {Code} {Status} 
                {Content}
                """,
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    await ReadableResponse(response, cancellationToken));

            return response;
        }

        static async Task<string> ReadableResponse(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            string? contentType = response.Content.Headers.GetValues("Content-Type")?.FirstOrDefault();
            if (contentType == "application/zip")
                return $"ZIP file {response.Content.Headers.GetValues("Content-Length").FirstOrDefault()} bytes";

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

    }

    public static class TraceContentLoggingExtension
    {

        /// <summary>When trace logging is enabled, log request/response bodies.
        /// However, this means that trace logging will be slower.</summary>
        public static IHttpClientBuilder AddTraceContentLogging(this IHttpClientBuilder httpClientBuilder)
        {
            // Get the logger for the named HttpClient
            var sp = httpClientBuilder.Services.BuildServiceProvider();
            var logger = sp.GetService<ILoggerFactory>()?.CreateLogger($"System.Net.Http.HttpClient.{httpClientBuilder.Name}.Content");

            // If trace logging is enabled, add the logging handler
            if (logger?.IsEnabled(LogLevel.Trace) ?? false)
                httpClientBuilder.Services.Configure(
                    httpClientBuilder.Name,
                    (HttpClientFactoryOptions options) =>
                        options.HttpMessageHandlerBuilderActions.Add(b =>
                            b.PrimaryHandler = new TraceContentLoggingHandler(b.PrimaryHandler, logger)
                    ));

            return httpClientBuilder;
        }
    }
}
