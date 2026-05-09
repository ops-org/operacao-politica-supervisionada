using Microsoft.EntityFrameworkCore;

namespace OPS.Infraestrutura.Entities.TSE;

public partial class AppDbContext
{
    // TSE Tables
    public DbSet<Candidatura> Candidaturas { get; set; }
    public DbSet<CandidaturaBem> CandidaturaBens { get; set; }
    public DbSet<CandidaturaMotivoCassacao> CandidaturaMotivoCassacoes { get; set; }
    public DbSet<Cargo> Cargos { get; set; }
    public DbSet<ColigacaoPartidaria> ColigacoesPartidarias { get; set; }
    public DbSet<ColigacaoPartidariaPartido> ColigacaoPartidariaPartidos { get; set; }
    public DbSet<Fonte> Fontes { get; set; }
    public DbSet<FonteReferencia> FonteReferencias { get; set; }
    public DbSet<MotivoCassacao> MotivoCassacoes { get; set; }
    public DbSet<Municipio> Municipios { get; set; }
    public DbSet<Pais> Paises { get; set; }
    public DbSet<Partido> Partidos { get; set; }
    public DbSet<PessoaFisica> PessoasFisicas { get; set; }
    public DbSet<PleitoGeral> PleitosGerais { get; set; }
    public DbSet<PleitoGeralCargo> PleitoGeralCargos { get; set; }
    public DbSet<PleitoRegional> PleitosRegionais { get; set; }
    public DbSet<PleitoRegionalCargo> PleitoRegionalCargos { get; set; }
    public DbSet<UnidadeFederativa> UnidadesFederativas { get; set; }
}

public static class TSEConfigurations
{
    public static void ConfigureCandidatura(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidatura>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseKey).HasMaxLength(255);
            entity.Property(e => e.TseCandidatoSequencial).HasMaxLength(50);
            entity.Property(e => e.TseCandidatoNumero).HasMaxLength(50);
            entity.Property(e => e.TseCandidatoNomeUrna).HasMaxLength(255);
            entity.Property(e => e.TseCandidaturaSituacaoCodigo).HasMaxLength(50);
            entity.Property(e => e.TseCandidaturaSituacaoDescricao).HasMaxLength(255);
            entity.Property(e => e.TseCandidaturaProtocolo).HasMaxLength(255);
            entity.Property(e => e.TseProcessoNumero).HasMaxLength(255);
            entity.Property(e => e.TseOcupacaoCodigo).HasMaxLength(50);
            entity.Property(e => e.TseOcupacaoDescricao).HasMaxLength(255);
            entity.Property(e => e.TseGeneroCodigo).HasMaxLength(50);
            entity.Property(e => e.TseGeneroDescricao).HasMaxLength(255);
            entity.Property(e => e.TseGrauInstrucaoCodigo).HasMaxLength(50);
            entity.Property(e => e.TseGrauInstrucaoDescricao).HasMaxLength(255);
            entity.Property(e => e.TseEstadoCivilCodigo).HasMaxLength(50);
            entity.Property(e => e.TseEstadoCivilDescricao).HasMaxLength(255);
            entity.Property(e => e.TseCorRacaCodigo).HasMaxLength(50);
            entity.Property(e => e.TseCorRacaDescricao).HasMaxLength(255);
            entity.Property(e => e.TseNacionalidadeCodigo).HasMaxLength(50);
            entity.Property(e => e.TseNacionalidadeDescricao).HasMaxLength(255);
            entity.Property(e => e.TseSituacaoTurnoCodigo).HasMaxLength(50);
            entity.Property(e => e.TseSituacaoTurnoDescricao).HasMaxLength(255);
            entity.Property(e => e.TseEmail).HasMaxLength(255);

            entity.HasOne(e => e.Cargo)
                  .WithMany()
                  .HasForeignKey(e => e.IdCargo);

            entity.HasOne(e => e.ColigacaoPartidaria)
                  .WithMany()
                  .HasForeignKey(e => e.IdColigacaoPartidaria);

            entity.HasOne(e => e.Municipio)
                  .WithMany()
                  .HasForeignKey(e => e.IdMunicipio);

            entity.HasOne(e => e.Pais)
                  .WithMany()
                  .HasForeignKey(e => e.IdPais);

            entity.HasOne(e => e.Partido)
                  .WithMany()
                  .HasForeignKey(e => e.IdPartido);

            entity.HasOne(e => e.PessoaFisica)
                  .WithMany()
                  .HasForeignKey(e => e.IdPessoaFisica);

            entity.HasOne(e => e.PleitoGeral)
                  .WithMany()
                  .HasForeignKey(e => e.IdPleitoGeral);

            entity.HasOne(e => e.PleitoRegional)
                  .WithMany()
                  .HasForeignKey(e => e.IdPleitoRegional);

            entity.HasOne(e => e.UnidadeFederativa)
                  .WithMany()
                  .HasForeignKey(e => e.IdUnidadeFederativa);

            entity.ToTable("candidatura", "tse");
        });
    }

    public static void ConfigureCandidaturaBem(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CandidaturaBem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseKey).HasMaxLength(255);
            entity.Property(e => e.TseOrdem).HasMaxLength(50);
            entity.Property(e => e.TseTipoCodigo).HasMaxLength(50);
            entity.Property(e => e.TseTipoDescricao).HasMaxLength(255);
            entity.Property(e => e.TseDescricao).HasMaxLength(1000);
            entity.Property(e => e.TseValor).HasColumnType("numeric(20, 6)");

            entity.HasOne(e => e.Candidatura)
                  .WithMany()
                  .HasForeignKey(e => e.IdCandidatura);

            entity.ToTable("candidatura_bem", "tse");
        });
    }

    public static void ConfigureCandidaturaMotivoCassacao(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CandidaturaMotivoCassacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(e => e.Candidatura)
                  .WithMany()
                  .HasForeignKey(e => e.IdCandidatura);

            entity.HasOne(e => e.MotivoCassacao)
                  .WithMany()
                  .HasForeignKey(e => e.IdMotivoCassacao);

            entity.ToTable("candidatura_motivo_cassacao", "tse");
        });
    }

    public static void ConfigureCargo(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseKey).HasMaxLength(255);
            entity.Property(e => e.TseCodigo).HasMaxLength(50);
            entity.Property(e => e.TseDescricao).HasMaxLength(255);

            entity.ToTable("cargo", "tse");
        });
    }

    public static void ConfigureColigacaoPartidaria(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ColigacaoPartidaria>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseKey).HasMaxLength(255);
            entity.Property(e => e.TseSequencialColigacao).HasMaxLength(50);
            entity.Property(e => e.TseNome).HasMaxLength(255);
            entity.Property(e => e.TseTipoDescricao).HasMaxLength(255);

            entity.ToTable("coligacao_partidaria", "tse");
        });
    }

    public static void ConfigureColigacaoPartidariaPartido(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ColigacaoPartidariaPartido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();

            entity.HasOne(e => e.ColigacaoPartidaria)
                  .WithMany()
                  .HasForeignKey(e => e.IdColigacaoPartidaria);

            entity.HasOne(e => e.Partido)
                  .WithMany()
                  .HasForeignKey(e => e.IdPartido);

            entity.ToTable("coligacao_partidaria_partido", "tse");
        });
    }

    public static void ConfigureFonte(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fonte>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.Sigla).HasMaxLength(50);
            entity.Property(e => e.Descricao).HasMaxLength(500);
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.RepositorioUrl).HasMaxLength(500);

            entity.ToTable("fonte", "tse");
        });
    }

    public static void ConfigureFonteReferencia(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FonteReferencia>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.Registro).HasMaxLength(255);

            entity.HasOne(e => e.Fonte)
                  .WithMany()
                  .HasForeignKey(e => e.IdFonte);

            entity.ToTable("fonte_referencia", "tse");
        });
    }

    public static void ConfigureMotivoCassacao(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MotivoCassacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseKey).HasMaxLength(255);
            entity.Property(e => e.TseCodigo).HasMaxLength(50);
            entity.Property(e => e.TseDescricao).HasMaxLength(255);

            entity.ToTable("motivo_cassacao", "tse");
        });
    }

    public static void ConfigureMunicipio(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Municipio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseKey).HasMaxLength(255);
            entity.Property(e => e.TseSigla).HasMaxLength(50);
            entity.Property(e => e.TseNome).HasMaxLength(255);

            entity.HasOne(e => e.UnidadeFederativa)
                  .WithMany()
                  .HasForeignKey(e => e.IdUnidadeFederativa);

            entity.ToTable("municipio", "tse");
        });
    }

    public static void ConfigurePais(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pais>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseCodigo).HasMaxLength(50);
            entity.Property(e => e.TseSigla).HasMaxLength(50);
            entity.Property(e => e.TseNome).HasMaxLength(255);

            entity.ToTable("pais", "tse");
        });
    }

    public static void ConfigurePartido(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Partido>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseKey).HasMaxLength(255);
            entity.Property(e => e.TseNumero).HasMaxLength(50);
            entity.Property(e => e.TseSigla).HasMaxLength(50);
            entity.Property(e => e.TseNome).HasMaxLength(255);

            entity.ToTable("partido", "tse");
        });
    }

    public static void ConfigurePessoaFisica(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PessoaFisica>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseKey).HasMaxLength(255);
            entity.Property(e => e.TseCpf).HasMaxLength(50);
            entity.Property(e => e.TseNome).HasMaxLength(255);
            entity.Property(e => e.TseNomeSocial).HasMaxLength(255);
            entity.Property(e => e.TseNumeroTituloEleitoral).HasMaxLength(50);

            entity.HasOne(e => e.PaisNascimento)
                  .WithMany()
                  .HasForeignKey(e => e.IdPaisNascimento);

            entity.HasOne(e => e.UnidadeFederativaNascimento)
                  .WithMany()
                  .HasForeignKey(e => e.IdUnidadeFederativaNascimento);

            entity.HasOne(e => e.MunicipioNascimento)
                  .WithMany()
                  .HasForeignKey(e => e.IdMunicipioNascimento);

            entity.ToTable("pessoa_fisica", "tse");
        });
    }

    public static void ConfigurePleitoGeral(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PleitoGeral>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseKey).HasMaxLength(255);
            entity.Property(e => e.TseCodigo).HasMaxLength(50);
            entity.Property(e => e.TseDescricao).HasMaxLength(255);
            entity.Property(e => e.TseTurno).HasMaxLength(50);
            entity.Property(e => e.TseTipoCodigo).HasMaxLength(50);
            entity.Property(e => e.TseTipoDescricao).HasMaxLength(255);

            entity.ToTable("pleito_geral", "tse");
        });
    }

    public static void ConfigurePleitoGeralCargo(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PleitoGeralCargo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseQuantidadeVagas);

            entity.HasOne(e => e.PleitoGeral)
                  .WithMany()
                  .HasForeignKey(e => e.IdPleitoGeral);

            entity.HasOne(e => e.Cargo)
                  .WithMany()
                  .HasForeignKey(e => e.IdCargo);

            entity.ToTable("pleito_geral_cargo", "tse");
        });
    }

    public static void ConfigurePleitoRegional(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PleitoRegional>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseKey).HasMaxLength(255);
            entity.Property(e => e.TseCodigo).HasMaxLength(50);
            entity.Property(e => e.TseDescricao).HasMaxLength(255);
            entity.Property(e => e.TseAbragenciaTipoDescricao).HasMaxLength(255);
            entity.Property(e => e.TseTurno).HasMaxLength(50);
            entity.Property(e => e.TseTipoCodigo).HasMaxLength(50);
            entity.Property(e => e.TseTipoDescricao).HasMaxLength(255);

            entity.HasOne(e => e.PleitoGeral)
                  .WithMany()
                  .HasForeignKey(e => e.IdPleitoGeral);

            entity.ToTable("pleito_regional", "tse");
        });
    }

    public static void ConfigurePleitoRegionalCargo(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PleitoRegionalCargo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseQuantidadeVagas);

            entity.HasOne(e => e.PleitoRegional)
                  .WithMany()
                  .HasForeignKey(e => e.IdPleitoRegional);

            entity.HasOne(e => e.Cargo)
                  .WithMany()
                  .HasForeignKey(e => e.IdCargo);

            entity.ToTable("pleito_regional_cargo", "tse");
        });
    }

    public static void ConfigureUnidadeFederativa(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UnidadeFederativa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.TseKey).HasMaxLength(255);
            entity.Property(e => e.TseSigla).HasMaxLength(50);
            entity.Property(e => e.TseNome).HasMaxLength(255);

            entity.HasOne(e => e.Pais)
                  .WithMany()
                  .HasForeignKey(e => e.IdPais);

            entity.ToTable("unidade_federativa", "tse");
        });
    }

    public static void ConfigureTSEEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureCandidatura();
        modelBuilder.ConfigureCandidaturaBem();
        modelBuilder.ConfigureCandidaturaMotivoCassacao();
        modelBuilder.ConfigureCargo();
        modelBuilder.ConfigureColigacaoPartidaria();
        modelBuilder.ConfigureColigacaoPartidariaPartido();
        modelBuilder.ConfigureFonte();
        modelBuilder.ConfigureFonteReferencia();
        modelBuilder.ConfigureMotivoCassacao();
        modelBuilder.ConfigureMunicipio();
        modelBuilder.ConfigurePais();
        modelBuilder.ConfigurePartido();
        modelBuilder.ConfigurePessoaFisica();
        modelBuilder.ConfigurePleitoGeral();
        modelBuilder.ConfigurePleitoGeralCargo();
        modelBuilder.ConfigurePleitoRegional();
        modelBuilder.ConfigurePleitoRegionalCargo();
        modelBuilder.ConfigureUnidadeFederativa();
    }
}
