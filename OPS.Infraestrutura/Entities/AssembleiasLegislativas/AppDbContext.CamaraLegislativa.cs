using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura.Entities.AssembleiasLegislativas;

namespace OPS.Infraestrutura;

public partial class AppDbContext
{
    // Câmara Legislativa (CL) Tables
    public DbSet<Deputado> Deputados { get; set; }
    public DbSet<DeputadoCampeaoGasto> DeputadoCampeaoGastos { get; set; }
    public DbSet<Despesa> DespesasAssembleias { get; set; }
    public DbSet<DespesaEspecificacao> DespesaEspecificacoes { get; set; }
    public DbSet<DespesaResumoMensal> DespesaResumosMensais { get; set; }
    public DbSet<DespesaTipo> DespesaTipos { get; set; }
}

public static class AssembleiasLegislativasConfigurations
{
    public static void ConfigureDeputadoEstadual(this ModelBuilder modelBuilder)
    {
        // Configure Deputado
        modelBuilder.Entity<Deputado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(e => e.Partido); //.WithMany(p => p.Deputados).HasForeignKey(e => e.IdPartido);
            entity.HasOne(e => e.Estado); //.WithMany(e => e.Deputados).HasForeignKey(e => e.IdEstado);
        });
    }

    public static void ConfigureDeputadoEstadualCampeaoGasto(this ModelBuilder modelBuilder)
    {
        // Configure DeputadoCampeaoGasto
        modelBuilder.Entity<DeputadoCampeaoGasto>(entity =>
        {
            entity.HasKey(e => e.IdDeputado);
            entity.HasOne(e => e.Deputado).WithMany().HasForeignKey(e => e.IdDeputado);
        });
    }

    public static void ConfigureDespesaDeputadoEstadual(this ModelBuilder modelBuilder)
    {
        // Configure Despesa
        modelBuilder.Entity<Despesa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(e => e.Deputado).WithMany(d => d.Despesas).HasForeignKey(e => e.IdDeputado);
            entity.HasOne(e => e.DespesaTipo).WithMany(t => t.Despesas).HasForeignKey(e => e.IdDespesaTipo);
            entity.HasOne(e => e.Fornecedor).WithMany(f => f.DespesasAssembleias).HasForeignKey(e => e.IdFornecedor);
        });
    }

    public static void ConfigureDespesaEspecificacaoDeputadoEstadual(this ModelBuilder modelBuilder)
    {
        // Configure DespesaEspecificacao
        modelBuilder.Entity<DespesaEspecificacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.DespesaTipo).WithMany(t => t.DespesaEspecificacoes).HasForeignKey(e => e.IdDespesaTipo);
        });
    }

    public static void ConfigureDespesaResumoMensalDeputadoEstadual(this ModelBuilder modelBuilder)
    {
        // Configure DespesaResumoMensal
        modelBuilder.Entity<DespesaResumoMensal>(entity =>
        {
            entity.HasKey(e => new { e.IdDeputado, e.Ano, e.Mes });
            entity.HasOne(e => e.Deputado).WithMany().HasForeignKey(e => e.IdDeputado);
        });
    }

    public static void ConfigureDespesaTipoDeputadoEstadual(this ModelBuilder modelBuilder)
    {
        // Configure DespesaTipo
        modelBuilder.Entity<DespesaTipo>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }

    // Master method to apply all configurations
    public static void ConfigureAssembleiasLegislativasEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureDeputadoEstadual();
        modelBuilder.ConfigureDeputadoEstadualCampeaoGasto();
        modelBuilder.ConfigureDespesaDeputadoEstadual();
        modelBuilder.ConfigureDespesaEspecificacaoDeputadoEstadual();
        modelBuilder.ConfigureDespesaResumoMensalDeputadoEstadual();
        modelBuilder.ConfigureDespesaTipoDeputadoEstadual();
    }
}
