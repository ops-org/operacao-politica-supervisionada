using System.Data;
using System.Globalization;
using System.Text;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Utilities;
using OPS.Infraestrutura;

namespace OPS.Importador.SenadoFederal
{

    /// <summary>
    /// Senado Federal
    /// https://www12.senado.leg.br/dados-abertos
    /// </summary>
    public class ImportadorDespesasSenado : IImportadorDespesas
    {
        protected readonly ILogger<ImportadorDespesasSenado> logger;
        protected readonly IDbConnection connection;
        protected readonly AppDbContext dbContext;

        public string rootPath { get; init; }
        public string tempPath { get; init; }

        private int linhasProcessadasAno { get; set; }

        public bool importacaoIncremental { get; init; }

        public HttpClient httpClient { get; }

        public ImportadorDespesasSenado(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetService<ILogger<ImportadorDespesasSenado>>();
            connection = serviceProvider.GetService<IDbConnection>();
            dbContext = serviceProvider.GetService<AppDbContext>();

            var configuration = serviceProvider.GetService<IConfiguration>();
            rootPath = configuration["AppSettings:SiteRootFolder"];
            tempPath = configuration["AppSettings:SiteTempFolder"];
            importacaoIncremental = Convert.ToBoolean(configuration["AppSettings:ImportacaoDespesas:Incremental"] ?? "false");

            httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient("ResilientClient");
        }

        public async Task Importar(int ano)
        {
            logger.LogDebug("Despesas do(a) Senado de {Ano}", ano);

            Dictionary<string, string> arquivos = DefinirUrlOrigemCaminhoDestino(ano);

            foreach (var arquivo in arquivos)
            {
                var urlOrigem = arquivo.Key;
                var caminhoArquivo = arquivo.Value;

                var novoArquivoBaixado = BaixarArquivo(urlOrigem, caminhoArquivo);
                if (importacaoIncremental && !novoArquivoBaixado)
                {
                    logger.LogInformation("Importação ignorada para arquivo previamente importado!");
                    return;
                }

                try
                {
                    await ImportarDespesas(caminhoArquivo, ano);
                }
                catch (Exception ex)
                {

                    logger.LogError(ex, ex.Message);

#if !DEBUG
                    //Excluir o arquivo para tentar importar novamente na proxima execução
                    if(File.Exists(caminhoArquivo))
                        File.Delete(caminhoArquivo);
#endif

                }
            }
        }

        public Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
        {
            Dictionary<string, string> arquivos = new();

            // https://www12.senado.leg.br/transparencia/dados-abertos-transparencia/dados-abertos-ceaps
            // Arquivos disponiveis anualmente a partir de 2008
            var _urlOrigem = $"https://www.senado.leg.br/transparencia/LAI/verba/despesa_ceaps_{ano}.csv";
            var _caminhoArquivo = $"{tempPath}/SF-{ano}.csv";

            arquivos.Add(_urlOrigem, _caminhoArquivo);
            return arquivos;
        }

        protected bool BaixarArquivo(string urlOrigem, string caminhoArquivo)
        {
            var caminhoArquivoDb = caminhoArquivo.Replace(tempPath, "");

            if (importacaoIncremental && File.Exists(caminhoArquivo))
            {
                var arquivoDB = dbContext.ArquivoChecksums.FirstOrDefault(x => x.Nome == caminhoArquivoDb);
                if (arquivoDB != null && arquivoDB.Verificacao > DateTime.UtcNow.AddDays(-7))
                {
                    logger.LogWarning("Ignorando arquivo verificado recentemente '{CaminhoArquivo}' a partir de '{UrlOrigem}'", caminhoArquivo, urlOrigem);
                    return false;
                }
            }

            logger.LogDebug("Baixando arquivo '{CaminhoArquivo}' a partir de '{UrlOrigem}'", caminhoArquivo, urlOrigem);

            string diretorio = new FileInfo(caminhoArquivo).Directory.ToString();
            if (!Directory.Exists(diretorio))
                Directory.CreateDirectory(diretorio);

            var fileExt = Path.GetExtension(caminhoArquivo);
            var caminhoArquivoTmp = caminhoArquivo.Replace(fileExt, $"_tmp{fileExt}");
            if (File.Exists(caminhoArquivoTmp))
                File.Delete(caminhoArquivoTmp);

            //if (config.Estado != Estado.DistritoFederal)
            //    httpClientResilient.DownloadFile(urlOrigem, caminhoArquivoTmp).Wait();
            //else
            httpClient.DownloadFile(urlOrigem, caminhoArquivoTmp).Wait();

            string checksum = ChecksumCalculator.ComputeFileChecksum(caminhoArquivoTmp);
            var arquivoChecksum = dbContext.ArquivoChecksums.FirstOrDefault(x => x.Nome == caminhoArquivoDb);
            if (arquivoChecksum != null && arquivoChecksum.Checksum == checksum && File.Exists(caminhoArquivo))
            {
                arquivoChecksum.Verificacao = DateTime.UtcNow;

                logger.LogDebug("Arquivo '{CaminhoArquivo}' é identico ao já existente.", caminhoArquivo);

                if (File.Exists(caminhoArquivoTmp))
                    File.Delete(caminhoArquivoTmp);

                return false;
            }

            if (arquivoChecksum == null)
            {
                logger.LogDebug("Arquivo '{CaminhoArquivo}' é novo.", caminhoArquivo);

                dbContext.ArquivoChecksums.Add(new ArquivoChecksum()
                {
                    Nome = caminhoArquivoDb,
                    Checksum = checksum,
                    TamanhoBytes = (int)new FileInfo(caminhoArquivoTmp).Length,
                    Criacao = DateTime.UtcNow,
                    Atualizacao = DateTime.UtcNow,
                    Verificacao = DateTime.UtcNow
                });
            }
            else
            {
                if (arquivoChecksum.Checksum != checksum)
                {
                    logger.LogDebug("Arquivo '{CaminhoArquivo}' foi atualizado.", caminhoArquivo);

                    arquivoChecksum.Checksum = checksum;
                    arquivoChecksum.TamanhoBytes = (int)new FileInfo(caminhoArquivoTmp).Length;

                }

                arquivoChecksum.Atualizacao = DateTime.UtcNow;
                arquivoChecksum.Verificacao = DateTime.UtcNow;
            }

            dbContext.SaveChanges();
            File.Move(caminhoArquivoTmp, caminhoArquivo, true);
            return true;
        }

        public async Task ImportarDespesas(string caminhoArquivo, int ano)
        {
            linhasProcessadasAno = 0;
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            string sResumoValores = string.Empty;
            int lote = 0, linhasInserida = 0;
            var dc = new Dictionary<string, UInt64>();
            var gerarHash = ano >= (DateTime.Now.Year - 2);

            int indice = 0;
            int ANO = indice++;
            int MES = indice++;
            int SENADOR = indice++;
            int TIPO_DESPESA = indice++;
            int CNPJ_CPF = indice++;
            int FORNECEDOR = indice++;
            int DOCUMENTO = indice++;
            int DATA = indice++;
            int DETALHAMENTO = indice++;
            int VALOR_REEMBOLSADO = indice++;
            int COD_DOCUMENTO = indice++;

            //using (var context = dbContextFactory.CreateDbContext())
            {
                if (gerarHash)
                {
                    int hashIgnorado = 0;
                    using (var connection = dbContext.Database.GetDbConnection())
                    {
                        await connection.OpenAsync();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = $"select id, hash FROM senado.sf_despesa where ano={ano} and hash IS NOT NULL";
                            using (var dReader = await command.ExecuteReaderAsync())
                                while (await dReader.ReadAsync())
                                {
                                    var hex = Convert.ToHexString((byte[])dReader["hash"]);
                                    if (!dc.ContainsKey(hex))
                                        dc.Add(hex, (UInt64)dReader["id"]);
                                    else
                                        hashIgnorado++;
                                }
                        }
                    }

                    logger.LogDebug("{Total} Hashes Carregados", dc.Count);
                    if (hashIgnorado > 0)
                        logger.LogWarning("{Total} Hashes Duplicados", hashIgnorado);
                }

                await LimpaDespesaTemporaria(dbContext);

                using (var reader = new StreamReader(caminhoArquivo, Encoding.GetEncoding("ISO-8859-1")))
                {
                    short count = 0;

                    while (!reader.EndOfStream)
                    {
                        count++;

                        var linha = reader.ReadLine();
                        if (string.IsNullOrEmpty(linha))
                            continue;
                        if (!linha.EndsWith("\""))
                            linha += reader.ReadLine();

                        var valores = ImportacaoUtils.ParseCsvRowToList(@""";""", linha);

                        if (count == 1) //Pula primeira linha, data da atualização
                            continue;

                        if (count == 2)
                        {
                            if (
                                    (valores[ANO] != "ANO") ||
                                    (valores[MES] != "MES") ||
                                    (valores[SENADOR] != "SENADOR") ||
                                    (valores[TIPO_DESPESA] != "TIPO_DESPESA") ||
                                    (valores[CNPJ_CPF] != "CNPJ_CPF") ||
                                    (valores[FORNECEDOR] != "FORNECEDOR") ||
                                    (valores[DOCUMENTO] != "DOCUMENTO") ||
                                    (valores[DATA] != "DATA") ||
                                    (valores[DETALHAMENTO] != "DETALHAMENTO") ||
                                    (valores[VALOR_REEMBOLSADO] != "VALOR_REEMBOLSADO") ||
                                    (valores[COD_DOCUMENTO] != "COD_DOCUMENTO")

                                )

                            {
                                throw new Exception("Mudança de integração detectada para o Senado Federal");
                            }

                            // Pular linha de titulo
                            continue;
                        }

                        var parameters = new[]
                        {
                            new NpgsqlParameter("ano", Convert.ToInt32(valores[ANO])),
                            new NpgsqlParameter("mes", Convert.ToInt32(valores[MES])),
                            new NpgsqlParameter("senador", valores[SENADOR]),
                            new NpgsqlParameter("tipo_despesa", valores[TIPO_DESPESA]),
                            new NpgsqlParameter("cnpj_cpf", !string.IsNullOrEmpty(valores[CNPJ_CPF]) ? Utils.RemoveCaracteresNaoNumericos(valores[CNPJ_CPF]) : ""),
                            new NpgsqlParameter("fornecedor", valores[FORNECEDOR]),
                            new NpgsqlParameter("documento", valores[DOCUMENTO]),
                            new NpgsqlParameter("data", !string.IsNullOrEmpty(valores[DATA]) ? (object)Convert.ToDateTime(valores[DATA], cultureInfo) : DBNull.Value),
                            new NpgsqlParameter("detalhamento", valores[DETALHAMENTO]),
                            new NpgsqlParameter("valor_reembolsado", Convert.ToDouble(valores[VALOR_REEMBOLSADO], cultureInfo)),
                            new NpgsqlParameter("cod_documento", Convert.ToUInt64(valores[COD_DOCUMENTO], cultureInfo))
                        };

                        linhasProcessadasAno++;
                        byte[] hash = null;
                        if (gerarHash)
                        {
                            hash = Utils.SHA1Hash(string.Join(",", parameters.Select(p => p.Value?.ToString() ?? "")));
                            var key = Convert.ToHexString(hash);
                            if (dc.Remove(key))
                            {
                                continue;
                            }
                        }

                        var allParameters = parameters.Concat(new[] { new NpgsqlParameter("hash", hash) }).ToArray();

                        await dbContext.Database.ExecuteSqlRawAsync(
                            @"INSERT INTO temp.sf_despesa_temp (
						ano, mes, senador, tipo_despesa, cnpj_cpf, fornecedor, documento, data, detalhamento, valor_reembolsado, cod_documento, hash
					) VALUES (
						@ano, @mes, @senador, @tipo_despesa, @cnpj_cpf, @fornecedor, @documento, @data, @detalhamento, @valor_reembolsado, @cod_documento, @hash
					)", allParameters);

                        if (linhasInserida++ == 10000)
                        {
                            logger.LogInformation("Processando lote {Lote}", ++lote);
                            await ProcessarDespesasTemp(dbContext, ano);
                            linhasInserida = 0;
                        }
                    }
                }

                if (dc.Values.Any())
                {
                    foreach (var id in dc.Values)
                    {
                        await dbContext.Database.ExecuteSqlRawAsync("delete FROM senado.sf_despesa where id=@id", new NpgsqlParameter("id", id));
                    }

                    logger.LogInformation("Removendo {Total} despesas", dc.Values.Count);
                }

                if (linhasInserida > 0)
                {
                    logger.LogInformation("Processando lote {Lote}", ++lote);
                    await ProcessarDespesasTemp(dbContext, ano);
                }

                await ValidaImportacao(dbContext, ano);

                if (ano == DateTime.Now.Year)
                {
                    await AtualizaParlamentarValores(dbContext);
                    await AtualizaCampeoesGastos(dbContext);
                    await AtualizaResumoMensal(dbContext);
                }
            }
        }

        private async Task ProcessarDespesasTemp(AppDbContext context, int ano)
        {
            await CorrigeDespesas(context);
            //InsereSenadorFaltante(context);
            await InsereFornecedorFaltante(context);
            await InsereDespesaFinal(context);
            await LimpaDespesaTemporaria(context);
        }

        public virtual async Task ValidaImportacao(AppDbContext context, int ano)
        {
            logger.LogDebug("Validar importação");

            var totalFinal = context.Database.GetDbConnection().ExecuteScalar<int>($@"
select count(1) 
FROM senado.sf_despesa d 
where d.ano_mes between {ano}01 and {ano}12");

            if (linhasProcessadasAno != totalFinal)
                logger.LogError("Totais divergentes! Arquivo: {LinhasArquivo} DB: {LinhasDB}",
                    linhasProcessadasAno, totalFinal);

            var despesasSemParlamentar = context.Database.GetDbConnection().ExecuteScalar<int>(@"
select count(1) from temp.sf_despesa_temp where senador  not in (select coalesce(nome_importacao, nome) FROM senado.sf_senador)");

            if (despesasSemParlamentar > 0)
                logger.LogError("Há deputados não identificados!");
        }

        private async Task CorrigeDespesas(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync(@"
			UPDATE temp.sf_despesa_temp 
			SET tipo_despesa = 'Aquisição de material de consumo para uso no escritório político' 
			WHERE tipo_despesa ILIKE 'Aquisição de material de consumo para uso no escritório político%';

			UPDATE temp.sf_despesa_temp 
			SET tipo_despesa = 'Contratação de consultorias, assessorias, pesquisas, trabalhos técnicos e outros serviços' 
			WHERE tipo_despesa ILIKE 'Contratação de consultorias, assessorias, pesquisas, trabalhos técnicos e outros serviços%';

			UPDATE temp.sf_despesa_temp
			SET tipo_despesa = '<Não especificado>'
			WHERE tipo_despesa = '';
		");
        }

        //private string InsereSenadorFaltante()
        //{
        //    //object total = banco.ExecuteScalar(@"select count(1) from temp.sf_despesa_temp where senador  not in (select coalesce(nome_importacao, nome) FROM senado.sf_senador);");
        //    //if (Convert.ToInt32(total) > 0)
        //    //{
        //    //	CarregaSenadoresAtuais();

        //    //object total = banco.ExecuteScalar(@"select count(1) from temp.sf_despesa_temp where senador  not in (select coalesce(nome_importacao, nome) FROM senado.sf_senador);");
        //    //if (Convert.ToInt32(total) > 0)
        //    //{
        //    //    throw new Exception("Existem despesas de senadores que não estão cadastrados!");
        //    //}
        //    //}

        //    return string.Empty;
        //}

        private async Task<string> InsereFornecedorFaltante(AppDbContext context)
        {
            var rowsAffected = await context.Database.ExecuteSqlRawAsync(@"
			INSERT INTO fornecedor (nome, cnpj_cpf)
			select MAX(dt.fornecedor), dt.cnpj_cpf
			from temp.sf_despesa_temp dt
			left join fornecedor f on f.cnpj_cpf = dt.cnpj_cpf
			where dt.cnpj_cpf is not null
			and f.id is null
			GROUP BY dt.cnpj_cpf;
		");

            if (rowsAffected > 0)
                logger.LogInformation("{Qtd} Fornecedores cadastrados.", rowsAffected);

            return string.Empty;
        }

        private async Task InsereDespesaFinal(AppDbContext context)
        {
            var rowsAffected = await context.Database.ExecuteSqlRawAsync(@"
				ALTER TABLE sf_despesa DISABLE KEYS;

				INSERT IGNORE INTO sf_despesa (
                    id,
					id_sf_senador,
					id_sf_despesa_tipo,
					id_fornecedor,
					ano_mes,
					ano,
					mes,
					documento,
					data_emissao,
					detalhamento,
					valor,
					hash
				)
				SELECT 
                    d.cod_documento,
					p.id,
					dt.id,
					f.id,
					concat(ano, LPAD(mes, 2, '0')),
					d.ano,
					d.mes,
					d.documento,
					d.data,
					d.detalhamento,
					d.valor_reembolsado,
					d.hash
				FROM temp.sf_despesa_temp d
				inner join sf_senador p on coalesce(p.nome_importacao, p.nome) = d.senador
				inner join sf_despesa_tipo dt on dt.descricao = d.tipo_despesa
				inner join fornecedor f on f.cnpj_cpf = d.cnpj_cpf;
    
				ALTER TABLE sf_despesa ENABLE KEYS;
			", 3600);


            if (rowsAffected > 0)
                logger.LogInformation("{Qtd} despesas cadastradas.", rowsAffected);

            var totalTemp = context.Database.GetDbConnection().ExecuteScalar<int>("select count(1) from temp.sf_despesa_temp");
            if (rowsAffected != totalTemp)
                logger.LogWarning("Há {Qtd} registros que não foram importados corretamente!", totalTemp - rowsAffected);

            //         banco.ExecuteNonQuery(@"
            //	UPDATE temp.sf_despesa_temp t 
            //	inner join sf_senador p on coalesce(p.nome_importacao, p.nome) = t.senador
            //	inner join sf_despesa_tipo dt on dt.descricao = t.tipo_despesa
            //	inner join fornecedor f on f.cnpj_cpf = t.cnpj_cpf
            //             inner join sf_despesa d on d.id = t.cod_documento and p.id = d.id_sf_senador
            //             set
            //		d.id_sf_despesa_tipo = dt.id,
            //		d.id_fornecedor = f.id,
            //		d.ano_mes = concat(t.ano, LPAD(t.mes, 2, '0')),
            //		d.ano = t.ano,
            //		d.mes = t.mes,
            //		d.documento = t.documento,
            //		d.data_emissao = t.data,
            //		d.detalhamento = t.detalhamento,
            //		d.valor = t.valor_reembolsado,
            //		d.hash = t.hash
            //", 3600);

            //         if (banco.RowsAffected > 0)
            //         {
            //             retorno += banco.RowsAffected + "+ Despesa alterada! ";
            //         }

            //if (!string.IsNullOrEmpty(retorno.Trim()))
            //{
            //    return "<p>" + retorno.Trim() + "</p>";
            //}
        }

        private async Task LimpaDespesaTemporaria(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync(@"
			truncate table temp.sf_despesa_temp;
		");
        }

        public async Task AtualizaParlamentarValores(AppDbContext context)
        {
            await context.Database.ExecuteSqlRawAsync("UPDATE sf_senador SET valor_total_ceaps=0;");

            var dt = context.Database.GetDbConnection().Query<int>("select id FROM senado.sf_senador\n\t\t\t\t\tWHERE id IN (\n\t\t\t\t\tselect distinct id_sf_senador\n\t\t\t\t\tFROM senado.sf_despesa\n\t\t\t\t)", new NpgsqlParameter("ano", DateTime.Now.Year));

            foreach (var id in dt)
            {
                var valor_total_ceaps = context.Database.GetDbConnection().ExecuteScalar<int>("select sum(valor) FROM senado.sf_despesa where id_sf_senador=@id_sf_senador",
                    new NpgsqlParameter("id_sf_senador", id));

                await context.Database.ExecuteSqlRawAsync(
                    @"update sf_senador set \n\t\t\t\t\tvalor_total_ceaps=@valor_total_ceaps\n\t\t\t\t\twhere id=@id_sf_senador",
                    new NpgsqlParameter("valor_total_ceaps", valor_total_ceaps),
                    new NpgsqlParameter("id_sf_senador", id));
            }
        }

        public async Task AtualizaCampeoesGastos(AppDbContext context)
        {
            var strSql =
                @"truncate table sf_senador_campeao_gasto;
\t\t\tinsert into sf_senador_campeao_gasto
\t\t\tSELECT l1.id_sf_senador, d.nome, l1.valor_total, p.sigla, e.sigla
\t\t\tFROM (
\t\t\t\tSELECT 
\t\t\t\t\tl.id_sf_senador,
\t\t\t\t\tsum(l.valor) as valor_total
\t\t\t\tFROM  sf_despesa l
\t\t\t\twhere l.ano_mes >= 202302 
\t\t\t\tGROUP BY l.id_sf_senador
\t\t\t\torder by valor_total desc 
\t\t\t\tlimit 4
\t\t\t) l1 
\t\t\tINNER JOIN sf_senador d on d.id = l1.id_sf_senador
\t\t\tLEFT JOIN partido p on p.id = d.id_partido
\t\t\tLEFT JOIN estado e on e.id = d.id_estado;";

            await context.Database.ExecuteSqlRawAsync(strSql);
        }

        public async Task AtualizaResumoMensal(AppDbContext context)
        {
            var strSql =
                @"truncate table sf_despesa_resumo_mensal;
\t\t\t\tinsert into sf_despesa_resumo_mensal
\t\t\t\t(ano, mes, valor) (
\t\t\t\t\tselect ano, mes, sum(valor)
\t\t\t\t\tFROM senado.sf_despesa
\t\t\t\t\tgroup by ano, mes
\t\t\t\t);";

            await context.Database.ExecuteSqlRawAsync(strSql);
        }

        public void AtualizarDatasImportacaoDespesas(DateTime? dInicio = null, DateTime? dFim = null)
        {
            var importacao = dbContext.Importacoes.FirstOrDefault(x => x.Chave == "Senado");
            if (importacao == null)
            {
                importacao = new Importacao()
                {
                    Chave = "Senado"
                };
                dbContext.Importacoes.Add(importacao);
            }

            if (dInicio != null)
            {
                importacao.DespesasInicio = dInicio.Value;
                importacao.DespesasFim = null;
            }

            if (dFim != null)
            {
                importacao.DespesasFim = dFim.Value;

                var sql = "select min(data_emissao) as primeira_despesa, max(data_emissao) as ultima_despesa from senado.sf_despesa";
                using (var dReader = connection.ExecuteReader(sql))
                {
                    if (dReader.Read())
                    {
                        importacao.PrimeiraDespesa = dReader["primeira_despesa"] != DBNull.Value ? DateOnly.Parse(dReader["primeira_despesa"].ToString()) : (DateOnly?)null;
                        importacao.UltimaDespesa = dReader["ultima_despesa"] != DBNull.Value ? DateOnly.Parse(dReader["ultima_despesa"].ToString()) : (DateOnly?)null;
                    }
                }
            }

            dbContext.SaveChanges();
        }
    }
}