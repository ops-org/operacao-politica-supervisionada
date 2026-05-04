using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using OPS.Infraestrutura.Interceptors;

namespace OPS.Infraestrutura
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        private static readonly DateTimeToUtcInterceptor _dateTimeInterceptor = new DateTimeToUtcInterceptor();

        public AppDbContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "OPS.API"))
                .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            ConfigureOptions(optionsBuilder, configuration.GetConnectionString("AuditoriaContext")!);

            return new AppDbContext(optionsBuilder.Options);
        }

        public static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            optionsBuilder
                .UseNpgsql(connectionString)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseLazyLoadingProxies(false)
                .AddInterceptors(_dateTimeInterceptor);
        }
    }
}
