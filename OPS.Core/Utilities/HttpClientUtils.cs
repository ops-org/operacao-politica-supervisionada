using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OPS.Core.Utilities
{
    public static class HttpClientUtils
    {
        /// https://stackoverflow.com/a/66270371
        public static async Task DownloadFile(this HttpClient client, string url, string FileName, CancellationToken ct = default)
        {
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
