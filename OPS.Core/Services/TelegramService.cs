using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using OPS.Core.DTOs;

namespace OPS.Core.Services
{
    public class TelegramService
    {
        private readonly HttpClient httpClient;
        private readonly string _telegramApiToken;

        public TelegramService(HttpClient httpClient, string telegramApiToken)
        {
            _telegramApiToken = telegramApiToken;
            this.httpClient = httpClient;
        }

        public async Task<string> SendMessage(TelegramMessage message)
        {
            var urlTelegram = $"https://api.telegram.org/bot{_telegramApiToken}/sendMessage";
            var response = await httpClient.PostAsJsonAsync(urlTelegram, message);

            return await response.Content.ReadAsStringAsync();
        }
    }

}
