using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace OPS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>();

#if !DEBUG
                    webBuilder.UseUrls("http://*:5200", "https://*:5201");
#endif
                });
    }
}
