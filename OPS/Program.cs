using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace OPS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateWebHostBuilder(args).Build().Run();
            CreateWebHostBuilder(args).Build().RunAsync().GetAwaiter().GetResult();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseContentRoot("/var/www/ops.net.br/")
                //.UseSetting("detailedErrors", "true")
                //.UseIISIntegration()
                .UseStartup<Startup>()
                .CaptureStartupErrors(true)
                .UseUrls("http://*:5000");
    }
}
