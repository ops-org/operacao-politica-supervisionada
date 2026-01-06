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
    public DbSet<ForcecedorCnpjIncorreto> ForcecedorCnpjIncorretos { get; set; }
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
            entity.HasOne(e => e.Fornecedor).WithOne(f => f.FornecedorInfo).HasForeignKey<FornecedorInfo>(e => e.IdFornecedor);
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
            entity.HasOne(e => e.Fornecedor).WithMany(f => f.FornecedorSocios).HasForeignKey(e => e.IdFornecedor);
            
            // Configure relationship to FornecedorSocioQualificacao
            entity.HasOne(e => e.FornecedorSocioQualificacao)
                  .WithMany(q => q.FornecedorSocios)
                  .HasForeignKey(e => e.IdFornecedorSocioQualificacao);
            
            // Configure relationship to FornecedorSocioRepresentanteQualificacao
            entity.HasOne(e => e.FornecedorSocioRepresentanteQualificacao)
                  .WithMany()
                  .HasForeignKey(e => e.IdFornecedorSocioRepresentanteQualificacao);
            
            entity.ToTable("fornecedor_socio", "fornecedor");
        });
    }

    public static void ConfigureFornecedorAtividadeSecundaria(this ModelBuilder modelBuilder)
    {
        // Configure FornecedorAtividadeSecundaria (Composite Key)
        modelBuilder.Entity<FornecedorAtividadeSecundaria>(entity =>
        {
            entity.HasKey(e => new { e.IdFornecedor, e.IdAtividade });
            entity.HasOne(e => e.Fornecedor).WithMany().HasForeignKey(e => e.IdFornecedor);
            entity.HasOne(e => e.FornecedorAtividade).WithMany(a => a.FornecedorAtividadeSecundarias).HasForeignKey(e => e.IdAtividade);
            entity.ToTable("fornecedor_atividade_secundaria", "fornecedor");
        });
    }

    public static void ConfigureForcecedorCnpjIncorreto(this ModelBuilder modelBuilder)
    {
        // Configure ForcecedorCnpjIncorreto
        modelBuilder.Entity<ForcecedorCnpjIncorreto>(entity =>
        {
            entity.HasKey(e => e.CnpjIncorreto);
            entity.ToTable("forcecedor_cnpj_incorreto", "fornecedor");
        });
    }

    public static void ConfigureFornecedorFaixaEtaria(this ModelBuilder modelBuilder)
    {
        // Configure FornecedorFaixaEtaria
        modelBuilder.Entity<FornecedorFaixaEtaria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("fornecedor_faixa_etaria", "fornecedor");
        });
    }

    public static void ConfigureFornecedorSocioQualificacao(this ModelBuilder modelBuilder)
    {
        // Configure FornecedorSocioQualificacao
        modelBuilder.Entity<FornecedorSocioQualificacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("fornecedor_socio_qualificacao", "fornecedor");
        });
    }

    public static void ConfigureFornecedorEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureFornecedorInfo();
        modelBuilder.ConfigureFornecedorSocio();
        modelBuilder.ConfigureFornecedorAtividadeSecundaria();
        modelBuilder.ConfigureForcecedorCnpjIncorreto();
        modelBuilder.ConfigureFornecedorFaixaEtaria();
        modelBuilder.ConfigureFornecedorSocioQualificacao();
        
        // Configure remaining entities with schema
        modelBuilder.Entity<Fornecedor>(entity => entity.ToTable("fornecedor", "fornecedor"));
        modelBuilder.Entity<FornecedorAtividade>(entity => entity.ToTable("fornecedor_atividade", "fornecedor"));
        modelBuilder.Entity<FornecedorNaturezaJuridica>(entity => entity.ToTable("fornecedor_natureza_juridica", "fornecedor"));
    }
}
