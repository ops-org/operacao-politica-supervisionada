using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura.Entities.AssembleiasLegislativas;
using OPS.Infraestrutura.Entities.CamaraFederal;
using OPS.Infraestrutura.Entities.Comum;
using OPS.Infraestrutura.Entities.SenadoFederal;

namespace OPS.Infraestrutura
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure all entity contexts
            modelBuilder.ConfigureCamaraFederalEntities();
            modelBuilder.ConfigureCamaraLegislativaEntities();
            modelBuilder.ConfigureSenadoFederalEntities();
            modelBuilder.ConfigureComumEntities();
        }
    }
}
