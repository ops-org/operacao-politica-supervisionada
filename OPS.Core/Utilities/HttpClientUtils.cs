using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Context;

namespace OPS.Core.Utilities
{
    public static class HttpClientUtils
    {
        /// https://stackoverflow.com/a/66270371
        public static async Task DownloadFile(this HttpClient client, string url, string FileName, CancellationToken ct = default)
        {
            using (LogContext.PushProperty("Url", url))
            using (var stream = await client.GetStreamAsync(url, ct))
            {
                using (var fileStream = new FileStream(FileName, FileMode.Create))
                {
                    await stream.CopyToAsync(fileStream, ct);
                }
            }
        }
    }
}
