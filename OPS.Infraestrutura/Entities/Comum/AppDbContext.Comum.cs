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
    public DbSet<Fornecedor> Fornecedores { get; set; }
    public DbSet<FornecedorInfo> FornecedorInfos { get; set; }
    public DbSet<FornecedorSocio> FornecedorSocios { get; set; }
    public DbSet<TrechoViagem> TrechoViagens { get; set; }
    public DbSet<FornecedorAtividade> FornecedorAtividades { get; set; }
    public DbSet<FornecedorAtividadeSecundaria> FornecedorAtividadesSecundarias { get; set; }
    public DbSet<Parametros> Parametros { get; set; }
    public DbSet<PartidoHistorico> PartidoHistoricos { get; set; }
    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<PessoaNew> PessoasNew { get; set; }
    public DbSet<Profissao> Profissoes { get; set; }
    public DbSet<EleicaoCandidato> EleicaoCandidatos { get; set; }
    public DbSet<EleicaoCandidatura> EleicaoCandidaturas { get; set; }
    public DbSet<EleicaoCargo> EleicaoCargos { get; set; }
    public DbSet<EleicaoDoacao> EleicaoDoacoes { get; set; }
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
        });
    }

    public static void ConfigureFornecedorInfo(this ModelBuilder modelBuilder)
    {
        // Configure FornecedorInfo
        modelBuilder.Entity<FornecedorInfo>(entity =>
        {
            entity.HasKey(e => e.IdFornecedor);
            entity.HasOne(e => e.Fornecedor).WithOne(f => f.FornecedorInfo).HasForeignKey<FornecedorInfo>(e => e.IdFornecedor);
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
        });
    }

    public static void ConfigureFornecedorAtividadeSecundaria(this ModelBuilder modelBuilder)
    {
        // Configure FornecedorAtividadeSecundaria (Composite Key)
        modelBuilder.Entity<FornecedorAtividadeSecundaria>(entity =>
        {
            entity.HasKey(e => new { e.IdFornecedor, e.IdFornecedorAtividade });
            entity.HasOne(e => e.Fornecedor).WithMany().HasForeignKey(e => e.IdFornecedor);
            entity.HasOne(e => e.FornecedorAtividade).WithMany(a => a.FornecedorAtividadeSecundarias).HasForeignKey(e => e.IdFornecedorAtividade);
        });
    }

    public static void ConfigureParametros(this ModelBuilder modelBuilder)
    {
        // Configure Parametros
        modelBuilder.Entity<Parametros>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }

    public static void ConfigurePartidoHistorico(this ModelBuilder modelBuilder)
    {
        // Configure PartidoHistorico
        modelBuilder.Entity<PartidoHistorico>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Partido).WithMany(p => p.PartidoHistoricos).HasForeignKey(e => e.IdPartido);
        });
    }

    public static void ConfigurePessoa(this ModelBuilder modelBuilder)
    {
        // Configure Pessoa
        modelBuilder.Entity<Pessoa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Estado).WithMany().HasForeignKey(e => e.IdEstado);
        });
    }

    public static void ConfigurePessoaNew(this ModelBuilder modelBuilder)
    {
        // Configure PessoaNew
        modelBuilder.Entity<PessoaNew>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Estado).WithMany().HasForeignKey(e => e.IdEstado);
        });
    }

    public static void ConfigureProfissao(this ModelBuilder modelBuilder)
    {
        // Configure Profissao
        modelBuilder.Entity<Profissao>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }

    public static void ConfigureEleicaoCandidato(this ModelBuilder modelBuilder)
    {
        // Configure EleicaoCandidato
        modelBuilder.Entity<EleicaoCandidato>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Cpf).IsUnique();
        });
    }

    public static void ConfigureEleicaoCandidatura(this ModelBuilder modelBuilder)
    {
        // Configure EleicaoCandidatura
        modelBuilder.Entity<EleicaoCandidatura>(entity =>
        {
            entity.HasKey(e => new { e.Numero, e.Cargo, e.Ano, e.SiglaEstado });
        });
    }

    public static void ConfigureEleicaoCargo(this ModelBuilder modelBuilder)
    {
        // Configure EleicaoCargo
        modelBuilder.Entity<EleicaoCargo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Nome).IsUnique();
        });
    }

    public static void ConfigureEleicaoDoacao(this ModelBuilder modelBuilder)
    {
        // Configure EleicaoDoacao
        modelBuilder.Entity<EleicaoDoacao>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }

    public static void ConfigureComumEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureEstado();
        modelBuilder.ConfigurePartido();
        modelBuilder.ConfigureFornecedorInfo();
        modelBuilder.ConfigureFornecedorSocio();
        modelBuilder.ConfigureFornecedorAtividadeSecundaria();
        modelBuilder.ConfigureParametros();
        modelBuilder.ConfigurePartidoHistorico();
        modelBuilder.ConfigurePessoa();
        modelBuilder.ConfigurePessoaNew();
        modelBuilder.ConfigureProfissao();
        modelBuilder.ConfigureEleicaoCandidato();
        modelBuilder.ConfigureEleicaoCandidatura();
        modelBuilder.ConfigureEleicaoCargo();
        modelBuilder.ConfigureEleicaoDoacao();
    }
}
