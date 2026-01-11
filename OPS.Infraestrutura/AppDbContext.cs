using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using OPS.Infraestrutura.Entities.TSE;
using OPS.Infraestrutura.Interceptors;

namespace OPS.Infraestrutura;

public partial class AppDbContext : DbContext
{
    private static readonly DateTimeToUtcInterceptor _dateTimeInterceptor = new DateTimeToUtcInterceptor();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Always add the interceptor, even if options are already configured
        //if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                //.ConfigureWarnings(w => w.Ignore(RelationalEventId.CommandExecuted).Throw(RelationalEventId.MultipleCollectionIncludeWarning))
                //.LogTo(_ => { }, LogLevel.None)
                .UseLazyLoadingProxies(false);
        }
        
        // Ensure the interceptor is always present
        optionsBuilder.AddInterceptors(_dateTimeInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Register unaccent function for EF Core
        modelBuilder.HasDbFunction(typeof(DbFunctions).GetMethod(nameof(DbFunctions.Unaccent)))
            .HasName("unaccent");

        // Configure all entity contexts by area
        modelBuilder.ConfigureCommonEntities();
        modelBuilder.ConfigureCamaraFederalEntities();
        modelBuilder.ConfigureSenadoFederalEntities();
        modelBuilder.ConfigureAssembleiasLegislativasEntities();
        modelBuilder.ConfigureFornecedorEntities();
        modelBuilder.ConfigureTSEEntities();
        modelBuilder.ConfigureTempEntities();
    }

    /// <summary>
    /// Initializes the database extensions required for the application.
    /// This ensures that PostgreSQL extensions like 'unaccent' are available.
    /// </summary>
    public void InitializeDatabaseExtensions()
    {
        try
        {
            Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS unaccent;");
        }
        catch (Exception ex)
        {
            // Log but don't throw - extension may already exist or user may not have permissions
            System.Diagnostics.Debug.WriteLine($"Warning: Could not create unaccent extension: {ex.Message}");
        }
    }
}

public static class DbFunctions
{
    public static string Unaccent(string text)
    {
        throw new NotSupportedException("For EF Core queries only");
    }
}