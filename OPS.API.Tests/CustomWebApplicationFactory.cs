using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPS.API.Configuration;
using OPS.API.Services;
using OPS.Core.Repositories;
using OPS.Infraestrutura;

namespace OPS.API.Tests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
            });

            builder.ConfigureServices(services =>
            {
                // Configure DbContext with test database or mock
                var serviceProvider = services.BuildServiceProvider();
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                // Ensure repositories are registered
                services.AddScoped<DeputadoRepository>();
                services.AddScoped<SenadorRepository>();
                services.AddScoped<DeputadoEstadualRepository>();
                services.AddScoped<FornecedorRepository>();
                services.AddScoped<InicioRepository>();
                services.AddScoped<PartidoRepository>();
                services.AddScoped<EstadoRepository>();

                // Configure cache for tests
                services.AddHybridCache(options =>
                {
                    options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
                    {
                        Expiration = TimeSpan.FromDays(15),
                        LocalCacheExpiration = TimeSpan.FromDays(15),
                    };
                });

                services.AddScoped<IHybridCacheService, HybridCacheService>();
            });
        }
    }
}
