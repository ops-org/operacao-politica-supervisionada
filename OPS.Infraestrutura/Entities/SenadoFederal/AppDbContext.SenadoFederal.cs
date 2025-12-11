using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura.Entities.Comum;

namespace OPS.Infraestrutura.Entities.SenadoFederal;

public partial class AppDbContext
{
    // Senado Federal (SF) Tables
    public DbSet<Senador> Senadores { get; set; }
    public DbSet<Despesa> DespesasSenado { get; set; }
    public DbSet<DespesaTipo> DespesaTiposSenado { get; set; }
    public DbSet<Mandato> MandatosSenado { get; set; }
    public DbSet<MandatoExercicio> MandatoExerciciosSenado { get; set; }
    public DbSet<MandatoLegislatura> MandatoLegislaturasSenado { get; set; }
    public DbSet<Legislatura> LegislaturasSenado { get; set; }
    public DbSet<MotivoAfastamento> MotivoAfastamentos { get; set; }
    public DbSet<Remuneracao> Remuneracoes { get; set; }
    public DbSet<Vinculo> Vinculos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Cargo> Cargos { get; set; }
    public DbSet<ReferenciaCargo> ReferenciaCargos { get; set; }
    public DbSet<Funcao> Funcoes { get; set; }
    public DbSet<Lotacao> Lotacoes { get; set; }
    public DbSet<TipoFolha> TipoFolhas { get; set; }
}

public static class SenadoFederalConfigurations
{
    public static void ConfigureSenador(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Senador>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
        });
    }

    public static void ConfigureDespesa(this ModelBuilder modelBuilder)
    {
        // Configure Despesa (Senado Federal - Composite Key)
        modelBuilder.Entity<Despesa>(entity =>
        {
            entity.HasKey(e => new { e.IdSenador, e.Id });
            entity.HasOne(e => e.Senador).WithMany(s => s.Despesas).HasForeignKey(e => e.IdSenador);
            entity.HasOne(e => e.DespesaTipo).WithMany(t => t.Despesas).HasForeignKey(e => e.IdDespesaTipo);
        });
    }

    public static void ConfigureMandato(this ModelBuilder modelBuilder)
    {
        // Configure Mandato (Senado Federal - Composite Key)
        modelBuilder.Entity<Mandato>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.IdSenador });
            entity.HasOne(e => e.Senador).WithMany(s => s.Mandatos).HasForeignKey(e => e.IdSenador);
            entity.HasOne(e => e.Estado); //.WithMany(e => e.MandatosSenadoFederal).HasForeignKey(e => e.IdEstado);
        });
    }

    public static void ConfigureMandatoExercicio(this ModelBuilder modelBuilder)
    {
        // Configure MandatoExercicio
        modelBuilder.Entity<MandatoExercicio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(e => e.Senador).WithMany().HasForeignKey(e => e.IdSenador);
            entity.HasOne(e => e.Mandato).WithMany(m => m.MandatoExercicios)
                .HasForeignKey(e => new { e.IdMandato, e.IdSenador });
            entity.HasOne(e => e.MotivoAfastamento).WithMany(m => m.MandatoExercicios).HasForeignKey(e => e.IdMotivoAfastamento);
        });
    }

    public static void ConfigureMandatoLegislatura(this ModelBuilder modelBuilder)
    {
        // Configure MandatoLegislatura (Composite Key)
        modelBuilder.Entity<MandatoLegislatura>(entity =>
        {
            entity.HasKey(e => new { e.IdMandato, e.IdLegislatura });
            entity.HasOne(e => e.Mandato).WithMany(m => m.MandatoLegislaturas)
                .HasForeignKey(e => new { e.IdMandato, e.IdSenador });
            entity.HasOne(e => e.Legislatura).WithMany(l => l.MandatoLegislaturas).HasForeignKey(e => e.IdLegislatura);
        });
    }

    public static void ConfigureRemuneracao(this ModelBuilder modelBuilder)
    {
        // Configure Remuneracao
        modelBuilder.Entity<Remuneracao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(e => e.Vinculo).WithMany(v => v.Remuneracoes).HasForeignKey(e => e.IdVinculo);
            entity.HasOne(e => e.Categoria).WithMany(c => c.Remuneracoes).HasForeignKey(e => e.IdCategoria);
            entity.HasOne(e => e.Cargo).WithMany(c => c.Remuneracoes).HasForeignKey(e => e.IdCargo);
            entity.HasOne(e => e.ReferenciaCargo).WithMany(r => r.Remuneracoes).HasForeignKey(e => e.IdReferenciaCargo);
            entity.HasOne(e => e.Funcao).WithMany(f => f.Remuneracoes).HasForeignKey(e => e.IdSimboloFuncao);
            entity.HasOne(e => e.Lotacao).WithMany(l => l.Remuneracoes).HasForeignKey(e => e.IdLotacao);
            entity.HasOne(e => e.TipoFolha).WithMany(t => t.Remuneracoes).HasForeignKey(e => e.IdTipoFolha);
        });
    }

    public static void ConfigureSenadoFederalEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureSenador();
        modelBuilder.ConfigureDespesa();
        modelBuilder.ConfigureMandato();
        modelBuilder.ConfigureMandatoExercicio();
        modelBuilder.ConfigureMandatoLegislatura();
        modelBuilder.ConfigureRemuneracao();
    }
}
