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
            entity.ToTable("sf_senador", "senado");
        });
    }

    public static void ConfigureDespesaSenado(this ModelBuilder modelBuilder)
    {
        // Configure Despesa (Senado Federal - Composite Key)
        modelBuilder.Entity<Despesa>(entity =>
        {
            entity.HasKey(e => new { e.IdSenador, e.Id });
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(e => e.Senador).WithMany(s => s.Despesas).HasForeignKey(e => e.IdSenador);
            entity.HasOne(e => e.DespesaTipo).WithMany(t => t.Despesas).HasForeignKey(e => e.IdDespesaTipo);
            entity.HasOne(e => e.Fornecedor).WithMany().HasForeignKey(e => e.IdFornecedor);
            entity.ToTable("sf_despesa", "senado");
        });
    }

    public static void ConfigureMandatoSenado(this ModelBuilder modelBuilder)
    {
        // Configure Mandato (Senado Federal - Composite Key)
        modelBuilder.Entity<Mandato>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.IdSenador });
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(e => e.Senador).WithMany(s => s.Mandatos).HasForeignKey(e => e.IdSenador);
            entity.HasOne(e => e.Estado).WithMany().HasForeignKey(e => e.IdEstado);
            entity.HasOne(e => e.Partido).WithMany().HasForeignKey(e => e.IdPartido);
            entity.ToTable("sf_mandato", "senado");
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
            entity.ToTable("sf_mandato_exercicio", "senado");
        });
    }

    public static void ConfigureMandatoLegislaturaSenado(this ModelBuilder modelBuilder)
    {
        // Configure MandatoLegislatura (Composite Key)
        modelBuilder.Entity<MandatoLegislatura>(entity =>
        {
            entity.HasKey(e => new { e.IdMandato, e.IdLegislatura, e.IdSenador });
            entity.HasOne(e => e.Mandato).WithMany(m => m.MandatoLegislaturas)
                .HasForeignKey(e => new { e.IdMandato, e.IdSenador });
            entity.HasOne(e => e.Legislatura).WithMany(l => l.MandatoLegislaturas).HasForeignKey(e => e.IdLegislatura);
            entity.ToTable("sf_mandato_legislatura", "senado");
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
            entity.ToTable("sf_remuneracao", "senado");
        });
    }

    public static void ConfigureDespesaResumoMensalSenado(this ModelBuilder modelBuilder)
    {
        // Configure DespesaResumoMensal (Composite Key)
        modelBuilder.Entity<DespesaResumoMensal>(entity =>
        {
            entity.HasKey(e => new { e.Ano, e.Mes });
            entity.ToTable("sf_despesa_resumo_mensal", "senado");
        });
    }

    public static void ConfigureSenadoFederalSupportEntities(this ModelBuilder modelBuilder)
    {
        // Configure basic lookup tables
        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("sf_cargo", "senado");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("sf_categoria", "senado");
        });

        modelBuilder.Entity<DespesaTipo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("sf_despesa_tipo", "senado");
        });

        modelBuilder.Entity<Funcao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("sf_funcao", "senado");
        });

        modelBuilder.Entity<Legislatura>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("sf_legislatura", "senado");
        });

        modelBuilder.Entity<Lotacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(e => e.Senador).WithMany(s => s.Lotacoes).HasForeignKey(e => e.IdSenador);
            entity.ToTable("sf_lotacao", "senado");
        });

        modelBuilder.Entity<MotivoAfastamento>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("sf_motivo_afastamento", "senado");
        });

        modelBuilder.Entity<ReferenciaCargo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("sf_referencia_cargo", "senado");
        });

        modelBuilder.Entity<TipoFolha>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("sf_tipo_folha", "senado");
        });

        modelBuilder.Entity<Vinculo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("sf_vinculo", "senado");
        });
    }

    public static void ConfigureSenadoFederalAdditionalEntities(this ModelBuilder modelBuilder)
    {
        // Configure SenadorProfissao (Composite Key)
        modelBuilder.Entity<SenadorProfissao>(entity =>
        {
            entity.HasKey(e => new { e.IdSenador, e.IdProfissao });
            entity.HasOne(e => e.Senador).WithMany(s => s.Profissoes).HasForeignKey(e => e.IdSenador);
            entity.ToTable("sf_senador_profissao", "senado");
        });

        // Configure SenadorHistoricoAcademico (Composite Key)
        modelBuilder.Entity<SenadorHistoricoAcademico>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Senador).WithMany(s => s.HistoricoAcademico).HasForeignKey(e => e.IdSenador);
            entity.ToTable("sf_senador_historico_academico", "senado");
        });

        // Configure SenadorCampeaoGasto
        modelBuilder.Entity<SenadorCampeaoGasto>(entity =>
        {
            entity.HasKey(e => e.IdSenador);
            entity.HasOne(e => e.Senador).WithOne(s => s.CampeaoGasto).HasForeignKey<SenadorCampeaoGasto>(e => e.IdSenador);
            entity.ToTable("sf_senador_campeao_gasto", "senado");
        });

        // Configure Secretario
        modelBuilder.Entity<Secretario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(e => e.Senador).WithMany(s => s.Secretarios).HasForeignKey(e => e.IdSenador);
            entity.ToTable("sf_secretario", "senado");
        });

        // Configure SecretarioCompleto (view-like entity)
        modelBuilder.Entity<SecretarioCompleto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(e => e.Senador).WithMany().HasForeignKey(e => e.IdSenador);
            entity.ToTable("sf_secretario_completo", "senado");
        });

        // Configure SenadorPartido
        modelBuilder.Entity<SenadorPartido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(e => e.Senador).WithMany().HasForeignKey(e => e.IdSenador);
            entity.HasOne(e => e.Partido).WithMany().HasForeignKey(e => e.IdPartido);
            entity.ToTable("sf_senador_partido", "senado");
        });
    }

    public static void ConfigureSenadoFederalEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureSenador();
        modelBuilder.ConfigureDespesaSenado();
        modelBuilder.ConfigureDespesaResumoMensalSenado();
        modelBuilder.ConfigureMandatoSenado();
        modelBuilder.ConfigureMandatoExercicioSenado();
        modelBuilder.ConfigureMandatoLegislaturaSenado();
        modelBuilder.ConfigureRemuneracaoSenado();
        modelBuilder.ConfigureSenadoFederalSupportEntities();
        modelBuilder.ConfigureSenadoFederalAdditionalEntities();
    }
}
