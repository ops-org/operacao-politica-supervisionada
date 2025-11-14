using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io.Network;
using OPS.Core;
using Serilog;

namespace OPS.Importador.Utilities;

public static class AngleSharpExtensions
{
    public static IBrowsingContext CreateAngleSharpContext(this HttpClient httpClient)
    {
        var configuration = AngleSharp.Configuration.Default
            .With(new HttpClientRequester(httpClient))
            .WithDefaultLoader()
            .WithDefaultCookies()
            .WithCulture("pt-BR");

        return BrowsingContext.New(configuration);
    }


    public static async Task<IDocument> OpenAsyncAutoRetry(this IBrowsingContext context, String address, int totalRetries = 5)
    {
        int retries = 0;
        do
        {
            retries++;
            var doc = await context.OpenAsync(address); // For StatusCode Error, polly will manage the retries

            if (doc.StatusCode == HttpStatusCode.NotFound)
                return doc;

            if (doc.StatusCode == HttpStatusCode.OK)
            {
                var html = doc.DocumentElement.OuterHtml;
                if (!doc.Url.Contains("error") &&
                    !string.IsNullOrEmpty(html) &&
                    html != "<html><head></head><body></body></html>" &&
                    !html.StartsWith("<html><head></head><body>")) // Validate empty response and page error redirect
                    return doc;
            }

            if (doc.BaseUri != address)
            {
                if (!address.Contains("camara.leg.br/transparencia/recursos-humanos/remuneracao"))
                    throw new BusinessException($"Prevent auto redirect on request {address} to {doc.BaseUri}");

                // Quando há redirect para pagina de erro, é necessario esperar mais tempo que o normal.
                Log.Information("Try {Retries} of {MaxRetries} on {Address}. Wait for {WaitSeconds} seconds.", retries, totalRetries, address, 60);
                Thread.Sleep(TimeSpan.FromMinutes(1));
                continue;
            }

            var waitSeconds = Math.Pow(2, retries);
            Log.Information("Try {Retries} of {MaxRetries} on {Address}. Wait for {WaitSeconds} seconds.", retries, totalRetries, address, waitSeconds);
            Thread.Sleep(TimeSpan.FromSeconds(waitSeconds));

        } while (retries < totalRetries);

        throw new Exception($"Error Get Request: {address}");
    }

    public static async Task<IDocument> SubmitAsyncAutoRetry(this IHtmlFormElement form, IDictionary<string, string> fields, bool createMissing = false, int totalRetries = 3)
    {
        int retries = 0;
        do
        {
            retries++;
            var doc = await form.SubmitAsync(fields, createMissing); // For StatusCode Error, polly will manage the retries

            if (doc.StatusCode == HttpStatusCode.OK)
            {
                var html = doc.ToHtml();
                if (!string.IsNullOrEmpty(html) && html != "<html><head></head><body></body></html>") // Validate empty response and page error redirect
                    return doc;
            }

            var waitSeconds = Math.Pow(2, retries);
            Log.Information("Try {Retries} of {MaxRetries} on {Address}. Wait for {WaitSeconds} seconds.", retries, totalRetries, form.BaseUri.ToString(), waitSeconds);
            Thread.Sleep(TimeSpan.FromSeconds(waitSeconds));

        } while (retries < totalRetries);

        throw new Exception($"Error Submit Form: {form.BaseUri.ToString()}");
    }
}