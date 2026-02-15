using System.Globalization;
using System.Text;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPS.Importador.Comum;
using OPS.Importador.Comum.Utilities;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.TSE;

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
insert into tse.candidato 
  (id_pais_nascimento, id_estado_nascimento, id_municipio_nascimento, cpf, nome, nome_social, data_hora_nascimento, numero_titulo_eleitoral)
select distinct 
  1, e.id, max(m.id), c.cpf, c.nome, max(c.nome_social), CAST(c.data_nascimento as TIMESTAMP), c.titulo_eleitoral
from temp.tse_candidatura c
left join estado e on e.sigla = c.sigla_unidade_federativa_nascimento
left join municipio m on e.id = m.id_estado and lower(unaccent(m.nome)) = lower(unaccent(c.municipio_nascimento))
left join tse.candidato pf on pf.cpf = c.cpf
where pf.id is null 
and c.cpf is NOT null
group by e.id, c.cpf, c.nome, c.nome_social, CAST(c.data_nascimento as TIMESTAMP), c.titulo_eleitoral
order by c.nome
ON CONFLICT DO NOTHING;");

            context.Database.ExecuteSqlRaw(@"
insert into tse.candidato 
  (id_pais_nascimento, id_estado_nascimento, id_municipio_nascimento, cpf, nome, nome_social, data_hora_nascimento, numero_titulo_eleitoral)
select distinct 
  1, e.id, m.id, c.cpf, c.nome, c.nome_social, CAST(c.data_nascimento as TIMESTAMP), c.titulo_eleitoral
from temp.tse_candidatura c
left join estado e on e.sigla = c.sigla_unidade_federativa_nascimento
left join municipio m on e.id = m.id_estado and lower(unaccent(m.nome)) = lower(unaccent(c.municipio_nascimento))
left join tse.candidato pf on lower(unaccent(pf.nome)) = lower(unaccent(c.nome))
where pf.id is null 
and c.cpf is null
order by c.nome;");

            context.Database.ExecuteSqlRaw(@"
select distinct codigo_legenda, legenda, tipo_agremiacao
from temp.tse_candidatura c
left join tse.coligacao_partidaria cp on c.codigo_legenda = cp.sequencial and lower(unaccent(cp.nome)) = lower(unaccent(c.legenda))
where cp.id is null");

            context.Database.ExecuteSqlRaw(@"
INSERT INTO tse.coligacao_partidaria_partido (id_coligacao_partidaria, id_partido)
select distinct cp.id, p.id
from temp.tse_candidatura c
join partido p on p.legenda = c.numero_partido::int4 and p.sigla = c.sigla_partido
join tse.coligacao_partidaria cp on cp.sequencial = c.codigo_legenda and lower(unaccent(cp.nome)) = lower(unaccent(c.legenda))
ON CONFLICT DO NOTHING;");

            context.Database.ExecuteSqlRaw(@"
INSERT INTO tse.coligacao_partidaria (tse_key, tse_sequencial_coligacao, tse_nome, tse_tipo_descricao)
select distinct codigo_legenda, codigo_legenda, legenda, tipo_agremiacao
from temp.tse_candidatos 
order by tipo_agremiacao, legenda
ON CONFLICT DO NOTHING;
");

            context.Database.ExecuteSqlRaw(@"
insert into tse.cargo (id, descricao)
select DISTINCT cast(codigo_cargo as int4), cargo
from temp.tse_candidatura c
left join tse.cargo cr on cast(c.codigo_cargo as int4) = cr.id
order by cast(codigo_cargo as int4)
ON CONFLICT DO NOTHING;");

            context.Database.ExecuteSqlRaw(@"
INSERT INTO tse.pleito
  (tse_key, codigo, descricao, turno, data_hora, tipo_codigo, tipo_descricao, abragencia_tipo_descricao, ano)  
select distinct 
   cast(codigo_eleicao as int4), codigo_eleicao, descricao, turno, cast(data_eleicao as TIMESTAMP), codigo_tipo_eleicao, tipo_eleicao, tipo_abrangencia_eleicao, cast(ano as int2)
from temp.tse_candidatura 
order by cast(codigo_eleicao as int4)
ON CONFLICT DO NOTHING;");

            context.Database.ExecuteSqlRaw(@"
INSERT INTO tse.pleito_cargo (id_pleito, id_cargo)
select distinct p.id, cr.id
from temp.tse_candidatura c
join tse.pleito p on p.tse_key = c.codigo_eleicao
join tse.cargo cr on cr.id = c.codigo_cargo::int4
ON CONFLICT DO NOTHING;

INSERT into public.estado_civil (id, nome)
SELECT DISTINCT codigo_estado_civil::int4, estado_civil
from temp.tse_candidatura c
LEFT JOIN public.estado_civil ec on ec.id = codigo_estado_civil::int4
where codigo_estado_civil is not null
and ec.id is null;

INSERT into public.etnia (id, nome)
SELECT DISTINCT codigo_etnia::int4, etnia
from temp.tse_candidatura c
LEFT JOIN public.etnia ec on ec.id = codigo_etnia::int4
where codigo_etnia is not null
and ec.id is null;

INSERT into public.genero (id, nome)
SELECT DISTINCT codigo_genero::int4, genero
from temp.tse_candidatura c
LEFT JOIN public.genero ec on ec.id = codigo_genero::int4
where codigo_genero is not null
and ec.id is null;

INSERT into public.grau_instrucao (id, nome)
SELECT DISTINCT codigo_grau_instrucao::int4, grau_instrucao
from temp.tse_candidatura c
LEFT JOIN public.grau_instrucao ec on ec.id = codigo_grau_instrucao::int4
where ec.id is null
and codigo_grau_instrucao is not null;

update public.profissao p
set codigo_tse = codigo_ocupacao::int4
FROM temp.tse_candidatura c 
where lower(unaccent(c.ocupacao)) = lower(unaccent(p.descricao))
and p.codigo_tse is null;

INSERT into public.profissao (codigo_tse, descricao)
SELECT DISTINCT codigo_ocupacao::int4, ocupacao
from temp.tse_candidatura c
LEFT JOIN public.profissao ec on ec.id = codigo_ocupacao::int4
where codigo_ocupacao is null;");

            context.Database.ExecuteSqlRaw(@"
INSERT INTO tse.candidatura (
  id_pleito,
  id_candidato,
  id_cargo,
  id_pais,
  id_estado,
  id_municipio,
  id_partido,
  id_coligacao_partidaria,
  id_estado_civil,
  id_genero,
  id_cor_raca,
  id_grau_instrucao,
  id_profissao,
  id_nacionalidade,
  id_situacao_turno,
  codigo,
  candidato_sequencial,
  candidato_numero,
  candidato_nome_urna,
  candidatura_situacao_codigo,
  candidatura_situacao_descricao,
  candidatura_protocolo,
  processo_numero,
  reeleicao,
  bens_declarar,
  email
 )
 select 
  pl.id as id_pleito,
  pf.id as  id_candidato,
  cr.id as id_cargo,
  1 as id_pais,
  uf.id as id_estado,
  null as id_municipio,
  pr.id as id_partido,
  cp.id as id_coligacao_partidaria,
  c.codigo_estado_civil::int4 as id_estado_civil,
  c.codigo_genero::int4 as id_genero,
  c.codigo_etnia::int4 as id_cor_raca,
  c.codigo_grau_instrucao::int4 as id_grau_instrucao,
  c.codigo_ocupacao::int4 as id_profissao,
  null as id_nacionalidade,
  c.codigo_totalizacao_turno::int4 as id_situacao_turno,
  
  concat(c.numero_sequencial, '_', c.cpf, '_', c.numero_protocolo_candidatura, '_', c.turno) as codigo,
  c.numero_sequencial,
  c.numero_urna,
  c.nome_urna,
  c.codigo_situacao_candidatura_pleito,
  c.situacao_candidatura_pleito,
  c.numero_protocolo_candidatura,
  c.numero_processo_candidatura,

  CASE WHEN c.concorre_reeleicao = 'S' THEN TRUE ELSE FALSE END,
  CASE WHEN c.declara_bens = 'S' THEN TRUE ELSE FALSE END,
  c.email
from temp.tse_candidatura c
join tse.pleito pl on pl.tse_key = c.codigo_eleicao
join tse.candidato pf on pf.cpf = c.cpf
join tse.cargo cr on cast(c.codigo_cargo as int4) = cr.id
left join estado uf on uf.sigla = c.sigla_unidade_eleitoral
-- left join municipio m on m.id_estado = uf.id and lower(unaccent(m.nome)) = lower(unaccent(c.codigo_municipio))
join partido pr on pr.legenda = c.numero_partido::int2 and pr.sigla = c.sigla_partido
left join tse.coligacao_partidaria cp on cp.sequencial = c.codigo_legenda and lower(unaccent(cp.nome)) = lower(unaccent(c.legenda))
where concat(c.numero_sequencial, '_', c.cpf, '_', c.numero_protocolo_candidatura, '_', c.turno) = '170000614098_44901895400__1';
");
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
