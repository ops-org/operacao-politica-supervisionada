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
            try
            {
                var doc = await context.OpenAsync(address);

                if (doc.StatusCode == HttpStatusCode.OK)
                {
                    var html = doc.ToHtml();
                    if (!string.IsNullOrEmpty(html) && html != "<html><head></head><body></body></html>")
                        return doc;
                }

                Log.Warning($"Try {retries} on {address} - Status Code {doc.StatusCode}"); //  - {doc.ToHtml()}
            }
            catch (Exception ex)
            {
                Log.Warning($"Try {retries} on {address} - {ex.Message}");
            }


            Thread.Sleep(1000);
        } while (retries < totalRetries);

        throw new Exception($"Error: {address}");
    }
}
