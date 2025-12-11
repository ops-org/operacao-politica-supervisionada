using Microsoft.EntityFrameworkCore;

namespace OPS.Infraestrutura.Entities.CamaraFederal;

public partial class AppDbContext
{
    // Câmara Federal (CF) Tables
    public DbSet<Deputado> Deputados { get; set; }
    public DbSet<Despesa> Despesas { get; set; }
    public DbSet<Legislatura> Legislaturas { get; set; }
    public DbSet<Mandato> Mandatos { get; set; }
    public DbSet<DespesaTipo> DespesaTipos { get; set; }
    public DbSet<EspecificacaoTipo> EspecificacaoTipos { get; set; }
    public DbSet<Gabinete> Gabinetes { get; set; }
    public DbSet<Sessao> Sessoes { get; set; }
    public DbSet<SessaoPresenca> SessaoPresencas { get; set; }
    public DbSet<Secretario> Secretarios { get; set; }
    public DbSet<Funcionario> Funcionarios { get; set; }
    public DbSet<FuncionarioContratacao> FuncionarioContratacoes { get; set; }
    public DbSet<FuncionarioGrupoFuncional> FuncionarioGruposFuncionais { get; set; }
    public DbSet<FuncionarioCargo> FuncionarioCargos { get; set; }
    public DbSet<FuncionarioNivel> FuncionarioNiveis { get; set; }
    public DbSet<FuncionarioFuncaoComissionada> FuncionarioFuncoesComissionadas { get; set; }
    public DbSet<FuncionarioAreaAtuacao> FuncionarioAreasAtuacao { get; set; }
    public DbSet<FuncionarioLocalTrabalho> FuncionarioLocaisTrabalho { get; set; }
    public DbSet<FuncionarioSituacao> FuncionarioSituacoes { get; set; }
    public DbSet<FuncionarioRemuneracao> FuncionarioRemuneracoes { get; set; }
    public DbSet<FuncionarioTipoFolha> FuncionarioTipoFolhas { get; set; }

    // Additional CF Tables
    public DbSet<DeputadoAuxilioMoradia> DeputadoAuxilioMoradias { get; set; }
    public DbSet<DeputadoCampeaoGasto> DeputadoCampeaoGastos { get; set; }
    public DbSet<DeputadoCotaParlamentar> DeputadoCotaParlamentares { get; set; }
    public DbSet<DeputadoGabinete> DeputadoGabinetes { get; set; }
    public DbSet<DeputadoImovelFuncional> DeputadoImoveisFuncionais { get; set; }
    public DbSet<DeputadoMissaoOficial> DeputadoMissoesOficiais { get; set; }
    public DbSet<DeputadoRemuneracao> DeputadoRemuneracoes { get; set; }
    public DbSet<DeputadoVerbaGabinete> DeputadoVerbasGabinete { get; set; }
    public DbSet<Despesa53> Despesas53 { get; set; }
    public DbSet<Despesa54> Despesas54 { get; set; }
    public DbSet<Despesa55> Despesas55 { get; set; }
    public DbSet<Despesa56> Despesas56 { get; set; }
    public DbSet<Despesa57> Despesas57 { get; set; }
    public DbSet<DespesaResumoMensal> DespesaResumosMensais { get; set; }
    public DbSet<SecretarioHistorico> SecretarioHistoricos { get; set; }
    public DbSet<SenadorVerbaGabinete> SenadorVerbasGabinete { get; set; }
}

public static class CamaraFederalConfigurations
{
    public static void ConfigureDeputado(this ModelBuilder modelBuilder)
    {
        // Configure Deputado
        modelBuilder.Entity<Deputado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasOne(e => e.Partido).WithMany(p => p.DeputadosFederais).HasForeignKey(e => e.IdPartido);
            entity.HasOne(e => e.Estado).WithMany(e => e.DeputadosFedarais).HasForeignKey(e => e.IdEstado);
            entity.HasOne(e => e.EstadoNascimento).WithMany().HasForeignKey(e => e.IdEstadoNascimento);
            entity.HasOne(e => e.Gabinete).WithMany(g => g.Deputados).HasForeignKey(e => e.IdGabinete);
        });
    }

    public static void ConfigureDespesa(this ModelBuilder modelBuilder)
    {
        // Configure Despesa (Câmara Federal)
        modelBuilder.Entity<Despesa>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(e => e.Deputado).WithMany(d => d.Despesas).HasForeignKey(e => e.IdDeputado);
            entity.HasOne(e => e.Legislatura).WithMany(l => l.Despesas).HasForeignKey(e => e.IdLegislatura);
            entity.HasOne(e => e.Mandato).WithMany().HasForeignKey(e => e.IdMandato);
            entity.HasOne(e => e.DespesaTipo).WithMany(t => t.Despesas).HasForeignKey(e => e.IdDespesaTipo);
            entity.HasOne(e => e.EspecificacaoTipo).WithMany(t => t.Despesas).HasForeignKey(e => new { e.IdDespesaTipo, e.IdEspecificacao });
            entity.HasOne(e => e.TrechoViagem).WithMany(t => t.Despesas).HasForeignKey(e => e.IdTrechoViagem);
        });
    }

    public static void ConfigureMandato(this ModelBuilder modelBuilder)
    {
        // Configure Mandato (Câmara Federal)
        modelBuilder.Entity<Mandato>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(e => e.Deputado).WithMany(d => d.Mandatos).HasForeignKey(e => e.IdDeputado);
            entity.HasOne(e => e.Legislatura).WithMany(l => l.Mandatos).HasForeignKey(e => e.IdLegislatura);
            entity.HasOne(e => e.Estado); //.WithMany(e => e.MandatosCamaraFederal).HasForeignKey(e => e.IdEstado);
            entity.HasOne(e => e.Partido); //.WithMany(p => p.Mandatos).HasForeignKey(e => e.IdPartido);
            entity.HasIndex(e => new { e.IdDeputado, e.IdLegislatura }).IsUnique();
        });
    }

    public static void ConfigureEspecificacaoTipo(this ModelBuilder modelBuilder)
    {
        // Configure EspecificacaoTipo (Composite Key)
        modelBuilder.Entity<EspecificacaoTipo>(entity =>
        {
            entity.HasKey(e => new { e.IdDespesaTipo, e.IdEspecificacao });
            entity.HasOne(e => e.DespesaTipo).WithMany(t => t.EspecificacaoTipos).HasForeignKey(e => e.IdDespesaTipo);
        });
    }

    public static void ConfigureSessaoPresenca(this ModelBuilder modelBuilder)
    {
        // Configure SessaoPresenca
        modelBuilder.Entity<SessaoPresenca>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(e => e.Sessao).WithMany(s => s.SessaoPresencas).HasForeignKey(e => e.IdSessao);
            entity.HasOne(e => e.Deputado).WithMany(d => d.SessaoPresencas).HasForeignKey(e => e.IdDeputado);
        });
    }

    // Master method to apply all configurations
    public static void ConfigureCamaraFederalEntities(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureDeputado();
        modelBuilder.ConfigureDespesa();
        modelBuilder.ConfigureMandato();
        modelBuilder.ConfigureEspecificacaoTipo();
        modelBuilder.ConfigureSessaoPresenca();
    }
}