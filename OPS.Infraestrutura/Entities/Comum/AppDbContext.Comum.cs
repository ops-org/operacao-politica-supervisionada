using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura.Entities.CamaraFederal;
using OPS.Infraestrutura.Entities.Comum;

namespace OPS.Infraestrutura;

public partial class AppDbContext
{
    // Common Tables
    public DbSet<Estado> Estados { get; set; }
    public DbSet<Municipio> Municipios { get; set; }
    public DbSet<Importacao> Importacoes { get; set; }
    public DbSet<Partido> Partidos { get; set; }
    public DbSet<TrechoViagem> TrechoViagens { get; set; }
    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<Profissao> Profissoes { get; set; }
    public DbSet<IndiceInflacao> IndicesInflacao { get; set; }
}

public static class CommonConfigurations
{
    public static void ConfigureIndiceInflacao(this ModelBuilder modelBuilder)
    {
        // Configure IndiceInflacao
        modelBuilder.Entity<IndiceInflacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Ano, e.Mes }).IsUnique();
            entity.ToTable("indice_inflacao", "public");
        });
    }

    public static void ConfigureLocalidade(this ModelBuilder modelBuilder)
    {
        // Configure Estado
        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasIndex(e => e.Sigla).IsUnique();
            entity.ToTable("estado", "public");
        });

        // Configure Estado
        modelBuilder.Entity<Municipio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasIndex(e => new { e.IdEstado, e.Nome }).IsUnique();
            entity.ToTable("municipio", "public");
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
        modelBuilder.ConfigureIndiceInflacao();
        modelBuilder.ConfigureLocalidade();
        modelBuilder.ConfigureImportacao();
        modelBuilder.ConfigurePartido();
        modelBuilder.ConfigurePessoa();
        modelBuilder.ConfigureProfissao();
    }
}
