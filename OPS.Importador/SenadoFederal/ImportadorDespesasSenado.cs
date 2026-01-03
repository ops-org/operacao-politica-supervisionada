using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using CsvHelper;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Utilities;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.SenadoFederal;
using OPS.Infraestrutura.Factories;
using RestSharp;

namespace OPS.Importador.SenadoFederal
{
    /// <summary>
    /// Column indexes for Senado Federal remuneration CSV file
    /// </summary>
    public enum SenadoRemuneracaoCsvColumns
    {
        VÍNCULO = 0,
        CATEGORIA = 1,
        CARGO = 2,
        REFERÊNCIA_CARGO = 3,
        SÍMBOLO_FUNÇÃO = 4,
        ANO_EXERCÍCIO = 5,
        LOTAÇÃO_EXERCÍCIO = 6,
        TIPO_FOLHA = 7,
        REMUN_BASICA = 8,
        VANT_PESSOAIS = 9,
        FUNC_COMISSIONADA = 10,
        GRAT_NATALINA = 11,
        HORAS_EXTRAS = 12,
        OUTRAS_EVENTUAIS = 13,
        ABONO_PERMANENCIA = 14,
        REVERSAO_TETO_CONST = 15,
        IMPOSTO_RENDA = 16,
        PREVIDENCIA = 17,
        FALTAS = 18,
        REM_LIQUIDA = 19,
        DIARIAS = 20,
        AUXILIOS = 21,
        VANT_INDENIZATORIAS = 22
    }

    /// <summary>
    /// Senado Federal
    /// https://www12.senado.leg.br/dados-abertos
    /// </summary>
    public class ImportadorDespesasSenado : IImportadorDespesas
    {
        protected readonly ILogger<ImportadorDespesasSenado> logger;
        protected readonly IDbConnection connection;
        protected readonly AppDbContextFactory dbContextFactory;

        public string rootPath { get; init; }
        public string tempPath { get; init; }

        private int linhasProcessadasAno { get; set; }

        public bool importacaoIncremental { get; init; }

        public HttpClient httpClient { get; }

        public ImportadorDespesasSenado(IServiceProvider serviceProvider)
        {
            logger = serviceProvider.GetService<ILogger<ImportadorDespesasSenado>>();
            connection = serviceProvider.GetService<IDbConnection>();
            dbContextFactory = serviceProvider.GetService<AppDbContextFactory>();

            var configuration = serviceProvider.GetService<IConfiguration>();
            rootPath = configuration["AppSettings:SiteRootFolder"];
            tempPath = configuration["AppSettings:SiteTempFolder"];
            importacaoIncremental = Convert.ToBoolean(configuration["AppSettings:ImportacaoDespesas:Incremental"] ?? "false");

            httpClient = serviceProvider.GetService<IHttpClientFactory>().CreateClient("ResilientClient");
        }

        public void Importar(int ano)
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
                    ImportarDespesas(caminhoArquivo, ano);
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
                var arquivoDB = connection.GetList<ArquivoChecksum>(new { nome = caminhoArquivoDb }).FirstOrDefault();
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
            var arquivoChecksum = connection.GetList<ArquivoChecksum>(new { nome = caminhoArquivoDb }).FirstOrDefault();
            if (arquivoChecksum != null && arquivoChecksum.Checksum == checksum && File.Exists(caminhoArquivo))
            {
                arquivoChecksum.Verificacao = DateTime.UtcNow;
                connection.Update(arquivoChecksum);

                logger.LogDebug("Arquivo '{CaminhoArquivo}' é identico ao já existente.", caminhoArquivo);

                if (File.Exists(caminhoArquivoTmp))
                    File.Delete(caminhoArquivoTmp);

                return false;
            }

            if (arquivoChecksum == null)
            {
                logger.LogDebug("Arquivo '{CaminhoArquivo}' é novo.", caminhoArquivo);

                connection.Insert(new ArquivoChecksum()
                {
                    Nome = caminhoArquivoDb,
                    Checksum = checksum,
                    TamanhoBytes = (uint)new FileInfo(caminhoArquivoTmp).Length,
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
                    arquivoChecksum.TamanhoBytes = (uint)new FileInfo(caminhoArquivoTmp).Length;

                }

                arquivoChecksum.Atualizacao = DateTime.UtcNow;
                arquivoChecksum.Verificacao = DateTime.UtcNow;
                connection.Update(arquivoChecksum);
            }

            File.Move(caminhoArquivoTmp, caminhoArquivo, true);
            return true;
        }

        public void ImportarDespesas(string caminhoArquivo, int ano)
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

            using (var banco = new AppDb())
            {
                if (gerarHash)
                {
                    int hashIgnorado = 0;
                    using (var dReader = banco.ExecuteReader($"select id, hash from sf_despesa where ano={ano} and hash IS NOT NULL"))
                        while (dReader.Read())
                        {
                            var hex = Convert.ToHexString((byte[])dReader["hash"]);
                            if (!dc.ContainsKey(hex))
                                dc.Add(hex, (UInt64)dReader["id"]);
                            else
                                hashIgnorado++;
                        }

                    logger.LogDebug("{Total} Hashes Carregados", dc.Count);
                    if (hashIgnorado > 0)
                        logger.LogWarning("{Total} Hashes Duplicados", hashIgnorado);
                }

                LimpaDespesaTemporaria(banco);

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

                        banco.AddParameter("ano", Convert.ToInt32(valores[ANO]));
                        banco.AddParameter("mes", Convert.ToInt32(valores[MES]));
                        banco.AddParameter("senador", valores[SENADOR]);
                        banco.AddParameter("tipo_despesa", valores[TIPO_DESPESA]);
                        banco.AddParameter("cnpj_cpf", !string.IsNullOrEmpty(valores[CNPJ_CPF]) ? Utils.RemoveCaracteresNaoNumericos(valores[CNPJ_CPF]) : "");
                        banco.AddParameter("fornecedor", valores[FORNECEDOR]);
                        banco.AddParameter("documento", valores[DOCUMENTO]);
                        banco.AddParameter("data", !string.IsNullOrEmpty(valores[DATA]) ? (object)Convert.ToDateTime(valores[DATA], cultureInfo) : DBNull.Value);
                        banco.AddParameter("detalhamento", valores[DETALHAMENTO]);
                        banco.AddParameter("valor_reembolsado", Convert.ToDouble(valores[VALOR_REEMBOLSADO], cultureInfo));
                        banco.AddParameter("cod_documento", Convert.ToUInt64(valores[COD_DOCUMENTO], cultureInfo));

                        linhasProcessadasAno++;
                        byte[] hash = null;
                        if (gerarHash)
                        {
                            hash = banco.ParametersHash();
                            var key = Convert.ToHexString(hash);
                            if (dc.Remove(key))
                            {
                                banco.ClearParameters();
                                continue;
                            }
                        }
                        banco.AddParameter("hash", hash);

                        banco.ExecuteNonQuery(
                            @"INSERT INTO ops_tmp.sf_despesa_temp (
								ano, mes, senador, tipo_despesa, cnpj_cpf, fornecedor, documento, `data`, detalhamento, valor_reembolsado, cod_documento, hash
							) VALUES (
								@ano, @mes, @senador, @tipo_despesa, @cnpj_cpf, @fornecedor, @documento, @data, @detalhamento, @valor_reembolsado, @cod_documento, @hash
							)");

                        if (linhasInserida++ == 10000)
                        {
                            logger.LogInformation("Processando lote {Lote}", ++lote);
                            ProcessarDespesasTemp(banco, ano);
                            linhasInserida = 0;
                        }
                    }
                }

                if (dc.Values.Any())
                {
                    foreach (var id in dc.Values)
                    {
                        banco.AddParameter("id", id);
                        banco.ExecuteNonQuery("delete from sf_despesa where id=@id");
                    }

                    logger.LogInformation("Removendo {Total} despesas", dc.Values.Count);
                }

                if (linhasInserida > 0)
                {
                    logger.LogInformation("Processando lote {Lote}", ++lote);
                    ProcessarDespesasTemp(banco, ano);
                }

                ValidaImportacao(banco, ano);

                if (ano == DateTime.Now.Year)
                {
                    AtualizaParlamentarValores();
                    AtualizaCampeoesGastos();
                    AtualizaResumoMensal();
                }
            }
        }

        private void ProcessarDespesasTemp(AppDb banco, int ano)
        {

            CorrigeDespesas(banco);
            //InsereSenadorFaltante(banco);
            InsereFornecedorFaltante(banco);
            InsereDespesaFinal(banco);
            LimpaDespesaTemporaria(banco);
        }

        public virtual void ValidaImportacao(AppDb banco, int ano)
        {
            logger.LogDebug("Validar importação");

            var totalFinal = connection.ExecuteScalar<int>(@$"
select count(1) 
from sf_despesa d 
where d.ano_mes between {ano}01 and {ano}12");

            if (linhasProcessadasAno != totalFinal)
                logger.LogError("Totais divergentes! Arquivo: {LinhasArquivo} DB: {LinhasDB}",
                    linhasProcessadasAno, totalFinal);

            var despesasSemParlamentar = connection.ExecuteScalar<int>(@$"
select count(1) from ops_tmp.sf_despesa_temp where senador  not in (select ifnull(nome_importacao, nome) from sf_senador);");

            if (despesasSemParlamentar > 0)
                logger.LogError("Há deputados não identificados!");
        }

        private void CorrigeDespesas(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				UPDATE ops_tmp.sf_despesa_temp 
				SET tipo_despesa = 'Aquisição de material de consumo para uso no escritório político' 
				WHERE tipo_despesa LIKE 'Aquisição de material de consumo para uso no escritório político%';

				UPDATE ops_tmp.sf_despesa_temp 
				SET tipo_despesa = 'Contratação de consultorias, assessorias, pesquisas, trabalhos técnicos e outros serviços' 
				WHERE tipo_despesa LIKE 'Contratação de consultorias, assessorias, pesquisas, trabalhos técnicos e outros serviços%';

                UPDATE ops_tmp.sf_despesa_temp
                SET tipo_despesa = '<Não especificado>'
                WHERE tipo_despesa = '';
			");
        }

        //private string InsereSenadorFaltante(AppDb banco)
        //{
        //    //object total = banco.ExecuteScalar(@"select count(1) from ops_tmp.sf_despesa_temp where senador  not in (select ifnull(nome_importacao, nome) from sf_senador);");
        //    //if (Convert.ToInt32(total) > 0)
        //    //{
        //    //	CarregaSenadoresAtuais();

        //    //object total = banco.ExecuteScalar(@"select count(1) from ops_tmp.sf_despesa_temp where senador  not in (select ifnull(nome_importacao, nome) from sf_senador);");
        //    //if (Convert.ToInt32(total) > 0)
        //    //{
        //    //    throw new Exception("Existem despesas de senadores que não estão cadastrados!");
        //    //}
        //    //}

        //    return string.Empty;
        //}

        private string InsereFornecedorFaltante(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				INSERT INTO fornecedor (nome, cnpj_cpf)
				select MAX(dt.fornecedor), dt.cnpj_cpf
				from ops_tmp.sf_despesa_temp dt
				left join fornecedor f on f.cnpj_cpf = dt.cnpj_cpf
				where dt.cnpj_cpf is not null
				and f.id is null
				GROUP BY dt.cnpj_cpf;
			");

            if (banco.RowsAffected > 0)
                logger.LogInformation("{Qtd} Fornecedores cadastrados.", banco.RowsAffected);

            return string.Empty;
        }

        private void InsereDespesaFinal(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
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
					d.`data`,
					d.detalhamento,
					d.valor_reembolsado,
					d.hash
				FROM ops_tmp.sf_despesa_temp d
				inner join sf_senador p on ifnull(p.nome_importacao, p.nome) = d.senador
				inner join sf_despesa_tipo dt on dt.descricao = d.tipo_despesa
				inner join fornecedor f on f.cnpj_cpf = d.cnpj_cpf;
    
				ALTER TABLE sf_despesa ENABLE KEYS;
			", 3600);


            if (banco.RowsAffected > 0)
                logger.LogInformation("{Qtd} despesas cadastradas.", banco.RowsAffected);

            var totalTemp = connection.ExecuteScalar<int>("select count(1) from ops_tmp.sf_despesa_temp");
            if (banco.RowsAffected != totalTemp)
                logger.LogWarning("Há {Qtd} registros que não foram importados corretamente!", totalTemp - banco.RowsAffected);

            //         banco.ExecuteNonQuery(@"
            //	UPDATE ops_tmp.sf_despesa_temp t 
            //	inner join sf_senador p on ifnull(p.nome_importacao, p.nome) = t.senador
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
            //		d.data_emissao = t.`data`,
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

        private void InsereDespesaFinalParcial(AppDb banco)
        {
            var dt = banco.GetTable(
                @"DROP TABLE IF EXISTS table_in_memory_d;
                CREATE TEMPORARY TABLE table_in_memory_d
                AS (
	                select id_sf_senador, mes, sum(valor) as total
	                from sf_despesa d
	                where ano = 2017
	                GROUP BY id_sf_senador, mes
                );

                DROP TABLE IF EXISTS table_in_memory_dt;
                CREATE TEMPORARY TABLE table_in_memory_dt
                AS (
	                select p.id as id_sf_senador, mes, sum(valor_reembolsado) as total
	                from ops_tmp.sf_despesa_temp d
                    inner join sf_senador p on p.nome = d.senador
	                GROUP BY p.id, mes
                );

                select dt.id_sf_senador, dt.mes
                from table_in_memory_dt dt
                left join table_in_memory_d d on dt.id_sf_senador = d.id_sf_senador and dt.mes = d.mes
                where (d.id_sf_senador is null or d.total <> dt.total)
                order by d.id_sf_senador, d.mes;
			    ", 3600);

            foreach (DataRow dr in dt.Rows)
            {
                banco.AddParameter("id_sf_senador", dr["id_sf_senador"]);
                banco.AddParameter("mes", dr["mes"]);
                banco.ExecuteNonQuery(@"DELETE FROM sf_despesa WHERE id_sf_senador=@id_sf_senador and mes=@mes");

                banco.AddParameter("id_sf_senador", dr["id_sf_senador"]);
                banco.AddParameter("mes", dr["mes"]);
                banco.ExecuteNonQuery(@"
				        INSERT INTO sf_despesa (
					        id_sf_senador,
					        id_sf_despesa_tipo,
					        id_fornecedor,
					        ano_mes,
					        ano,
					        mes,
					        documento,
					        data_emissao,
					        detalhamento,
					        valor
				        )
				        SELECT 
					        d.id,
					        dt.id,
					        f.id,
					        concat(ano, LPAD(mes, 2, '0')),
					        d.ano,
					        d.mes,
					        d.documento,
					        d.`data`,
					        d.detalhamento,
					        d.valor_reembolsado
					    from (
						    select d.*, p.id					        
						    from ops_tmp.sf_despesa_temp d
                            inner join sf_senador p on p.nome = d.senador
						    WHERE p.id=@id_sf_senador and mes=@mes
					    ) d
				        inner join sf_despesa_tipo dt on dt.descricao = d.tipo_despesa
				        inner join fornecedor f on f.cnpj_cpf = d.cnpj_cpf;
			        ", 3600);
            }
        }

        private void LimpaDespesaTemporaria(AppDb banco)
        {
            banco.ExecuteNonQuery(@"
				truncate table ops_tmp.sf_despesa_temp;
			");
        }

        public void AtualizaParlamentarValores()
        {
            connection.Execute("UPDATE sf_senador SET valor_total_ceaps=0;");

            var dt = connection.Query(@"select id from sf_senador
						WHERE id IN (
						select distinct id_sf_senador
						from sf_despesa
					)");
            object valor_total_ceaps;

            foreach (var dr in dt)
            {
                valor_total_ceaps = connection.ExecuteScalar("select sum(valor) from sf_despesa where id_sf_senador=@id_sf_senador;",
                    new { id_sf_senador = Convert.ToInt32(dr.id) });

                connection.Execute(
                    @"update sf_senador set 
						valor_total_ceaps=@valor_total_ceaps
						where id=@id_sf_senador", new
                    {
                        valor_total_ceaps = valor_total_ceaps,
                        id_sf_senador = Convert.ToInt32(dr.id)
                    }
                );
            }
        }

        public void AtualizaCampeoesGastos()
        {
            var strSql =
                @"truncate table sf_senador_campeao_gasto;
				insert into sf_senador_campeao_gasto
				SELECT l1.id_sf_senador, d.nome, l1.valor_total, p.sigla, e.sigla
				FROM (
					SELECT 
						l.id_sf_senador,
						sum(l.valor) as valor_total
					FROM  sf_despesa l
					where l.ano_mes >= 202302 
					GROUP BY l.id_sf_senador
					order by valor_total desc 
					limit 4
				) l1 
				INNER JOIN sf_senador d on d.id = l1.id_sf_senador
				LEFT JOIN partido p on p.id = d.id_partido
				LEFT JOIN estado e on e.id = d.id_estado;";

            connection.Execute(strSql);
        }

        public void AtualizaResumoMensal()
        {
            var strSql =
                @"truncate table sf_despesa_resumo_mensal;
					insert into sf_despesa_resumo_mensal
					(ano, mes, valor) (
						select ano, mes, sum(valor)
						from sf_despesa
						group by ano, mes
					);";

            connection.Execute(strSql);
        }

        /// <summary>
        ///  https://www.senado.leg.br/transparencia/rh/servidores/consulta_remuneracao.asp
        /// </summary>
        /// <param name="ano"></param>
        /// <param name="mes"></param>
        public void ImportarRemuneracao(int ano, int mes)
        {
            var anomes = Convert.ToInt32($"{ano:0000}{mes:00}");
            var urlOrigem = string.Format("https://www.senado.leg.br/transparencia/LAI/secrh/SF_ConsultaRemuneracaoServidoresParlamentares_{0}.csv", anomes);
            var caminhoArquivo = System.IO.Path.Combine(tempPath, "SF-RM-" + anomes + ".csv");

            try
            {
                var novoArquivoBaixado = BaixarArquivo(urlOrigem, caminhoArquivo);
                if (importacaoIncremental && !novoArquivoBaixado)
                {
                    logger.LogInformation("Importação ignorada para arquivo previamente importado!");
                    return;
                }

                CarregaRemuneracaoCsv(caminhoArquivo, anomes);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);

                if (File.Exists(caminhoArquivo))
                    File.Delete(caminhoArquivo);
            }
        }

        private Vinculo GetOrCreateVinculo(AppDbContext dbContext, string descricao, List<Vinculo> vinculos)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Vínculo description cannot be empty");

            var vinculo = vinculos.FirstOrDefault(v => v.Descricao.Equals(descricao, StringComparison.CurrentCultureIgnoreCase));
            if (vinculo == null)
            {
                vinculo = new Vinculo { Descricao = descricao };
                dbContext.Vinculos.Add(vinculo);
                dbContext.SaveChanges();
                vinculos.Add(vinculo);
            }
            return vinculo;
        }

        private Categoria GetOrCreateCategoria(AppDbContext dbContext, string descricao, List<Categoria> categorias)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Categoria description cannot be empty");

            var categoria = categorias.FirstOrDefault(c => c.Descricao == descricao);
            if (categoria == null)
            {
                categoria = new Categoria { Descricao = descricao };
                dbContext.Categorias.Add(categoria);
                dbContext.SaveChanges();
                categorias.Add(categoria);
            }
            return categoria;
        }

        private Cargo? GetOrCreateCargo(AppDbContext dbContext, string descricao, List<Cargo> cargos)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                return null;

            var cargo = cargos.FirstOrDefault(c => c.Descricao.Equals(descricao, StringComparison.CurrentCultureIgnoreCase));
            if (cargo == null)
            {
                cargo = new Cargo { Descricao = descricao };
                dbContext.Cargos.Add(cargo);
                dbContext.SaveChanges();
                cargos.Add(cargo);
            }
            return cargo;
        }

        private ReferenciaCargo? GetOrCreateReferenciaCargo(AppDbContext dbContext, string descricao, List<ReferenciaCargo> referenciaCargos)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                return null;

            var referenciaCargo = referenciaCargos.FirstOrDefault(l => l.Descricao.Equals(descricao, StringComparison.CurrentCultureIgnoreCase));
            if (referenciaCargo == null)
            {
                referenciaCargo = new ReferenciaCargo { Descricao = descricao };
                dbContext.ReferenciaCargos.Add(referenciaCargo);
                dbContext.SaveChanges();
                referenciaCargos.Add(referenciaCargo);
            }
            return referenciaCargo;
        }

        private Funcao? GetOrCreateFuncao(AppDbContext dbContext, string descricao, List<Funcao> funcoes)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                return null;

            var funcao = funcoes.FirstOrDefault(f => f.Descricao.Equals(descricao, StringComparison.CurrentCultureIgnoreCase));
            if (funcao == null)
            {
                funcao = new Funcao { Descricao = descricao };
                dbContext.Funcoes.Add(funcao);
                dbContext.SaveChanges();
                funcoes.Add(funcao);
            }
            return funcao;
        }

        private Lotacao GetOrCreateLotacao(AppDbContext dbContext, string descricao, List<Lotacao> lotacoes)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Lotação description cannot be empty");

            var lotacao = lotacoes.FirstOrDefault(l => l.Descricao.Equals(descricao, StringComparison.CurrentCultureIgnoreCase));
            if (lotacao == null)
            {
                lotacao = new Lotacao { Descricao = descricao };
                dbContext.Lotacoes.Add(lotacao);
                dbContext.SaveChanges();
                lotacoes.Add(lotacao);
            }
            return lotacao;
        }

        private TipoFolha GetOrCreateTipoFolha(AppDbContext dbContext, string descricao, List<TipoFolha> tipoFolhas)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                throw new ArgumentException("Tipo Folha description cannot be empty");

            var tipoFolha = tipoFolhas.FirstOrDefault(t => t.Descricao.Equals(descricao, StringComparison.CurrentCultureIgnoreCase));
            if (tipoFolha == null)
            {
                tipoFolha = new TipoFolha { Descricao = descricao };
                dbContext.TipoFolhas.Add(tipoFolha);
                dbContext.SaveChanges();
                tipoFolhas.Add(tipoFolha);
            }
            return tipoFolha;
        }

        private decimal? ParseDecimal(string value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (decimal.TryParse(value, NumberStyles.Any, cultureInfo, out decimal result))
                return result;

            return 0;
        }

        private void CarregaRemuneracaoCsv(string file, int anomes)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            // var lotacoes = dbContext.Lotacoes.Where(x => x.Descricao.Contains("Senador", StringComparison.CurrentCultureIgnoreCase)).ToList();
            using (var dbContext = dbContextFactory.CreateDbContext())
            {
                // Load all data upfront for caching
                var funcoes = dbContext.Funcoes.ToList();
                var cargos = dbContext.Cargos.ToList();
                var categorias = dbContext.Categorias.ToList();
                var vinculos = dbContext.Vinculos.ToList();
                var referenciaCargos = dbContext.ReferenciaCargos.ToList();
                var lotacoes = dbContext.Lotacoes.ToList();
                var tipoFolhas = dbContext.TipoFolhas.ToList();

                using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
                {
                    short linha = 0;

                    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")))
                    {
                        while (csv.Read())
                        {
                            linha++;
                            if (linha == 1) //Pula primeira linha, data da atualização
                                continue;

                            if (linha == 2)
                            {
                                if (
                                        (csv[(int)SenadoRemuneracaoCsvColumns.VÍNCULO] != "VÍNCULO") ||
                                        (csv[(int)SenadoRemuneracaoCsvColumns.VANT_INDENIZATORIAS].Trim() != "VANT_INDENIZATORIAS")
                                    )
                                {
                                    throw new Exception("Mudança de integração detectada para o Senado Federal");
                                }

                                // Pular linha de titulo
                                continue;
                            }

                            // Get or create related entities
                            var vinculo = GetOrCreateVinculo(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.VÍNCULO], vinculos);
                            var categoria = GetOrCreateCategoria(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.CATEGORIA], categorias);
                            var cargo = GetOrCreateCargo(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.CARGO], cargos);
                            var referenciaCargo = GetOrCreateReferenciaCargo(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.REFERÊNCIA_CARGO], referenciaCargos);
                            var funcao = GetOrCreateFuncao(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.SÍMBOLO_FUNÇÃO], funcoes);
                            var lotacao = GetOrCreateLotacao(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.LOTAÇÃO_EXERCÍCIO], lotacoes);
                            var tipoFolha = GetOrCreateTipoFolha(dbContext, csv[(int)SenadoRemuneracaoCsvColumns.TIPO_FOLHA], tipoFolhas);

                            // Parse numeric values
                            var remunBasica = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.REMUN_BASICA], cultureInfo);
                            var vantPessoais = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.VANT_PESSOAIS], cultureInfo);
                            var funcComissionada = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.FUNC_COMISSIONADA], cultureInfo);
                            var gratNatalina = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.GRAT_NATALINA], cultureInfo);
                            var horasExtras = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.HORAS_EXTRAS], cultureInfo);
                            var outrasEventuais = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.OUTRAS_EVENTUAIS], cultureInfo);
                            var abonoPermanencia = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.ABONO_PERMANENCIA], cultureInfo);
                            var reversaoTetoConst = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.REVERSAO_TETO_CONST], cultureInfo);
                            var impostoRenda = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.IMPOSTO_RENDA], cultureInfo);
                            var previdencia = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.PREVIDENCIA], cultureInfo);
                            var faltas = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.FALTAS], cultureInfo);
                            var remLiquida = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.REM_LIQUIDA], cultureInfo);
                            var diarias = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.DIARIAS], cultureInfo);
                            var auxilios = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.AUXILIOS], cultureInfo);
                            var vantIndenizatorias = ParseDecimal(csv[(int)SenadoRemuneracaoCsvColumns.VANT_INDENIZATORIAS], cultureInfo);

                            var custoTotal = (remLiquida ?? 0) - (impostoRenda ?? 0) - (previdencia ?? 0) - (faltas ?? 0) + (diarias ?? 0) + (auxilios ?? 0) + (vantIndenizatorias ?? 0);

                            var remuneracao = new Remuneracao
                            {
                                IdVinculo = vinculo.Id,
                                IdCategoria = categoria.Id,
                                IdCargo = cargo?.Id,
                                IdReferenciaCargo = referenciaCargo?.Id,
                                IdSimboloFuncao = funcao?.Id,
                                IdLotacao = lotacao.Id,
                                IdTipoFolha = tipoFolha.Id,
                                AnoMes = (uint)anomes,
                                Admissao = Convert.ToUInt16(csv[(int)SenadoRemuneracaoCsvColumns.ANO_EXERCÍCIO]),
                                RemunBasica = remunBasica,
                                VantPessoais = vantPessoais,
                                FuncComissionada = funcComissionada,
                                GratNatalina = gratNatalina,
                                HorasExtras = horasExtras,
                                OutrasEventuais = outrasEventuais,
                                AbonoPermanencia = abonoPermanencia,
                                ReversaoTetoConst = reversaoTetoConst,
                                ImpostoRenda = impostoRenda,
                                Previdencia = previdencia,
                                Faltas = faltas,
                                RemLiquida = remLiquida,
                                Diarias = diarias,
                                Auxilios = auxilios,
                                VantIndenizatorias = vantIndenizatorias,
                                CustoTotal = custoTotal
                            };

                            dbContext.Remuneracoes.Add(remuneracao);

                            if (linha % 1000 == 0)
                            {
                                dbContext.SaveChanges();
                                dbContext.ChangeTracker.Clear();
                            }
                        }
                    }

                    logger.LogInformation("{Itens} processados!", linha);
                    dbContext.SaveChanges();
                    dbContext.ChangeTracker.Clear();
                }

                // Update senator total remuneration
                var anoMesParam = new MySqlParameter("@ano_mes", anomes);
                dbContext.Database.ExecuteSqlRaw(@"
                    UPDATE sf_senador s
                    JOIN sf_lotacao l ON l.id_senador = s.id
                    JOIN sf_remuneracao r ON l.id = r.id_lotacao
                    SET valor_total_remuneracao = (
	                        SELECT SUM(custo_total) AS total
	                        FROM sf_remuneracao r
	                        JOIN sf_lotacao l ON l.id = r.id_lotacao
	                        where l.id_senador = s.id
	                    )
                    WHERE r.ano_mes = @ano_mes
			    ", anoMesParam);
            }
        }

        public void AtualizarDatasImportacaoDespesas(DateTime? dInicio = null, DateTime? dFim = null)
        {
            var importacao = connection.GetList<Importacao>(new { chave = "Senado" }).FirstOrDefault();
            if (importacao == null)
            {
                importacao = new Importacao()
                {
                    Chave = "Senado"
                };
                importacao.Id = (ushort)connection.Insert(importacao);
            }

            if (dInicio != null)
            {
                importacao.DespesasInicio = dInicio.Value;
                importacao.DespesasFim = null;
            }

            if (dFim != null)
            {
                importacao.DespesasFim = dFim.Value;

                var sql = "select min(data_emissao) as primeira_despesa, max(data_emissao) as ultima_despesa from sf_despesa";
                using (var dReader = connection.ExecuteReader(sql))
                {
                    if (dReader.Read())
                    {
                        importacao.PrimeiraDespesa = dReader["primeira_despesa"] != DBNull.Value ? Convert.ToDateTime(dReader["primeira_despesa"]) : (DateTime?)null;
                        importacao.UltimaDespesa = dReader["ultima_despesa"] != DBNull.Value ? Convert.ToDateTime(dReader["ultima_despesa"]) : (DateTime?)null;
                    }
                }
            }

            connection.Update(importacao);
        }
    }
}