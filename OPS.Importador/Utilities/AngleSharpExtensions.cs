using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Org.BouncyCastle.Utilities.Net;
using Serilog;

namespace OPS.Importador.Utilities;

public static class AngleSharpExtensions
{
    public static async Task<IDocument> OpenAsyncAutoRetry(this IBrowsingContext context, String address, int totalRetries = 5)
    {
        int retries = 0;
        do
        {
            retries++;
            var doc = await context.OpenAsync(address); // For StatusCode Error, polly will manage the retries

            if (doc.StatusCode == HttpStatusCode.OK)
            {
                var html = doc.ToHtml();
                if (!doc.Url.Contains("error") && !string.IsNullOrEmpty(html) && html != "<html><head></head><body></body></html>") // Validate empty response and page error redirect
                    return doc;
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