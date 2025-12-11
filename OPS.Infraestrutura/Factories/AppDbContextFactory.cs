using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OPS.Infraestrutura.Factories
{
    public class AppDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _connectionString;

        public AppDbContextFactory(IServiceProvider serviceProvider, string connectionString)
        {
            _serviceProvider = serviceProvider;
            _connectionString = connectionString;
        }

        public AppDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
            return new AppDbContext(optionsBuilder.Options);
        }

        public async Task<AppDbContext> CreateDbContextAsync()
        {
            var context = CreateDbContext();
            await context.Database.EnsureCreatedAsync();
            return context;
        }
    }
}
