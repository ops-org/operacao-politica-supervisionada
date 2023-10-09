using System;
using System.Text.Json;
using OPS.Core;
using RestSharp;

namespace OPS.Importador.ALE.Parlamentar
{
    public abstract class ImportadorParlamentarRestApi : ImportadorParlamentarBase
    {

        public ImportadorParlamentarRestApi(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }


        public T RestApiGet<T>(string address)
        {
            var restClient = new RestClient();

            var request = new RestRequest(address);
            request.AddHeader("Accept", "application/json");

            RestResponse resParlamentares = restClient.GetWithAutoRetry(request);
            return JsonSerializer.Deserialize<T>(resParlamentares.Content);
        }
    }
}
