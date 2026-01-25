using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura.Entities.Temp;

namespace OPS.Infraestrutura;

public partial class AppDbContext
{
    // Temp Tables - Camara Federal
    public DbSet<DeputadoFederalDespesaTemp> DeputadoFederalDespesaTemps { get; set; }
    public DbSet<CamaraEstadualDespesa> CamaraEstadualDespesas { get; set; }
    public DbSet<CamaraEstadualDespesaTipo> CamaraEstadualDespesaTipos { get; set; }

    // Temp Tables - Senado Federal
    public DbSet<SenadoDespesaTemp> SenadoDespesaTemps { get; set; }
    public DbSet<SenadoRemuneracaoTemp> SenadoRemuneracaoTemps { get; set; }
    public DbSet<SenadoSecretarioTemp> SenadoSecretarioTemps { get; set; }

    // Temp Tables - TSE
    public DbSet<TseCandidato> TseCandidatos { get; set; }
    public DbSet<TseDespesaContratada> TseDespesaContratadas { get; set; }
    public DbSet<TseDespesaPaga> TseDespesaPagas { get; set; }
    public DbSet<TseReceita> TseReceitas { get; set; }
    public DbSet<TseReceitaDoadorOriginario> TseReceitaDoadorOriginarios { get; set; }

    // Temp Tables - Fornecedor
    public DbSet<FornecedorDePara> FornecedorDeParas { get; set; }

    public DbSet<PartidoDePara> PartidoDeParas { get; set; }

    // Temp Tables - Common
    public DbSet<ArquivoChecksum> ArquivoChecksums { get; set; }
    public DbSet<DeputadoEstadualDepara> DeputadoEstadualDeparas { get; set; }
    public DbSet<CamaraEstadualDespesaTemp> CamaraEstadualDespesaTemps { get; set; }
    public DbSet<CamaraEstadualFuncionarioTemp> CamaraEstadualFuncionarioTemps { get; set; }
    public DbSet<CamaraEstadualRemuneracaoTemp> CamaraEstadualRemuneracaoTemps { get; set; }
    public DbSet<CamaraEstadualSecretarioRemuneracaoTemp> CamaraEstadualSecretarioRemuneracaoTemps { get; set; }
    public DbSet<CamaraEstadualEmpenhoTemp> CamaraEstadualEmpenhoTemps { get; set; }
    public DbSet<CamaraFederalDeputadoFuncionarioTemp> CamaraEstadualDeputadoFuncionarioTemps { get; set; }
}

public static class TempConfigurations
{
    public static void ConfigureArquivoChecksum(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArquivoChecksum>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.ToTable("arquivo_cheksum", "temp");
        });
    }

    public static void ConfigureDeputadoFederalDespesaTemp(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeputadoFederalDespesaTemp>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Hash).HasColumnType("bytea").IsRequired(false);
            entity.ToTable("cf_despesa_temp", "temp");
        });
    }

    public static void ConfigureCamaraEstadualDespesaTemp(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CamaraEstadualDespesaTemp>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Hash).HasColumnType("bytea");
            entity.ToTable("cl_despesa_temp", "temp");
        });
    }

    public static void ConfigureCamaraEstadualDespesa(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CamaraEstadualDespesa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Hash).HasColumnType("bytea");
            entity.ToTable("cl_despesa", "temp");
        });
    }

    public static void ConfigureCamaraEstadualDespesaTipo(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CamaraEstadualDespesaTipo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cl_despesa_tipo", "temp");
        });
    }

    public static void ConfigureSenadoDespesaTemp(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SenadoDespesaTemp>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.Property(e => e.Hash).HasColumnType("bytea");
            entity.ToTable("sf_despesa_temp", "temp");
        });
    }

    public static void ConfigureSenadoRemuneracaoTemp(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SenadoRemuneracaoTemp>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable("sf_remuneracao_temp", "temp");
        });
    }

    public static void ConfigureSenadoSecretarioTemp(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SenadoSecretarioTemp>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable("sf_secretario_temp", "temp");
        });
    }

    public static void ConfigureTseCandidato(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TseCandidato>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable("tse_candidato", "temp");
        });
    }

    public static void ConfigureTseDespesaContratada(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TseDespesaContratada>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable("tse_despesa_contratada", "temp");
        });
    }

    public static void ConfigureTseDespesaPaga(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TseDespesaPaga>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable("tse_despesa_paga", "temp");
        });
    }

    public static void ConfigureTseReceita(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TseReceita>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable("tse_receita", "temp");
        });
    }

    public static void ConfigureTseReceitaDoadorOriginario(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TseReceitaDoadorOriginario>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable("tse_receita_doador_originario", "temp");
        });
    }

    public static void ConfigureDeputadoEstadualDePara(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeputadoEstadualDepara>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.ToTable("cl_deputado_de_para", "temp");
        });
    }

    public static void ConfigureFornecedorDeParaTemp(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FornecedorDePara>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.ToTable("fornecedor_de_para", "temp");
        });
    }

    public static void ConfigurePartidoDePara(this ModelBuilder modelBuilder)
    {
        // Configure Partido
        modelBuilder.Entity<PartidoDePara>(entity =>
        {
            entity.ToTable("partido_de_para", "temp");
        });
    }

    public static void ConfigureCamaraEstadualFuncionarioTemp(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CamaraEstadualFuncionarioTemp>(entity =>
        {
            entity.HasKey(e => e.Chave);
            entity.ToTable("cf_funcionario_temp", "temp");
        });
    }

    public static void ConfigureCamaraEstadualRemuneracaoTemp(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CamaraEstadualRemuneracaoTemp>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable("cf_remuneracao_temp", "temp");
        });
    }

    public static void ConfigureCamaraEstadualSecretarioRemuneracaoTemp(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CamaraEstadualSecretarioRemuneracaoTemp>(entity =>
        {
            entity.HasKey(e => new { e.IdCfSecretario, e.Referencia, e.Descricao });
            entity.ToTable("cf_secretario_remuneracao_temp", "temp");
        });
    }

    public static void ConfigureCamaraEstadualEmpenhoTemp(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CamaraEstadualEmpenhoTemp>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cl_empenho_temp", "temp");
        });
    }

    public static void ConfigureCamaraFederalDeputadoFuncionarioTemp(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CamaraFederalDeputadoFuncionarioTemp>(entity =>
        {
            entity.HasKey(e => e.Chave);
            entity.ToTable("cf_deputado_funcionario_temp", "temp");
        });
    }


    // Master method to apply all configurations
    public static void ConfigureTempEntities(this ModelBuilder modelBuilder)
    {
        // Camara Federal configurations
        modelBuilder.ConfigureDeputadoFederalDespesaTemp();
        modelBuilder.ConfigureCamaraEstadualDespesaTemp();
        modelBuilder.ConfigureCamaraEstadualDespesa();
        modelBuilder.ConfigureCamaraEstadualDespesaTipo();
        modelBuilder.ConfigureCamaraEstadualFuncionarioTemp();
        modelBuilder.ConfigureCamaraEstadualRemuneracaoTemp();
        modelBuilder.ConfigureCamaraEstadualSecretarioRemuneracaoTemp();
        modelBuilder.ConfigureCamaraEstadualEmpenhoTemp();
        modelBuilder.ConfigureCamaraFederalDeputadoFuncionarioTemp();

        // Senado Federal configurations
        modelBuilder.ConfigureSenadoDespesaTemp();
        modelBuilder.ConfigureSenadoRemuneracaoTemp();
        modelBuilder.ConfigureSenadoSecretarioTemp();

        // TSE configurations
        modelBuilder.ConfigureTseCandidato();
        modelBuilder.ConfigureTseDespesaContratada();
        modelBuilder.ConfigureTseDespesaPaga();
        modelBuilder.ConfigureTseReceita();
        modelBuilder.ConfigureTseReceitaDoadorOriginario();

        // Fornecedor configurations
        modelBuilder.ConfigureFornecedorDeParaTemp();
        modelBuilder.ConfigurePartidoDePara();

        // Common configurations
        modelBuilder.ConfigureArquivoChecksum();
        modelBuilder.ConfigureDeputadoEstadualDePara();
    }
}
