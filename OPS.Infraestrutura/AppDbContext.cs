using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OPS.Infraestrutura.Entities.CamaraFederal;
using OPS.Infraestrutura.Entities.Comum;

namespace OPS.Infraestrutura
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                //.LogTo(
                //    Console.WriteLine,
                //    new[] { DbLoggerCategory.Database.Command.Name },
                //    LogLevel.Information
                //)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure all entity contexts
            modelBuilder.ConfigureCamaraFederalEntities();
            modelBuilder.ConfigureAssembleiasLegislativasEntities();
            modelBuilder.ConfigureSenadoFederalEntities();
            modelBuilder.ConfigureComumEntities();
        }
    }
}
