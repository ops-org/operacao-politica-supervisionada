using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OPS.Core.Entity;
using RestSharp;
using System;
using System.Threading;

namespace OPS.Core
{
    public class TelegramApi
    {
        private readonly string _telegramApiToken;

        private string UrlTelegram
        {
            get
            {
                return "https://api.telegram.org/" + _telegramApiToken;
            }
        }

        public TelegramApi(string telegramApiToken)
        {
            _telegramApiToken = telegramApiToken;
        }

        public string SendMessage(TelegramMessage message)
        {
            var url = $"{UrlTelegram}/sendMessage";

            var jsonContent = JsonConvert.SerializeObject(message);
            var request = new RestRequest(Method.POST);

            request.AddHeader("Content-Type", "application/json");

            request.AddJsonBody(jsonContent);

            IRestResponse response;
            var restClient = new RestClient(url);
            restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            try
            {
                response = restClient.Execute(request);
                if (response.StatusCode.GetHashCode() == 429) //TooManyRequests
                {
                    int retryAfter = 30;
                    try
                    {
                        // { "ok":false,"error_code":429,"description":"Too Many Requests: retry after 35","parameters":{ "retry_after":35} }
                        var value = ((Newtonsoft.Json.Linq.JValue)JObject.Parse(response.Content)["parameters"]["retry_after"]).Value;
                        retryAfter = Convert.ToInt32(value);
                    }
                    catch { }

                    //Log.Verbose("Rate limit atingido, aguardando {Aguardar} segundos.", retryAfter);
                    Thread.Sleep(TimeSpan.FromSeconds(retryAfter));
                    response = restClient.Execute(request);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.GetBaseException().Message);

                Thread.Sleep(TimeSpan.FromSeconds(1));
                response = restClient.Execute(request);
            }

            return response.Content;
        }
    }

}
