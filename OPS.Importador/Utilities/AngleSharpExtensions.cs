using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Serilog;

namespace OPS.Importador.Utilities;

public static class AngleSharpExtensions
{
    public static async Task<IDocument> OpenAsyncAutoRetry(this IBrowsingContext context, String address, int totalRetries = 3)
    {
        int retries = 0;
        do
        {
            retries++;
            var doc = await context.OpenAsync(address); // For StatusCode Error, polly will manage the retries

            if (doc.StatusCode == HttpStatusCode.OK)
            {
                var html = doc.ToHtml();
                if (!string.IsNullOrEmpty(html) && html != "<html><head></head><body></body></html>") // Validate empty response
                    return doc;
            }

            Log.Warning($"Try {retries} on {address} - Status Code {doc.StatusCode}"); //  - {doc.ToHtml()}
            Thread.Sleep(1000);
        } while (retries < totalRetries);

        throw new Exception($"Error: {address}");
    }
}