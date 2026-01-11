using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura.Entities.CamaraFederal;
using OPS.Infraestrutura.Entities.Comum;

namespace OPS.Infraestrutura;

public partial class AppDbContext
{
    // Common Tables
    public DbSet<Estado> Estados { get; set; }
    public DbSet<Importacao> Importacoes { get; set; }
    public DbSet<Partido> Partidos { get; set; }
    public DbSet<TrechoViagem> TrechoViagens { get; set; }
    public DbSet<PartidoHistorico> PartidoHistoricos { get; set; }
    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<PessoaNew> PessoasNew { get; set; }
    public DbSet<Profissao> Profissoes { get; set; }
}

public static class CommonConfigurations
{
    public static void ConfigureEstado(this ModelBuilder modelBuilder)
    {
        // Configure Estado
        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasIndex(e => e.Sigla).IsUnique();
            entity.ToTable("estado", "public");
        });
    }

    public static void ConfigurePartido(this ModelBuilder modelBuilder)
    {
        // Configure Partido
        modelBuilder.Entity<Partido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasIndex(e => e.Legenda);
            entity.ToTable("partido", "public");
        });
    }

    public static void ConfigureImportacao(this ModelBuilder modelBuilder)
    {
        // Configure Importacao
        modelBuilder.Entity<Importacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("importacao", "public");
            
            // Configure the relationship with Estado
            entity.HasOne(e => e.Estado)
                  .WithMany()
                  .HasForeignKey(e => e.IdEstado);
        });
    }

    public static void ConfigurePartidoHistorico(this ModelBuilder modelBuilder)
    {
        // Configure PartidoHistorico
        modelBuilder.Entity<PartidoHistorico>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("partido_historico", "public");
        });
    }

    public static void ConfigurePessoa(this ModelBuilder modelBuilder)
    {
        // Configure Pessoa
        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("pessoa", "public");
        });
    }

    public static void ConfigurePessoaNew(this ModelBuilder modelBuilder)
    {
        // Configure PessoaNew
        modelBuilder.Entity<PessoaNew>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasIndex(e => e.Cpf).IsUnique();

            // Foreign key relationships
            entity.HasOne(e => e.EstadoNascimento).WithMany().HasForeignKey(e => e.IdEstadoNascimento);
            entity.HasOne(e => e.Nacionalidade).WithMany().HasForeignKey(e => e.IdNacionalidade);
            entity.HasOne(e => e.Genero).WithMany().HasForeignKey(e => e.IdGenero);
            entity.HasOne(e => e.Etnia).WithMany().HasForeignKey(e => e.IdEtnia);
            entity.HasOne(e => e.EstadoCivil).WithMany().HasForeignKey(e => e.IdEstadoCivil);
            entity.HasOne(e => e.GrauInstrucao).WithMany().HasForeignKey(e => e.IdGrauInstrucao);
            entity.HasOne(e => e.Ocupacao).WithMany().HasForeignKey(e => e.IdOcupacao);

            entity.ToTable("pessoa_new", "public");
        });
    }

    public static void ConfigureProfissao(this ModelBuilder modelBuilder)
    {
        // Configure Profissao
        modelBuilder.Entity<Profissao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("profissao", "public");
        });
    }

    public static void ConfigureCommonEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureEstado();
        modelBuilder.ConfigureImportacao();
        modelBuilder.ConfigurePartido();
        modelBuilder.ConfigurePartidoHistorico();
        modelBuilder.ConfigurePessoa();
        modelBuilder.ConfigurePessoaNew();
        modelBuilder.ConfigureProfissao();
    }
}
