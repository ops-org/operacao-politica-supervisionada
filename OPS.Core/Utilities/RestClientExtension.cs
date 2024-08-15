using System;
using System.Threading;
using RestSharp;
using Serilog;

namespace OPS.Core
{
    public static class RestClientExtension
    {
        public static RestResponse GetWithAutoRetry(this RestClient client, RestRequest request, int totalRetries = 5)
        {
            // Realizando retry pelo polly.
            return client.Get(request);
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //Log.Verbose("Chamando URL: {Url}", request.Resource);

            //try
            //{
            //    var response = client.Get(request);
            //    if (!string.IsNullOrEmpty(response.Content))
            //        return response;

            //    if (totalRetries > 0)
            //        return ReTry(client, request, totalRetries);

            //    return response;
            //}
            //catch (Exception)
            //{
            //    if (totalRetries > 0)
            //        return ReTry(client, request, totalRetries);

            //    throw;
            //}
            //finally
            //{
            //    watch.Stop();
            //    Log.Verbose("Requisição processada em {TimeElapsed:c}", watch.Elapsed);
            //}
        }

        //private static RestResponse ReTry(RestClient client, RestRequest request, int totalRetries)
        //{
        //    Log.Verbose("Tentativa {Tentativa} para a url {Url}", totalRetries, request.Resource);

        //    Thread.Sleep(TimeSpan.FromMinutes(1));
        //    return GetWithAutoRetry(client, request, totalRetries - 1);
        //}
    }
}
