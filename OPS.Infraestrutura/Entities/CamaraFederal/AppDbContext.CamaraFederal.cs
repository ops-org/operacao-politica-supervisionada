using Microsoft.EntityFrameworkCore;
using OPS.Infraestrutura.Entities.CamaraFederal;

namespace OPS.Infraestrutura;

public partial class AppDbContext
{
    // Câmara Federal (CF) Tables - Essential for DeputadoRepository
    public DbSet<Deputado> DeputadosFederais { get; set; }
    public DbSet<DespesaCamara> DespesasCamaraFederal { get; set; }
    public DbSet<LegislaturaCamara> LegislaturasCamaraFederal { get; set; }
    public DbSet<MandatoCamara> MandatosCamaraFederal { get; set; }
    public DbSet<DespesaTipoCamara> DespesaTiposCamaraFederal { get; set; }
    public DbSet<EspecificacaoTipo> EspecificacaoTipos { get; set; }
    public DbSet<Gabinete> GabinetesCamaraFederal { get; set; }
    public DbSet<Sessao> SessoesCamaraFederal { get; set; }
    public DbSet<SessaoPresenca> SessaoPresencas { get; set; }
    public DbSet<SecretarioCamara> SecretariosCamaraFederal { get; set; }
    public DbSet<Funcionario> FuncionariosCamaraFederal { get; set; }
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
    public DbSet<DeputadoCampeaoGasto> DeputadoCampeaoGastosCamara { get; set; }
    public DbSet<DeputadoCotaParlamentar> DeputadoCotaParlamentares { get; set; }
    public DbSet<DeputadoGabinete> DeputadoGabinetes { get; set; }
    public DbSet<DeputadoImovelFuncional> DeputadoImoveisFuncionais { get; set; }
    public DbSet<DeputadoMissaoOficial> DeputadoMissoesOficiais { get; set; }
    public DbSet<DeputadoRemuneracao> DeputadoRemuneracoes { get; set; }
    public DbSet<DeputadoVerbaGabinete> DeputadoVerbasGabinete { get; set; }
    public DbSet<DespesaResumoMensal> DespesaResumosMensaisCamara { get; set; }
    public DbSet<SecretarioHistorico> SecretarioHistoricos { get; set; }
    
}

public static class CamaraFederalEntityConfigurations
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
        entity.ToTable("cf_deputado", "camara");
    });
    }

    public static void ConfigureDespesa(this ModelBuilder modelBuilder)
    {
        // Configure Despesa (Câmara Federal)
        modelBuilder.Entity<DespesaCamara>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(e => e.Deputado).WithMany(d => d.Despesas).HasForeignKey(e => e.IdDeputado);
            entity.HasOne(e => e.Legislatura).WithMany(l => l.Despesas).HasForeignKey(e => e.IdLegislatura);
            entity.HasOne(e => e.Mandato).WithMany().HasForeignKey(e => e.IdMandato);
            entity.HasOne(e => e.DespesaTipo).WithMany(t => t.Despesas).HasForeignKey(e => e.IdDespesaTipo);
            entity.HasOne(e => e.EspecificacaoTipo).WithMany(t => t.Despesas).HasForeignKey(e => new { e.IdDespesaTipo, e.IdEspecificacao });
            entity.HasOne(e => e.TrechoViagem).WithMany(t => t.Despesas).HasForeignKey(e => e.IdTrechoViagem);
            entity.ToTable("cf_despesa", "camara");
        });
    }

    public static void ConfigureMandato(this ModelBuilder modelBuilder)
    {
        // Configure Mandato (Câmara Federal)
        modelBuilder.Entity<MandatoCamara>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasOne(e => e.Deputado).WithMany(d => d.Mandatos).HasForeignKey(e => e.IdDeputado);
            entity.HasOne(e => e.Legislatura).WithMany(l => l.Mandatos).HasForeignKey(e => e.IdLegislatura);
            entity.HasOne(e => e.Estado); //.WithMany(e => e.MandatosCamaraFederal).HasForeignKey(e => e.IdEstado);
            entity.HasOne(e => e.Partido); //.WithMany(p => p.Mandatos).HasForeignKey(e => e.IdPartido);
            entity.HasIndex(e => new { e.IdDeputado, e.IdLegislatura }).IsUnique();
            entity.ToTable("cf_mandato", "camara");
        });
    }

    public static void ConfigureEspecificacaoTipo(this ModelBuilder modelBuilder)
    {
        // Configure EspecificacaoTipo (Composite Key)
        modelBuilder.Entity<EspecificacaoTipo>(entity =>
        {
            entity.HasKey(e => new { e.IdDespesaTipo, e.IdEspecificacao });
            entity.HasOne(e => e.DespesaTipo).WithMany(t => t.EspecificacaoTipos).HasForeignKey(e => e.IdDespesaTipo);
            entity.ToTable("cf_especificacao_tipo", "camara");
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
            entity.ToTable("cf_sessao_presenca", "camara");
        });
    }

    public static void ConfigureLegislatura(this ModelBuilder modelBuilder)
    {
        // Configure Legislatura
        modelBuilder.Entity<LegislaturaCamara>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_legislatura", "camara");
        });
    }

    public static void ConfigureDespesaTipo(this ModelBuilder modelBuilder)
    {
        // Configure DespesaTipo
        modelBuilder.Entity<DespesaTipoCamara>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_despesa_tipo", "camara");
        });
    }

    public static void ConfigureGabinete(this ModelBuilder modelBuilder)
    {
        // Configure Gabinete
        modelBuilder.Entity<Gabinete>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_gabinete", "camara");
        });
    }

    public static void ConfigureSessao(this ModelBuilder modelBuilder)
    {
        // Configure Sessao
        modelBuilder.Entity<Sessao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_sessao", "camara");
        });
    }

    public static void ConfigureSecretario(this ModelBuilder modelBuilder)
    {
        // Configure Secretario
        modelBuilder.Entity<SecretarioCamara>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_secretario", "camara");
        });
    }

    public static void ConfigureFuncionario(this ModelBuilder modelBuilder)
    {
        // Configure Funcionario
        modelBuilder.Entity<Funcionario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_funcionario", "camara");
        });
    }

    public static void ConfigureFuncionarioContratacao(this ModelBuilder modelBuilder)
    {
        // Configure FuncionarioContratacao
        modelBuilder.Entity<FuncionarioContratacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_funcionario_contratacao", "camara");
        });
    }

    public static void ConfigureFuncionarioGrupoFuncional(this ModelBuilder modelBuilder)
    {
        // Configure FuncionarioGrupoFuncional
        modelBuilder.Entity<FuncionarioGrupoFuncional>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_funcionario_grupo_funcional", "camara");
        });
    }

    public static void ConfigureFuncionarioCargo(this ModelBuilder modelBuilder)
    {
        // Configure FuncionarioCargo
        modelBuilder.Entity<FuncionarioCargo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_funcionario_cargo", "camara");
        });
    }

    public static void ConfigureFuncionarioNivel(this ModelBuilder modelBuilder)
    {
        // Configure FuncionarioNivel
        modelBuilder.Entity<FuncionarioNivel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_funcionario_nivel", "camara");
        });
    }

    public static void ConfigureFuncionarioFuncaoComissionada(this ModelBuilder modelBuilder)
    {
        // Configure FuncionarioFuncaoComissionada
        modelBuilder.Entity<FuncionarioFuncaoComissionada>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_funcionario_funcao_comissionada", "camara");
        });
    }

    public static void ConfigureFuncionarioAreaAtuacao(this ModelBuilder modelBuilder)
    {
        // Configure FuncionarioAreaAtuacao
        modelBuilder.Entity<FuncionarioAreaAtuacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_funcionario_area_atuacao", "camara");
        });
    }

    public static void ConfigureFuncionarioLocalTrabalho(this ModelBuilder modelBuilder)
    {
        // Configure FuncionarioLocalTrabalho
        modelBuilder.Entity<FuncionarioLocalTrabalho>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_funcionario_local_trabalho", "camara");
        });
    }

    public static void ConfigureFuncionarioSituacao(this ModelBuilder modelBuilder)
    {
        // Configure FuncionarioSituacao
        modelBuilder.Entity<FuncionarioSituacao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_funcionario_situacao", "camara");
        });
    }

    public static void ConfigureFuncionarioRemuneracao(this ModelBuilder modelBuilder)
    {
        // Configure FuncionarioRemuneracao
        modelBuilder.Entity<FuncionarioRemuneracao>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_funcionario_remuneracao", "camara");
        });
    }

    public static void ConfigureFuncionarioTipoFolha(this ModelBuilder modelBuilder)
    {
        // Configure FuncionarioTipoFolha
        modelBuilder.Entity<FuncionarioTipoFolha>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_funcionario_tipo_folha", "camara");
        });
    }

    public static void ConfigureDeputadoAuxilioMoradia(this ModelBuilder modelBuilder)
    {
        // Configure DeputadoAuxilioMoradia
        modelBuilder.Entity<DeputadoAuxilioMoradia>(entity =>
        {
            entity.HasKey(e => new { e.IdDeputado, e.Ano, e.Mes });
            entity.ToTable("cf_deputado_auxilio_moradia", "camara");
        });
    }

    public static void ConfigureDeputadoCampeaoGasto(this ModelBuilder modelBuilder)
    {
        // Configure DeputadoCampeaoGasto
        modelBuilder.Entity<DeputadoCampeaoGasto>(entity =>
        {
            entity.HasKey(e => e.IdDeputado);
            entity.ToTable("cf_deputado_campeao_gasto", "camara");
        });
    }

    public static void ConfigureDeputadoCotaParlamentar(this ModelBuilder modelBuilder)
    {
        // Configure DeputadoCotaParlamentar
        modelBuilder.Entity<DeputadoCotaParlamentar>(entity =>
        {
            entity.HasKey(e => new { e.IdDeputado, e.Ano, e.Mes });
            entity.ToTable("cf_deputado_cota_parlamentar", "camara");
        });
    }

    public static void ConfigureDeputadoGabinete(this ModelBuilder modelBuilder)
    {
        // Configure DeputadoGabinete
        modelBuilder.Entity<DeputadoGabinete>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.ToTable("cf_deputado_gabinete", "camara");
        });
    }

    public static void ConfigureDeputadoImovelFuncional(this ModelBuilder modelBuilder)
    {
        // Configure DeputadoImovelFuncional
        modelBuilder.Entity<DeputadoImovelFuncional>(entity =>
        {
            entity.HasKey(e => e.IdDeputado);
            entity.ToTable("cf_deputado_imovel_funcional", "camara");
        });
    }

    public static void ConfigureDeputadoMissaoOficial(this ModelBuilder modelBuilder)
    {
        // Configure DeputadoMissaoOficial
        modelBuilder.Entity<DeputadoMissaoOficial>(entity =>
        {
            entity.HasKey(e => e.IdDeputado);
            entity.ToTable("cf_deputado_missao_oficial", "camara");
        });
    }

    public static void ConfigureDeputadoRemuneracao(this ModelBuilder modelBuilder)
    {
        // Configure DeputadoRemuneracao
        modelBuilder.Entity<DeputadoRemuneracao>(entity =>
        {
            entity.HasKey(e => e.IdDeputado);
            entity.ToTable("cf_deputado_remuneracao", "camara");
        });
    }

    public static void ConfigureDeputadoVerbaGabinete(this ModelBuilder modelBuilder)
    {
        // Configure DeputadoVerbaGabinete
        modelBuilder.Entity<DeputadoVerbaGabinete>(entity =>
        {
            entity.HasKey(e => e.IdDeputado);
            entity.ToTable("cf_deputado_verba_gabinete", "camara");
        });
    }

    public static void ConfigureDespesaResumoMensal(this ModelBuilder modelBuilder)
    {
        // Configure DespesaResumoMensal
        modelBuilder.Entity<DespesaResumoMensal>(entity =>
        {
            entity.HasKey(e => new { e.Ano, e.Mes });
            entity.ToTable("cf_despesa_resumo_mensal", "camara");
        });
    }

    public static void ConfigureSecretarioHistorico(this ModelBuilder modelBuilder)
    {
        // Configure SecretarioHistorico
        modelBuilder.Entity<SecretarioHistorico>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("cf_secretario_historico", "camara");
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
        modelBuilder.ConfigureLegislatura();
        modelBuilder.ConfigureDespesaTipo();
        modelBuilder.ConfigureGabinete();
        modelBuilder.ConfigureSessao();
        modelBuilder.ConfigureSecretario();
        modelBuilder.ConfigureFuncionario();
        modelBuilder.ConfigureFuncionarioContratacao();
        modelBuilder.ConfigureFuncionarioGrupoFuncional();
        modelBuilder.ConfigureFuncionarioCargo();
        modelBuilder.ConfigureFuncionarioNivel();
        modelBuilder.ConfigureFuncionarioFuncaoComissionada();
        modelBuilder.ConfigureFuncionarioAreaAtuacao();
        modelBuilder.ConfigureFuncionarioLocalTrabalho();
        modelBuilder.ConfigureFuncionarioSituacao();
        modelBuilder.ConfigureFuncionarioRemuneracao();
        modelBuilder.ConfigureFuncionarioTipoFolha();
        modelBuilder.ConfigureDeputadoAuxilioMoradia();
        modelBuilder.ConfigureDeputadoCampeaoGasto();
        modelBuilder.ConfigureDeputadoCotaParlamentar();
        modelBuilder.ConfigureDeputadoGabinete();
        modelBuilder.ConfigureDeputadoImovelFuncional();
        modelBuilder.ConfigureDeputadoMissaoOficial();
        modelBuilder.ConfigureDeputadoRemuneracao();
        modelBuilder.ConfigureDeputadoVerbaGabinete();
        modelBuilder.ConfigureDespesaResumoMensal();
        modelBuilder.ConfigureSecretarioHistorico();
        
    }
}