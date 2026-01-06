using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura.Entities.Comum;
using OPS.Infraestrutura.Entities.SenadoFederal;

namespace OPS.Infraestrutura;

public partial class AppDbContext
{
    // Senado Federal (SF) Tables
    public DbSet<Senador> Senadores { get; set; }
    public DbSet<Despesa> DespesasSenado { get; set; }
    public DbSet<DespesaTipo> DespesaTiposSenado { get; set; }
    public DbSet<DespesaResumoMensal> DespesaResumoMensal { get; set; }
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
    public DbSet<Secretario> Secretarios { get; set; }
    public DbSet<SecretarioCompleto> SecretariosCompletos { get; set; }
    public DbSet<SenadorCampeaoGasto> SenadoresCampeaoGasto { get; set; }
    public DbSet<SenadorHistoricoAcademico> SenadoresHistoricoAcademico { get; set; }
    public DbSet<SenadorProfissao> SenadoresProfissao { get; set; }
    public DbSet<SenadorPartido> SenadorPartidos { get; set; }
}

public static class SenadoFederalConfigurations
{
    public static void ConfigureSenador(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Senador>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            
            // Configure foreign keys
            entity.HasOne(e => e.Partido)
                  .WithMany()
                  .HasForeignKey(e => e.IdPartido);
                  
            entity.HasOne(e => e.Estado)
                  .WithMany()
                  .HasForeignKey(e => e.IdEstado);
                  
            entity.HasOne(e => e.EstadoNaturalidade)
                  .WithMany()
                  .HasForeignKey(e => e.IdEstadoNaturalidade);
        });
    }

    public static void ConfigureDespesaSenado(this ModelBuilder modelBuilder)
    {
        // Configure Despesa (Senado Federal - Composite Key)
        modelBuilder.Entity<Despesa>(entity =>
        {
            entity.HasKey(e => new { e.IdSenador, e.Id });
            entity.HasOne(e => e.Senador).WithMany(s => s.Despesas).HasForeignKey(e => e.IdSenador);
            entity.HasOne(e => e.DespesaTipo).WithMany(t => t.Despesas).HasForeignKey(e => e.IdDespesaTipo);
            entity.HasOne(e => e.Fornecedor).WithMany().HasForeignKey(e => e.IdFornecedor);
        });
    }

    public static void ConfigureMandatoSenado(this ModelBuilder modelBuilder)
    {
        // Configure Mandato (Senado Federal - Composite Key)
        modelBuilder.Entity<Mandato>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.IdSenador });
            entity.HasOne(e => e.Senador).WithMany(s => s.Mandatos).HasForeignKey(e => e.IdSenador);
            entity.HasOne(e => e.Estado).WithMany().HasForeignKey(e => e.IdEstado);
            entity.HasOne(e => e.Partido).WithMany().HasForeignKey(e => e.IdPartido);
        });
    }

    public static void ConfigureMandatoExercicioSenado(this ModelBuilder modelBuilder)
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

    public static void ConfigureMandatoLegislaturaSenado(this ModelBuilder modelBuilder)
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

    public static void ConfigureRemuneracaoSenado(this ModelBuilder modelBuilder)
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

    public static void ConfigureDespesaResumoMensal(this ModelBuilder modelBuilder)
    {
        // Configure DespesaResumoMensal (Composite Key)
        modelBuilder.Entity<DespesaResumoMensal>(entity =>
        {
            entity.HasKey(e => new { e.Ano, e.Mes });
        });
    }

    public static void ConfigureSenadoFederalSupportEntities(this ModelBuilder modelBuilder)
    {
        // Configure basic lookup tables
        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Descricao).IsUnique();
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Descricao).IsUnique();
        });

        modelBuilder.Entity<DespesaTipo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Descricao).IsUnique();
        });

        modelBuilder.Entity<Funcao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Descricao).IsUnique();
        });

        modelBuilder.Entity<Legislatura>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<Lotacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Descricao).IsUnique();
            entity.HasOne(e => e.Senador).WithMany(s => s.Lotacoes).HasForeignKey(e => e.IdSenador);
        });

        modelBuilder.Entity<MotivoAfastamento>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<ReferenciaCargo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Descricao).IsUnique();
        });

        modelBuilder.Entity<TipoFolha>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Vinculo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => e.Descricao).IsUnique();
        });
    }

    public static void ConfigureSenadoFederalAdditionalEntities(this ModelBuilder modelBuilder)
    {
        // Configure SenadorProfissao (Composite Key)
        modelBuilder.Entity<SenadorProfissao>(entity =>
        {
            entity.HasKey(e => new { e.IdSenador, e.IdProfissao });
            entity.HasOne(e => e.Senador).WithMany(s => s.Profissoes).HasForeignKey(e => e.IdSenador);
        });

        // Configure SenadorHistoricoAcademico (Composite Key)
        modelBuilder.Entity<SenadorHistoricoAcademico>(entity =>
        {
            entity.HasKey(e => new { e.IdSenador, e.Curso, e.Nivel });
            entity.HasOne(e => e.Senador).WithMany(s => s.HistoricoAcademico).HasForeignKey(e => e.IdSenador);
        });

        // Configure SenadorCampeaoGasto
        modelBuilder.Entity<SenadorCampeaoGasto>(entity =>
        {
            entity.HasKey(e => e.IdSenador);
            entity.HasOne(e => e.Senador).WithOne(s => s.CampeaoGasto).HasForeignKey<SenadorCampeaoGasto>(e => e.IdSenador);
        });

        // Configure Secretario
        modelBuilder.Entity<Secretario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(e => e.Senador).WithMany(s => s.Secretarios).HasForeignKey(e => e.IdSenador);
        });

        // Configure SecretarioCompleto (view-like entity)
        modelBuilder.Entity<SecretarioCompleto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(e => e.Senador).WithMany().HasForeignKey(e => e.IdSenador);
        });

        // Configure SenadorPartido
        modelBuilder.Entity<SenadorPartido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Senador).WithMany().HasForeignKey(e => e.IdSenador);
            entity.HasOne(e => e.Partido).WithMany().HasForeignKey(e => e.IdPartido);
        });
    }

    public static void ConfigureSenadoFederalEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureSenador();
        modelBuilder.ConfigureDespesaSenado();
        modelBuilder.ConfigureDespesaResumoMensal();
        modelBuilder.ConfigureMandatoSenado();
        modelBuilder.ConfigureMandatoExercicioSenado();
        modelBuilder.ConfigureMandatoLegislaturaSenado();
        modelBuilder.ConfigureRemuneracaoSenado();
        modelBuilder.ConfigureSenadoFederalSupportEntities();
        modelBuilder.ConfigureSenadoFederalAdditionalEntities();
    }
}
