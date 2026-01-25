using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura.Entities.AssembleiasLegislativas;

namespace OPS.Infraestrutura;

public partial class AppDbContext
{
    // Câmara Legislativa (CL) Tables
    public DbSet<DeputadoEstadual> DeputadosEstaduais { get; set; }
    public DbSet<DeputadoCampeaoGastoAssembleias> DeputadoCampeaoGastos { get; set; }
    public DbSet<DespesaAssembleias> DespesasAssembleias { get; set; }
    public DbSet<DespesaEspecificacaoAssembleias> DespesaEspecificacoes { get; set; }
    public DbSet<DespesaResumoMensalAssembleias> DespesaResumosMensais { get; set; }
    public DbSet<DespesaTipoAssembleias> DespesaTipos { get; set; }
}

public static class AssembleiasLegislativasConfigurations
{
    public static void ConfigureDeputadoEstadual(this ModelBuilder modelBuilder)
    {
        // Configure Deputado
        modelBuilder.Entity<DeputadoEstadual>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(e => e.Partido).WithMany(p => p.DeputadosEstaduais).HasForeignKey(e => e.IdPartido);
            entity.HasOne(e => e.Estado).WithMany(e => e.DeputadosEstaduais).HasForeignKey(e => e.IdEstado);
            entity.ToTable("cl_deputado", "assembleias");
        });
    }

    public static void ConfigureDeputadoEstadualCampeaoGasto(this ModelBuilder modelBuilder)
    {
        // Configure DeputadoCampeaoGasto
        modelBuilder.Entity<DeputadoCampeaoGastoAssembleias>(entity =>
        {
            entity.HasKey(e => e.IdDeputado);
            entity.HasOne(e => e.Deputado).WithMany().HasForeignKey(e => e.IdDeputado);
            entity.ToTable("cl_deputado_campeao_gasto", "assembleias");
        });
    }

    public static void ConfigureDespesaDeputadoEstadual(this ModelBuilder modelBuilder)
    {
        // Configure Despesa
        modelBuilder.Entity<DespesaAssembleias>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(e => e.Deputado).WithMany(d => d.Despesas).HasForeignKey(e => e.IdDeputado);
            entity.HasOne(e => e.DespesaTipo).WithMany(t => t.Despesas).HasForeignKey(e => e.IdDespesaTipo);
            entity.HasOne(e => e.DespesaEspecificacao).WithMany(es => es.Despesas).HasForeignKey(e => e.IdDespesaEspecificacao);
            entity.HasOne(e => e.Fornecedor).WithMany(f => f.DespesasAssembleias).HasForeignKey(e => e.IdFornecedor);
            entity.ToTable("cl_despesa", "assembleias");
        });
    }

    public static void ConfigureDespesaEspecificacaoDeputadoEstadual(this ModelBuilder modelBuilder)
    {
        // Configure DespesaEspecificacao
        modelBuilder.Entity<DespesaEspecificacaoAssembleias>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(e => e.DespesaTipo).WithMany(t => t.DespesaEspecificacoes).HasForeignKey(e => e.IdDespesaTipo);
            entity.ToTable("cl_despesa_especificacao", "assembleias");
        });
    }

    public static void ConfigureDespesaResumoMensalDeputadoEstadual(this ModelBuilder modelBuilder)
    {
        // Configure DespesaResumoMensal
        modelBuilder.Entity<DespesaResumoMensalAssembleias>(entity =>
        {
            entity.HasKey(e => new { e.Ano, e.Mes });
            entity.ToTable("cl_despesa_resumo_mensal", "assembleias");
        });
    }

    public static void ConfigureDespesaTipoDeputadoEstadual(this ModelBuilder modelBuilder)
    {
        // Configure DespesaTipo
        modelBuilder.Entity<DespesaTipoAssembleias>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.ToTable("cl_despesa_tipo", "assembleias");
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
