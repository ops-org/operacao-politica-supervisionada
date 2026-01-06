using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura.Entities.CamaraFederal;
using OPS.Infraestrutura.Entities.Comum;
using OPS.Infraestrutura.Entities.Fornecedores;

namespace OPS.Infraestrutura;

public partial class AppDbContext
{
    // Common Tables
    public DbSet<Estado> Estados { get; set; }
    public DbSet<Partido> Partidos { get; set; }
    public DbSet<TrechoViagem> TrechoViagens { get; set; }
    public DbSet<PartidoHistorico> PartidoHistoricos { get; set; }
    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<PessoaNew> PessoasNew { get; set; }
    public DbSet<Profissao> Profissoes { get; set; }
}

public static class ComumConfigurations
{
    public static void ConfigureEstado(this ModelBuilder modelBuilder)
    {
        // Configure Estado
        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasIndex(e => e.Sigla).IsUnique();
            entity.HasIndex(e => e.Nome).IsUnique();
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
            entity.HasIndex(e => e.Sigla).IsUnique();
            entity.HasIndex(e => e.Nome).IsUnique();
            entity.ToTable("partido", "public");
        });
    }

    public static void ConfigurePartidoHistorico(this ModelBuilder modelBuilder)
    {
        // Configure PartidoHistorico
        modelBuilder.Entity<PartidoHistorico>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Partido).WithMany(p => p.PartidoHistoricos).HasForeignKey(e => e.IdPartido);
            entity.ToTable("partido_historico", "public");
        });
    }

    public static void ConfigurePessoa(this ModelBuilder modelBuilder)
    {
        // Configure Pessoa
        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Estado).WithMany().HasForeignKey(e => e.IdEstado);
            entity.ToTable("pessoa", "public");
        });
    }

    public static void ConfigurePessoaNew(this ModelBuilder modelBuilder)
    {
        // Configure PessoaNew
        modelBuilder.Entity<PessoaNew>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Estado).WithMany().HasForeignKey(e => e.IdEstado);
            entity.ToTable("pessoa_new", "public");
        });
    }

    public static void ConfigureProfissao(this ModelBuilder modelBuilder)
    {
        // Configure Profissao
        modelBuilder.Entity<Profissao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("profissao", "public");
        });
    }

    public static void ConfigureComumEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureEstado();
        modelBuilder.ConfigurePartido();
        modelBuilder.ConfigurePartidoHistorico();
        modelBuilder.ConfigurePessoa();
        modelBuilder.ConfigurePessoaNew();
        modelBuilder.ConfigureProfissao();
    }
}
