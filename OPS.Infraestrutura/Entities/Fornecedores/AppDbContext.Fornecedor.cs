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
            
            // Temporarily remove navigation property configurations to prevent shadow properties
            // Configure the relationship with FornecedorNaturezaJuridica
            // entity.HasOne(e => e.FornecedorNaturezaJuridica)
            //       .WithMany()
            //       .HasForeignKey(e => e.IdFornecedorNaturezaJuridica);
            
            // Configure the relationship with FornecedorAtividade
            // entity.HasOne(e => e.FornecedorAtividadePrincipal)
            //       .WithMany()
            //       .HasForeignKey(e => e.IdFornecedorAtividadePrincipal);
            
            entity.ToTable("fornecedor_info", "fornecedor");
        });
    }

    public static void ConfigureFornecedorSocio(this ModelBuilder modelBuilder)
    {
        // Configure FornecedorSocio - minimal configuration to avoid complex relationships
        modelBuilder.Entity<FornecedorSocio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
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
        modelBuilder.ConfigureFornecedorFaixaEtaria();
        modelBuilder.ConfigureFornecedorSocioQualificacao();

        // Configure remaining entities with schema
        modelBuilder.Entity<Fornecedor>(entity => 
        {
            entity.ToTable("fornecedor", "fornecedor");

            //// Explicitly configure all properties to prevent shadow property creation
            //entity.Property(e => e.Id).HasColumnName("id");
            //entity.Property(e => e.CnpjCpf).HasColumnName("cnpj_cpf");
            //entity.Property(e => e.Nome).HasColumnName("nome");
            //entity.Property(e => e.Categoria).HasColumnName("categoria");
            //entity.Property(e => e.Doador).HasColumnName("doador");
            //entity.Property(e => e.Controle).HasColumnName("controle");
            //entity.Property(e => e.Mensagem).HasColumnName("mensagem");

            // Configure the FornecedorInfo relationship to prevent shadow properties
            entity.HasOne(e => e.FornecedorInfo)
                  .WithOne(f => f.Fornecedor)
                  .HasForeignKey<FornecedorInfo>(f => f.IdFornecedor);

            //// Ignore ALL potential shadow properties that EF might try to create
            //entity.Ignore("FornecedorNaturezaJuridicaId");
            //entity.Ignore("FornecedorNaturezaJuridicaId1");
            //entity.Ignore("FornecedorNaturezaJuridicaId2");
            //entity.Ignore("FornecedorAtividadePrincipalId");
            //entity.Ignore("FornecedorAtividadePrincipalId1");
            //entity.Ignore("FornecedorAtividadePrincipalId2");
            //entity.Ignore("FornecedorInfoId");
            //entity.Ignore("FornecedorInfoId1");
        });
        modelBuilder.Entity<FornecedorAtividade>(entity => entity.ToTable("fornecedor_atividade", "fornecedor"));
        modelBuilder.Entity<FornecedorNaturezaJuridica>(entity => entity.ToTable("fornecedor_natureza_juridica", "fornecedor"));
    }
}
