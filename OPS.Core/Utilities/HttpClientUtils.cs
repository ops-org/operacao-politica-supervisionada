using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OPS.Core
{
    public static class HttpClientUtils
    {
        /// https://stackoverflow.com/a/66270371
        public static async Task DownloadFile(this HttpClient client, string url, string FileName)
        {
            using (var stream = await client.GetStreamAsync(url))
            {
                using (var fileStream = new FileStream(FileName, FileMode.CreateNew))
                {
                    await stream.CopyToAsync(fileStream);
                }
            }
        }
    }
}
