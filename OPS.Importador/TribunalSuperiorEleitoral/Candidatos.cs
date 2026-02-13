using System.Globalization;
using System.Text;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPS.Importador.Comum;
using OPS.Importador.Comum.Utilities;
using OPS.Infraestrutura;

namespace OPS.Importador.TribunalSuperiorEleitoral
{
    /// <summary>
    /// https://dadosabertos.tse.jus.br/dataset/
    /// Normalização: https://github.com/turicas/eleicoes-brasil/
    /// </summary>
    public class Candidatos
    {
        private readonly ILogger<Candidatos> logger;
        private readonly AppSettings appSettings;
        private readonly AppDbContext context;
        private readonly FileManager fileManager;
        private readonly string basePath;
        private readonly string candidatosBasePath = "https://cdn.tse.jus.br/estatistica/sead/odsele/";

        public Candidatos(ILogger<Candidatos> logger, AppDbContext context, FileManager fileManager, IOptions<AppSettings> appSettings)
        {
            this.context = context;
            this.fileManager = fileManager;
            this.logger = logger;
            this.appSettings = appSettings.Value;

            this.basePath = Path.Combine(this.appSettings.TempFolder, "TribunalSuperiorEleitoral");
        }


        public async Task ImportarCompleto(int ano)
        {
            // context.Database.ExecuteSqlRaw(@"TRUNCATE TABLE temp.tse_candidato, temp.tse_despesa_contratada, temp.tse_despesa_paga, temp.tse_receita, temp.tse_receita_doador_originario RESTART IDENTITY;");

            await ImportarCandidatos(ano);
            //ImportarDespesasPagas(@"C:\\temp\TribunalSuperiorEleitoral\despesas_pagas_candidatos_2018_BRASIL.csv");
            //ImportarDespesasContratadas(@"C:\\temp\TribunalSuperiorEleitoral\despesas_contratadas_candidatos_2018_BRASIL.csv");
            //ImportarReceitas(@"C:\\temp\TribunalSuperiorEleitoral\receitas_candidatos_2018_BRASIL.csv");
            //ImportarReceitasDoadorOriginario(@"C:\\temp\TribunalSuperiorEleitoral\receitas_candidatos_doador_originario_2018_BRASIL.csv");

            // await ProcesarTemp();
        }

        private async Task ProcesarTemp()
        {
            context.Database.ExecuteSqlRaw(@"
-- update temp.tse_candidatos set nome_social=null where nome_social='#NULO';
-- update temp.tse_candidatos set numero_federacao=null where numero_federacao='-1';
-- update temp.tse_candidatos set federacao=null where federacao='#NULO';
-- update temp.tse_candidatos set sigla_federacao=null where sigla_federacao='#NULO';
-- update temp.tse_candidatos set composicao_federacao=null where composicao_federacao='#NULO';
-- update temp.tse_candidatos set nome_social=null where nome_social='NÃO DIVULGÁVEL';
-- update temp.tse_candidatos set email=null where email='NÃO DIVULGÁVEL';
-- update temp.tse_candidatos set email=null where email='#NULO';
-- update temp.tse_candidatos set email=null where email='#NULO#';
-- update temp.tse_candidatos set email=LOWER(email) where email is not null;
-- update temp.tse_candidatos set codigo_totalizacao_turno=null where codigo_totalizacao_turno='-4';
-- update temp.tse_candidatos set totalizacao_turno=null where totalizacao_turno='#NULO';
-- update temp.tse_candidatos set codigo_totalizacao_turno=null where codigo_totalizacao_turno='-1';
-- update temp.tse_candidatos set totalizacao_turno=null where totalizacao_turno='#NULO#';
-- update temp.tse_candidatos set codigo_genero=null where codigo_genero='-4';
-- update temp.tse_candidatos set genero=null where genero='NÃO DIVULGÁVEL';
-- update temp.tse_candidatos set codigo_estado_civil=null where codigo_estado_civil='-4';
-- update temp.tse_candidatos set estado_civil=null where estado_civil='NÃO DIVULGÁVEL';
-- update temp.tse_candidatos set codigo_etnia=null where codigo_etnia='-4';
-- update temp.tse_candidatos set etnia=null where etnia='NÃO DIVULGÁVEL';
-- update temp.tse_candidatos set codigo_etnia=null where codigo_etnia='-3';
-- update temp.tse_candidatos set etnia=null where etnia='#NE';
-- update temp.tse_candidatos set codigo_grau_instrucao=null where codigo_grau_instrucao='-4';
-- update temp.tse_candidatos set grau_instrucao=null where grau_instrucao='NÃO DIVULGÁVEL';
-- update temp.tse_candidatos set codigo_ocupacao=null where codigo_ocupacao='-4';
-- update temp.tse_candidatos set ocupacao=null where ocupacao='NÃO DIVULGÁVEL';
-- update temp.tse_candidatos set codigo_situacao_candidatura_pleito=null where codigo_situacao_candidatura_pleito='-3';
-- update temp.tse_candidatos set situacao_candidatura_pleito=null where situacao_candidatura_pleito='#NE';
-- update temp.tse_candidatos set declara_bens=null where declara_bens='#NE';

update temp.tse_candidatos set data_nascimento=null where data_nascimento='';
update temp.tse_candidatos set data_nascimento=null where data_nascimento='19/25/1950';
");

//             context.Database.ExecuteSqlRaw(@"
// insert into tse.unidade_federativa (id_pais, tse_key, tse_sigla, tse_nome)
// select DISTINCT 1, sigla_unidade_eleitoral, sigla_unidade_federativa, unidade_eleitoral 
// from temp.tse_candidatos 
// order by unidade_eleitoral
// ON CONFLICT DO NOTHING;
// ");

//             context.Database.ExecuteSqlRaw(@"
// insert into tse.municipio (id_unidade_federativa, tse_key, tse_sigla, tse_nome)
// select DISTINCT uf.id, codigo_municipio_nascimento, sigla_unidade_federativa_nascimento, municipio_nascimento 
// from temp.tse_candidatos c
// join tse.unidade_federativa uf on uf.tse_key = c.sigla_unidade_federativa_nascimento
// where codigo_municipio_nascimento is not null
// order by codigo_municipio_nascimento
// ON CONFLICT DO NOTHING;
// ");

            context.Database.ExecuteSqlRaw(@"
-- Remover tse_key
insert into tse.cargo (tse_key, tse_codigo, tse_descricao)
select DISTINCT cast(codigo_cargo as int4), codigo_cargo, cargo
from temp.tse_candidatos 
order by cast(codigo_cargo as int4)
ON CONFLICT DO NOTHING;
");

//             context.Database.ExecuteSqlRaw(@"
// -- Remover tse_key
// insert into tse.partido (tse_key, tse_numero, tse_sigla, tse_nome)
// select distinct numero_partido, numero_partido, sigla_partido, partido
// from temp.tse_candidatos 
// order by partido
// ON CONFLICT DO NOTHING;
// ");

            context.Database.ExecuteSqlRaw(@"
-- Ajustar data_nascimento para data yyyy-MM-dd
insert into tse.pessoa_fisica 
  (id_pais_nascimento, id_unidade_federativa_nascimento, id_municipio_nascimento, 
    tse_key, tse_cpf, tse_nome, tse_nome_social, tse_data_hora_nascimento, tse_numero_titulo_eleitoral)
select distinct 
  1, uf.id, m.id, numero_sequencial, cpf, nome, nome_social, CAST(data_nascimento as TIMESTAMP), titulo_eleitoral
from temp.tse_candidatos c
left join tse.unidade_federativa uf on uf.tse_key = c.sigla_unidade_federativa_nascimento
left join tse.municipio m on m.tse_key = c.codigo_municipio_nascimento
order by nome
ON CONFLICT DO NOTHING;
");

            context.Database.ExecuteSqlRaw(@"
INSERT INTO tse.coligacao_partidaria (tse_key, tse_sequencial_coligacao, tse_nome, tse_tipo_descricao)
select distinct codigo_legenda, codigo_legenda, legenda, tipo_agremiacao
from temp.tse_candidatos 
order by tipo_agremiacao, legenda
ON CONFLICT DO NOTHING;
");

            context.Database.ExecuteSqlRaw(@"
INSERT INTO tse.coligacao_partidaria_partido (id_coligacao_partidaria, id_partido)
select distinct cp.id, p.id
from temp.tse_candidatos c
join tse.partido p on p.tse_key = c.numero_partido
join tse.coligacao_partidaria cp on cp.tse_key = c.codigo_legenda
ON CONFLICT DO NOTHING;
");

            context.Database.ExecuteSqlRaw(@"
-- Remover tse_key
INSERT INTO tse.pleito
  (tse_key, tse_codigo, tse_descricao, tse_turno, tse_data_hora, tse_tipo_codigo, tse_tipo_descricao, tse_abragencia_tipo_descricao, tse_ano)  
select distinct 
   cast(codigo_eleicao as int4), codigo_eleicao, descricao, turno, cast(data_eleicao as TIMESTAMP), codigo_tipo_eleicao, nome_tipo_eleicao, tipo_abrangencia_eleicao, cast(ano as int2)
from temp.tse_candidatos 
order by cast(codigo_eleicao as int4)
ON CONFLICT DO NOTHING;
");

            context.Database.ExecuteSqlRaw(@"
INSERT INTO tse.pleito_cargo (id_pleito, id_cargo)
select distinct p.id, cr.id
from temp.tse_candidatos c
join tse.pleito p on p.tse_key = c.codigo_eleicao
join tse.cargo cr on cr.tse_key = c.codigo_cargo
ON CONFLICT DO NOTHING;
");

            context.Database.ExecuteSqlRaw(@"
INSERT INTO tse.candidatura (
  id_pleito,
  id_pessoa_fisica,
  id_cargo,
  id_pais,
  id_unidade_federativa,
  id_municipio,
  id_partido,
  id_coligacao_partidaria,
  tse_key,
  tse_candidato_sequencial,
  tse_candidato_numero,
  tse_candidato_nome_urna,
  tse_candidatura_situacao_codigo,
  tse_candidatura_situacao_descricao,
  tse_candidatura_protocolo,
  tse_processo_numero,
  tse_ocupacao_codigo,
  tse_ocupacao_descricao,
  tse_genero_codigo,
  tse_genero_descricao,
  tse_grau_instrucao_codigo,
  tse_grau_instrucao_descricao,
  tse_estado_civil_codigo,
  tse_estado_civil_descricao,
  tse_cor_raca_codigo,
  tse_cor_raca_descricao,
  tse_nacionalidade_codigo,
  tse_nacionalidade_descricao,
  tse_situacao_turno_codigo,
  tse_situacao_turno_descricao,
  tse_reeleicao,
  tse_bens_declarar,
  tse_email
 )
 select 
  pl.id as id_pleito,
  pf.id as  id_pessoa_fisica,
  cr.id as id_cargo,
  1 as id_pais,
  uf.id as id_unidade_federativa,
  m.id as id_municipio,
  pr.id as id_partido,
  cp.id as id_coligacao_partidaria,
  concat(c.numero_sequencial, '_', c.cpf, '_', c.numero_protocolo_candidatura, '_', c.turno),
  c.numero_sequencial,
  c.numero_urna,
  c.nome_urna,
  c.codigo_situacao_candidatura_pleito,
  c.situacao_candidatura_pleito,
  c.numero_protocolo_candidatura,
  c.numero_processo_candidatura,
  c.codigo_ocupacao,
  c.ocupacao,
  c.codigo_genero,
  c.genero,
  c.codigo_grau_instrucao,
  c.grau_instrucao,
  c.codigo_estado_civil,
  c.estado_civil,
  c.codigo_etnia,
  c.etnia,
  null as tse_nacionalidade_codigo,
  null as tse_nacionalidade_descricao,
  c.codigo_totalizacao_turno,
  c.totalizacao_turno,
  CASE WHEN c.concorre_reeleicao = 'S' THEN TRUE ELSE FALSE END,
  CASE WHEN c.declara_bens = 'S' THEN TRUE ELSE FALSE END,
  c.email
from temp.tse_candidatos c
join tse.pleito pl on pl.tse_key = c.codigo_eleicao
join tse.pessoa_fisica pf on pf.tse_key = c.numero_sequencial
join tse.cargo cr on cr.tse_key = c.codigo_cargo
join tse.unidade_federativa uf on uf.tse_key = c.sigla_unidade_eleitoral
left join tse.municipio m on m.tse_key = c.codigo_municipio_nascimento
join tse.partido pr on pr.tse_key = c.numero_partido
left join tse.coligacao_partidaria cp on cp.tse_key = c.codigo_legenda;");
        }

        // consulta_cand/consulta_cand_2018.zip
        // consulta_cand_complementar/consulta_cand_complementar_2018.zip
        // bem_candidato/bem_candidato_2018.zip
        // consulta_coligacao/consulta_coligacao_2018.zip
        // consulta_vagas/consulta_vagas_2018.zip
        // motivo_cassacao/motivo_cassacao_2018.zip

        public async Task ImportarCandidatos(int ano)
        {
            var cultureInfo = new CultureInfo("pt-BR");
            string url = string.Concat(candidatosBasePath, "consulta_cand/consulta_cand_", ano, ".zip");
            string filename = Path.Combine(basePath, $"consulta_cand_{ano}.zip");

            var hasChanges = await fileManager.BaixarArquivo(context, url, filename, null);
            if (!hasChanges && !appSettings.ForceImport) return;

            var filenameCSV = string.Concat("consulta_cand_", ano, "_BRASIL.csv");
            fileManager.DescompactarArquivo(filename, filenameCSV);
            filename = Path.Combine(basePath, filenameCSV);

            using (var reader = new StreamReader(filename, Encoding.GetEncoding("ISO-8859-1")))
            using (var csv = new CsvReader(reader, cultureInfo))
            {
                csv.Context.RegisterClassMap<TseCandidatoMap>();
                var records = csv.GetRecords<TseCandidato>();

                var bulkService = new BulkInsertService<TseCandidato>();
                bulkService.BulkInsertNoTracking(context, records);
            }
        }

        public void ImportarDespesasPagas(string file)
        {
            var cultureInfoBR = new CultureInfo("pt-BR");

            using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
            using (var csv = new CsvReader(reader, cultureInfoBR))
            {
                csv.Context.RegisterClassMap<TseDespesaPagaMap>();
                var records = csv.GetRecords<TseDespesaPaga>();

                var bulkService = new BulkInsertService<TseDespesaPaga>();
                bulkService.BulkInsertNoTracking(context, records);
            }
        }


        public void ImportarDespesasContratadas(string file)
        {
            var cultureInfoBR = new CultureInfo("pt-BR");

            using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
            using (var csv = new CsvReader(reader, cultureInfoBR))
            {
                csv.Context.RegisterClassMap<TseDespesaContratadaMap>();
                var records = csv.GetRecords<TseDespesaContratada>();

                var bulkService = new BulkInsertService<TseDespesaContratada>();
                bulkService.BulkInsertNoTracking(context, records);
            }
        }


        public void ImportarReceitasDoadorOriginario(string file)
        {
            var cultureInfoBR = new CultureInfo("pt-BR");

            using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
            using (var csv = new CsvReader(reader, cultureInfoBR))
            {
                csv.Context.RegisterClassMap<TseReceitaDoadorOriginarioMap>();
                var records = csv.GetRecords<TseReceitaDoadorOriginario>();

                var bulkService = new BulkInsertService<TseReceitaDoadorOriginario>();
                bulkService.BulkInsertNoTracking(context, records);
            }
        }


        public void ImportarReceitas(string file)
        {
            var cultureInfoBR = new CultureInfo("pt-BR");

            using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
            using (var csv = new CsvReader(reader, cultureInfoBR))
            {
                csv.Context.RegisterClassMap<TseReceitaMap>();
                var records = csv.GetRecords<TseReceita>();

                var bulkService = new BulkInsertService<TseReceita>();
                bulkService.BulkInsertNoTracking(context, records);
            }
        }

    }
}
