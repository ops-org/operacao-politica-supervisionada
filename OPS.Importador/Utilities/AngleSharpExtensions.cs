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
    public static async Task<IDocument> OpenAsyncAutoRetry(this IBrowsingContext context, String address)
    {
        var doc = await BrowsingContextExtensions.OpenAsync(context, address);

        if (doc.StatusCode == HttpStatusCode.OK)
        {
            var html = doc.ToHtml();
            if (!string.IsNullOrEmpty(html) && html != "<html><head></head><body></body></html>")
                return doc;
        }

        throw new Exception($"{doc.StatusCode.ToString()}: {address}");
    }
}