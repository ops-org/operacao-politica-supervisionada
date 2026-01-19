using System;
using System.Text.Json;
using System.Threading;
using OPS.Core.DTOs;
using RestSharp;

namespace OPS.Core.Utilities
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

            var jsonContent = JsonSerializer.Serialize(message);
            var request = new RestRequest();

            request.AddHeader("Content-Type", "application/json");

            request.AddJsonBody(jsonContent);

            RestResponse response;
            var restClient = new RestClient(url);
            //restClient.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            try
            {
                response = restClient.Post(request);
                if (response.StatusCode.GetHashCode() == 429) //TooManyRequests
                {
                    int retryAfter = 30;
                    try
                    {
                        // { "ok":false,"error_code":429,"description":"Too Many Requests: retry after 35","parameters":{ "retry_after":35} }
                        retryAfter = JsonDocument.Parse(response.Content).RootElement.GetProperty("parameters").GetProperty("retry_after").GetInt32();
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
