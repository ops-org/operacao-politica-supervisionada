using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace OPS.API.Tests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();

                    webBuilder.ConfigureAppConfiguration((context, config) =>
                    {
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                        config.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
                    });
                });

            return builder;
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            });
        }
    }
}
