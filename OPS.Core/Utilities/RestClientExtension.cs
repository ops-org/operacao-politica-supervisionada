using System;
using System.Threading;
using RestSharp;
using Serilog;

namespace OPS.Core
{
    public static class RestClientExtension
    {
        public static IRestResponse ExecuteWithAutoRetry(this RestClient client, IRestRequest request, int totalRetries = 5)
        {
            try
            {
                var response = client.Execute(request);
                if (!string.IsNullOrEmpty(response.Content))
                    return response;

                if (totalRetries > 0)
                    return ReTry(client, request, totalRetries);

                return response;
            }
            catch (Exception)
            {
                if (totalRetries > 0)
                    return ReTry(client, request, totalRetries);

                throw;
            }
        }

        private static IRestResponse ReTry(RestClient client, IRestRequest request, int totalRetries)
        {
            Log.Verbose("Tentativa {Tentativa} para a url {Url}", totalRetries, client.BaseUrl);

            Thread.Sleep(TimeSpan.FromSeconds(1));
            return ExecuteWithAutoRetry(client, request, totalRetries - 1);
        }
    }
}
