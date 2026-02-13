using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura.Entities.TSE;
using OPS.Infraestrutura.Interceptors;

namespace OPS.Infraestrutura;

public partial class AppDbContext : DbContext
{
    private static readonly DateTimeToUtcInterceptor _dateTimeInterceptor = new DateTimeToUtcInterceptor();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        //Database.ExecuteSqlRaw("SET max_stack_depth = '6MB'");
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
                //.LogTo(
                //    Console.WriteLine,
                //    new[] { RelationalEventId.CommandExecuted },  // Only command execution
                //    LogLevel.Information  // Only errors
                //)
                //.LogTo(
                //    action: log =>
                //    {
                //        // Suppress specific errors
                //        if (log.Contains("destructuring"))
                //        {
                //            return;  // Don't log
                //        }

                //        Console.WriteLine($"[EF Core] {log}");
                //    },
                //    filter: (eventId, level) => level >= LogLevel.Information
                //)
                .UseLazyLoadingProxies(false);
        }

        // Ensure the interceptor is always present
        optionsBuilder.AddInterceptors(_dateTimeInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Register unaccent function for EF Core
        modelBuilder.HasDbFunction(typeof(PgFunctions).GetMethod(nameof(PgFunctions.Unaccent)))
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
}

public static class PgFunctions
{
    [DbFunction("unaccent", IsBuiltIn = true)]
    public static string Unaccent(string text)
    {
        throw new NotSupportedException("For EF Core queries only");
    }
}