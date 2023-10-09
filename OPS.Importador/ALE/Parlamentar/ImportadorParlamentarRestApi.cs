using System;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
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
