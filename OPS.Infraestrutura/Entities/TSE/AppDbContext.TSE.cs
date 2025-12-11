using Microsoft.EntityFrameworkCore;

namespace OPS.Infraestrutura.Entities.TSE;

public partial class AppDbContext
{
    // TSE Tables
    public DbSet<Eleicao> Eleicoes { get; set; }
    public DbSet<EleicaoCandidato> EleicaoCandidatos { get; set; }
    public DbSet<EleicaoCandidatura> EleicaoCandidaturas { get; set; }
    public DbSet<EleicaoCargo> EleicaoCargos { get; set; }
    public DbSet<EleicaoDoacao> EleicaoDoacoes { get; set; }
}

public static class TSEConfigurations
{
    public static void ConfigureEleicao(this ModelBuilder modelBuilder)
    {
        // Configure Eleicao - tse_eleicao table
        modelBuilder.Entity<Eleicao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            
            entity.Property(e => e.Descricao).HasMaxLength(50);
            entity.Property(e => e.Turno).HasMaxLength(50);
            entity.Property(e => e.Tipo).HasMaxLength(50);
            entity.Property(e => e.Abrangencia).HasMaxLength(50);
            
            entity.ToTable("tse_eleicao");
        });
    }

    public static void ConfigureEleicaoCandidato(this ModelBuilder modelBuilder)
    {
        // Configure EleicaoCandidato - tse_eleicao_candidato table
        modelBuilder.Entity<EleicaoCandidato>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.Cpf).HasMaxLength(11);
            entity.Property(e => e.Nome).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            
            entity.HasIndex(e => e.Cpf).IsUnique();
            
            entity.ToTable("tse_eleicao_candidato");
        });
    }

    public static void ConfigureEleicaoCandidatura(this ModelBuilder modelBuilder)
    {
        // Configure EleicaoCandidatura - tse_eleicao_candidatura table
        modelBuilder.Entity<EleicaoCandidatura>(entity =>
        {
            entity.HasKey(e => new { e.Numero, e.Cargo, e.Ano, e.SiglaEstado });
            
            entity.Property(e => e.Numero).ValueGeneratedNever();
            entity.Property(e => e.Cargo).ValueGeneratedNever();
            entity.Property(e => e.Ano).ValueGeneratedNever();
            entity.Property(e => e.SiglaEstado).HasMaxLength(2).IsUnicode(false);
            
            entity.Property(e => e.SiglaPartido).HasMaxLength(50);
            entity.Property(e => e.SiglaPartidoVice).HasMaxLength(50);
            entity.Property(e => e.NomeUrna).HasMaxLength(255);
            entity.Property(e => e.NomeUrnaVice).HasMaxLength(255);
            entity.Property(e => e.Sequencia).HasMaxLength(50);
            entity.Property(e => e.SequenciaVice).HasMaxLength(50);
            
            entity.ToTable("tse_eleicao_candidatura");
        });
    }

    public static void ConfigureEleicaoCargo(this ModelBuilder modelBuilder)
    {
        // Configure EleicaoCargo - tse_eleicao_cargo table
        modelBuilder.Entity<EleicaoCargo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.Nome).HasMaxLength(50);
            
            entity.HasIndex(e => e.Nome).IsUnique();
            
            entity.ToTable("tse_eleicao_cargo");
        });
    }

    public static void ConfigureEleicaoDoacao(this ModelBuilder modelBuilder)
    {
        //央 Configure EleicaoDoacao pts - tse_eleicao_doacao table
        modelBuilder.Entity<EleicaoDoacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            
            entity.Property(e => e.NumDocumento).HasMaxLength(50);
            entity.Property(e => e.CnpjCpfDoador).HasMaxLength(14);
            entity.Property(e => e.RaizCnpjCpfDoador).HasMaxLength(14);
            entity.Property(e => e.ValorReceita).HasColumnType("decimal(10,2)");
            
            entity.ToTable("tse_eleicao_doacao");
        });
    }

    public static void ConfigureTSEEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureEleicao();
        modelBuilder.ConfigureEleicaoCandidato();
        modelBuilder.ConfigureEleicaoCandidatura();
        modelBuilder.ConfigureEleicaoCargo();
        modelBuilder.ConfigureEleicaoDoacao();
    }
}
