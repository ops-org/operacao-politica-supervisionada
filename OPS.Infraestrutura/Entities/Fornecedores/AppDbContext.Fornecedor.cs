using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura.Entities.Fornecedores;

namespace OPS.Infraestrutura;

public partial class AppDbContext
{
    // Fornecedor Tables
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<FornecedorInfo> FornecedorInfos { get; set; }
    public DbSet<FornecedorSocio> FornecedorSocios { get; set; }
    public DbSet<FornecedorAtividade> FornecedorAtividades { get; set; }
    public DbSet<FornecedorAtividadeSecundaria> FornecedorAtividadesSecundarias { get; set; }
    public DbSet<FornecedorNaturezaJuridica> FornecedorNaturezaJuridicas { get; set; }
    public DbSet<FornecedorFaixaEtaria> FornecedorFaixaEtarias { get; set; }
    public DbSet<FornecedorSocioQualificacao> FornecedorSocioQualificacoes { get; set; }
}

public static class FornecedorConfigurations
{
    public static void ConfigureFornecedorInfo(this ModelBuilder modelBuilder)
    {
        // Configure FornecedorInfo
        modelBuilder.Entity<FornecedorInfo>(entity =>
        {
            entity.HasKey(e => e.IdFornecedor);

            // Configure the relationship with FornecedorNaturezaJuridica
            entity.HasOne(e => e.FornecedorNaturezaJuridica)
                  .WithMany(e => e.FornecedorInfos)
                  .HasForeignKey(e => e.IdFornecedorNaturezaJuridica);

            // Configure the relationship with FornecedorAtividade
            entity.HasOne(e => e.FornecedorAtividadePrincipal)
                  .WithMany(e => e.FornecedorInfos)
                  .HasForeignKey(e => e.IdFornecedorAtividadePrincipal);

            entity.ToTable("fornecedor_info", "fornecedor");
        });
    }

    public static void ConfigureFornecedorSocio(this ModelBuilder modelBuilder)
    {
        // Configure FornecedorSocio
        modelBuilder.Entity<FornecedorSocio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            // Configure relationships with explicit foreign keys
            entity.HasOne(e => e.Fornecedor)
                .WithMany(e => e.FornecedorSocios)
                .HasForeignKey(e => e.IdFornecedor)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.FornecedorSocioQualificacao)
                .WithMany(e => e.FornecedorSocios)
                .HasForeignKey(e => e.IdFornecedorSocioQualificacao)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.FornecedorSocioRepresentanteQualificacao)
                .WithMany(e => e.FornecedorSocioRepresentantes)
                .HasForeignKey(e => e.IdFornecedorSocioRepresentanteQualificacao)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.FornecedorFaixaEtaria)
                .WithMany(e => e.FornecedorSocios)
                .HasForeignKey(e => e.IdFornecedorFaixaEtaria)
                //.HasConstraintName("fk_fornecedor_socio_faixa_etaria")
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable("fornecedor_socio", "fornecedor");
        });
    }

    public static void ConfigureFornecedorAtividadeSecundaria(this ModelBuilder modelBuilder)
    {
        // Configure FornecedorAtividadeSecundaria (Composite Key)
        modelBuilder.Entity<FornecedorAtividadeSecundaria>(entity =>
        {
            entity.HasKey(e => new { e.IdFornecedor, e.IdAtividade });

            entity.HasOne(e => e.Fornecedor)
                .WithMany(a => a.FornecedorAtividadeSecundarias)
                .HasForeignKey(e => e.IdFornecedor);

            entity.HasOne(e => e.FornecedorAtividade)
                .WithMany(a => a.FornecedorAtividadeSecundarias)
                .HasForeignKey(e => e.IdAtividade);

            entity.ToTable("fornecedor_atividade_secundaria", "fornecedor");
        });
    }

    public static void ConfigureFornecedorFaixaEtaria(this ModelBuilder modelBuilder)
    {
        // Configure FornecedorFaixaEtaria
        modelBuilder.Entity<FornecedorFaixaEtaria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.ToTable("fornecedor_faixa_etaria", "fornecedor");
        });
    }

    public static void ConfigureFornecedorSocioQualificacao(this ModelBuilder modelBuilder)
    {
        // Configure FornecedorSocioQualificacao
        modelBuilder.Entity<FornecedorSocioQualificacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.ToTable("fornecedor_socio_qualificacao", "fornecedor");
        });
    }

    public static void ConfigureFornecedorEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureFornecedorInfo();
        modelBuilder.ConfigureFornecedorSocio();
        modelBuilder.ConfigureFornecedorAtividadeSecundaria();
        modelBuilder.ConfigureFornecedorFaixaEtaria();
        modelBuilder.ConfigureFornecedorSocioQualificacao();

        // Configure remaining entities with schema
        modelBuilder.Entity<Fornecedor>(entity =>
        {
            entity.ToTable("fornecedor", "fornecedor");

            // Configure the FornecedorInfo relationship
            entity.HasOne(e => e.FornecedorInfo)
                  .WithOne(f => f.Fornecedor)
                  .HasForeignKey<FornecedorInfo>(f => f.IdFornecedor);
        });

        modelBuilder.Entity<FornecedorAtividade>(entity => 
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.ToTable("fornecedor_atividade", "fornecedor");
        });

        modelBuilder.Entity<FornecedorNaturezaJuridica>(entity => 
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.ToTable("fornecedor_natureza_juridica", "fornecedor");
        });
    }
}
