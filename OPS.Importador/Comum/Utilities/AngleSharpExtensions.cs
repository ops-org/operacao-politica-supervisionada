using System.Net;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Io.Network;
using OPS.Core.Exceptions;
using Serilog;

namespace OPS.Importador.Comum.Utilities;

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
            IDocument doc;

            var baseDelaySeconds = Math.Pow(3, retries);
            var jitterSeconds = Random.Shared.NextDouble() * baseDelaySeconds;
            try
            {
                doc = await context.OpenAsync(address); // For StatusCode Error, polly will manage the retries
            }
            catch (NullReferenceException ex) when (ex.Source == "AngleSharp")
            {
                if (retries >= totalRetries) throw;

                Log.Warning(ex, "AngleSharp NRE occurred. Try {Retries} of {MaxRetries} on {Address}. Wait for {WaitSeconds} seconds.", retries, totalRetries, address, baseDelaySeconds + jitterSeconds);
                await Task.Delay(TimeSpan.FromSeconds(baseDelaySeconds + jitterSeconds));
                continue;
            }

            if (doc.StatusCode == HttpStatusCode.NotFound)
                return doc;

            if (doc.StatusCode == HttpStatusCode.OK)
            {
                var html = doc.DocumentElement.OuterHtml;
                if (!doc.Url.Contains("error") && !string.IsNullOrEmpty(html))
                {
                    if (html.StartsWith("<html><head></head><body>") && !html.StartsWith("<html><head></head><body><option value=\"\"></option>"))
                    {
                        Log.Warning("Empty response on request {Address}. Try {Retries} of {MaxRetries}.", address, retries, totalRetries);
                    }

                    if (!html.StartsWith("<html><head></head><body>") || html.StartsWith("<html><head></head><body><option value=\"\"></option>")) // Validate empty response and page error redirect
                        return doc;
                }
            }

            if (doc.BaseUri != address)
            {
                if (!address.Contains("camara.leg.br/transparencia/recursos-humanos/remuneracao"))
                    throw new BusinessException($"Prevent auto redirect on request {address} to {doc.BaseUri}");

                // Quando há redirect para pagina de erro, é necessario esperar mais tempo que o normal.
                Log.Information("Try {Retries} of {MaxRetries} on {Address}. Wait for {WaitSeconds} seconds.", retries, totalRetries, address, 60);
                await Task.Delay(TimeSpan.FromMinutes(1));
                continue;
            }

            
            Log.Information("Try {Retries} of {MaxRetries} on {Address}. Wait for {WaitSeconds} seconds.", retries, totalRetries, address, baseDelaySeconds + jitterSeconds);
            await Task.Delay(TimeSpan.FromSeconds(baseDelaySeconds + jitterSeconds));

        } while (retries < totalRetries);

        throw new Exception($"Error Get Request: {address}");
    }

    public static async Task<IDocument> SubmitAsyncAutoRetry(this IHtmlFormElement form, IDictionary<string, string> fields, bool createMissing = false, int totalRetries = 3)
    {
        int retries = 0;
        do
        {
            retries++;
            IDocument doc;
            try
            {
                doc = await form.SubmitAsync(fields, createMissing); // For StatusCode Error, polly will manage the retries
            }
            catch (NullReferenceException ex) when (ex.Source == "AngleSharp")
            {
                if (retries >= totalRetries) throw;

                var waitExSeconds = Math.Pow(2, retries);
                Log.Warning(ex, "AngleSharp NRE occurred. Try {Retries} of {MaxRetries} on {Address}. Wait for {WaitSeconds} seconds.", retries, totalRetries, form.BaseUri.ToString(), waitExSeconds);
                await Task.Delay(TimeSpan.FromSeconds(waitExSeconds));
                continue;
            }

            if (doc.StatusCode == HttpStatusCode.OK)
            {
                var html = doc.ToHtml();
                if (!string.IsNullOrEmpty(html) && html != "<html><head></head><body></body></html>") // Validate empty response and page error redirect
                    return doc;
            }

            var waitSeconds = Math.Pow(2, retries);
            Log.Information("Try {Retries} of {MaxRetries} on {Address}. Wait for {WaitSeconds} seconds.", retries, totalRetries, form.BaseUri.ToString(), waitSeconds);
            await Task.Delay(TimeSpan.FromSeconds(waitSeconds));

        } while (retries < totalRetries);

        throw new Exception($"Error Submit Form: {form.BaseUri.ToString()}");
    }
}