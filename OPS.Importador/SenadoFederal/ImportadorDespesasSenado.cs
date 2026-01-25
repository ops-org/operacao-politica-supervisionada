using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using OPS.Core.Utilities;
using OPS.Importador.Comum;
using OPS.Importador.Comum.Despesa;
using OPS.Importador.Comum.Utilities;
using OPS.Infraestrutura;

namespace OPS.Importador.SenadoFederal;


/// <summary>
/// Senado Federal
/// https://www12.senado.leg.br/dados-abertos
/// </summary>
public class ImportadorDespesasSenado : IImportadorDespesas
{
    protected readonly ILogger<ImportadorDespesasSenado> logger;
    protected readonly AppSettings appSettings;
    protected readonly FileManager fileManager;
    protected readonly AppDbContext dbContext;
    protected readonly HttpClient httpClient;

    private int linhasProcessadasAno { get; set; }

    public ImportadorDespesasSenado(IServiceProvider serviceProvider)
    {
        appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        fileManager = serviceProvider.GetRequiredService<FileManager>();
        logger = serviceProvider.GetRequiredService<ILogger<ImportadorDespesasSenado>>();
        httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("ResilientClient");
    }

    public async Task Importar(int ano)
    {
        logger.LogDebug("Despesas do(a) Senado de {Ano}", ano);

        Dictionary<string, string> arquivos = DefinirUrlOrigemCaminhoDestino(ano);

        foreach (var arquivo in arquivos)
        {
            var urlOrigem = arquivo.Key;
            var caminhoArquivo = arquivo.Value;

            var novoArquivoBaixado = await fileManager.BaixarArquivo(dbContext, urlOrigem, caminhoArquivo, null);
            if (!appSettings.ForceImport && !novoArquivoBaixado)
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
                fileManager.MoverArquivoComErro(caminhoArquivo);
            }
        }
    }

    public Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
    {
        Dictionary<string, string> arquivos = new();

        // https://www12.senado.leg.br/transparencia/dados-abertos-transparencia/dados-abertos-ceaps
        // Arquivos disponiveis anualmente a partir de 2008
        var _urlOrigem = $"https://www.senado.leg.br/transparencia/LAI/verba/despesa_ceaps_{ano}.csv";
        var _caminhoArquivo = Path.Combine(appSettings.TempFolder, "Senado", $"SF-{ano}.csv");

        arquivos.Add(_urlOrigem, _caminhoArquivo);
        return arquivos;
    }

    public void ImportarDespesas(string caminhoArquivo, int ano)
    {
        linhasProcessadasAno = 0;
        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
        string sResumoValores = string.Empty;
        int linhasInserida = 0;
        var dc = new Dictionary<string, long>();

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

        //using (var dbContext = dbContextFactory.CreateDbContext())
        {
            int hashIgnorado = 0;
            var connection = dbContext.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $"select id, hash FROM senado.sf_despesa where ano={ano} and hash IS NOT NULL";
                using (var dReader = command.ExecuteReader())
                    while (dReader.Read())
                    {
                        var hex = Convert.ToHexString((byte[])dReader["hash"]);
                        if (!dc.ContainsKey(hex))
                            dc.Add(hex, (long)dReader["id"]);
                        else
                            hashIgnorado++;
                    }
            }

            logger.LogInformation("{Total} Hashes Carregados", dc.Count);
            if (hashIgnorado > 0)
                logger.LogWarning("{Total} Hashes Duplicados", hashIgnorado);

            LimpaDespesaTemporaria();
            var despesasTemp = new List<SenadoDespesaTemp>();

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

                    var despesaTemp = new SenadoDespesaTemp()
                    {
                        Ano = Convert.ToInt32(valores[ANO]),
                        Mes = Convert.ToInt32(valores[MES]),
                        Senador = valores[SENADOR],
                        TipoDespesa = valores[TIPO_DESPESA],
                        CnpjCpf = !string.IsNullOrEmpty(valores[CNPJ_CPF]) ? Utils.RemoveCaracteresNaoNumericos(valores[CNPJ_CPF]) : "",
                        Fornecedor = valores[FORNECEDOR],
                        Documento = valores[DOCUMENTO],
                        Data = !string.IsNullOrEmpty(valores[DATA]) ? Convert.ToDateTime(valores[DATA], cultureInfo) : null,
                        Detalhamento = valores[DETALHAMENTO],
                        ValorReembolsado = Convert.ToDecimal(valores[VALOR_REEMBOLSADO], cultureInfo),
                        CodDocumento = Convert.ToUInt64(valores[COD_DOCUMENTO], cultureInfo),
                    };
                    linhasProcessadasAno++;

                    // Pediodo legal de apresentação de notas é de 90 dias.
                    var dataVigencia = new DateTime(despesaTemp.Ano.Value, despesaTemp.Mes.Value, 1);
                    if (despesaTemp.Data > dataVigencia.AddDays(90) || despesaTemp.Data < dataVigencia.AddDays(-90))
                    {
                        if (despesaTemp.Data > dataVigencia.AddDays(180) || despesaTemp.Data < dataVigencia.AddDays(-180))
                        {
                            // Diferença de mais de 180 dias (6 meses) entre a data de apresentação da nota (vigencia) e data do documento.
                            logger.LogError("Data da despesa muito diferente do ano/mês informado. Linha {Linha}, Senador: {Senador}, Data: {Data}, Ano/Mês: {Ano}/{Mes}",
                               linhasProcessadasAno + 2, despesaTemp.Senador, despesaTemp.Data?.ToString("yyyy-MM-dd"), despesaTemp.Ano, despesaTemp.Mes);

                            var monthLastDay = DateTime.DaysInMonth(despesaTemp.Ano.Value, despesaTemp.Mes.Value);
                            despesaTemp.Data = new DateTime(despesaTemp.Ano.Value, despesaTemp.Mes.Value, Math.Min(despesaTemp.Data.Value.Day, monthLastDay));
                        }
                        else
                        {
                            // Diferença de mais de 90 dias (3 meses) entre a data de apresentação da nota (vigencia) e data do documento.
                            logger.LogWarning("Data da despesa diferente do ano/mês informado. Linha {Linha}, Senador: {Senador}, Data: {Data}, Ano/Mês: {Ano}/{Mes}",
                               linhasProcessadasAno + 2, despesaTemp.Senador, despesaTemp.Data?.ToString("yyyy-MM-dd"), despesaTemp.Ano, despesaTemp.Mes);
                        }
                    }

                    var options = new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    };
                    var str = JsonSerializer.Serialize(despesaTemp, options);
                    despesaTemp.Hash = Utils.SHA1Hash(str);
                    var hex = Convert.ToHexString(despesaTemp.Hash);
                    if (dc.Remove(hex)) continue;

                    despesasTemp.Add(despesaTemp);
                    linhasInserida++;
                }
            }

            if (linhasInserida > 0)
            {
                var valorTotalProcessadoAno = despesasTemp.Sum(x => x.ValorReembolsado ?? 0);

                var bulkService = new BulkInsertService<SenadoDespesaTemp>();
                bulkService.BulkInsertNoTracking(dbContext, despesasTemp);

                logger.LogInformation("Processando {Itens} despesas com valor total de {ValorTotalDespesas}.", linhasInserida, valorTotalProcessadoAno);
            }

            if (dc.Values.Any())
            {
                dbContext.DespesasSenado
                    .AsNoTracking()
                    .Where(u => dc.Values.Contains(u.Id))
                    .ExecuteDelete();

                logger.LogInformation("{Total} despesas removidas.", dc.Values.Count);
            }

            if (linhasInserida > 0)
            {
                ProcessarDespesasTemp(ano);
                ValidaImportacao(ano);
            }

            if (ano == DateTime.Now.Year)
            {
                AtualizaParlamentarValores();
                AtualizaCampeoesGastos();
                AtualizaResumoMensal();
            }
        }
    }

    private void ProcessarDespesasTemp(int ano)
    {
        CorrigeDespesas();
        InsereFornecedorFaltante();
        InsereDespesaFinal();
        LimpaDespesaTemporaria();
    }

    public virtual void ValidaImportacao(int ano)
    {
        logger.LogDebug("Validar importação");

        var totalFinal = dbContext.Database.GetDbConnection().ExecuteScalar<int>(
            $@"select count(1) FROM senado.sf_despesa d WHERE d.ano_mes between {ano}01 and {ano}12");

        if (linhasProcessadasAno != totalFinal)
            logger.LogError("Quantidades divergentes! Arquivo: {LinhasArquivo} DB: {LinhasDB}", linhasProcessadasAno, totalFinal);

        var despesasSemParlamentar = dbContext.Database.GetDbConnection().ExecuteScalar<int>(
            @"select count(1) from temp.sf_despesa_temp where lower(senador) not in (select lower(coalesce(nome_importacao, nome)) FROM senado.sf_senador)");

        if (despesasSemParlamentar > 0)
            logger.LogError("Há senadores não identificados!");
    }

    private void CorrigeDespesas()
    {
        dbContext.Database.ExecuteSqlRaw(@"
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

    private void InsereFornecedorFaltante()
    {
        var rowsAffected = dbContext.Database.ExecuteSqlRaw(@"
			    INSERT INTO fornecedor.fornecedor (nome, cnpj_cpf)
			    SELECT MAX(dt.fornecedor), dt.cnpj_cpf
			    FROM temp.sf_despesa_temp dt
			    LEFT JOIN fornecedor.fornecedor f on f.cnpj_cpf = dt.cnpj_cpf
			    WHERE dt.cnpj_cpf is not null
			    AND f.id is null
			    GROUP BY dt.cnpj_cpf;
		    ");

        if (rowsAffected > 0)
            logger.LogInformation("{Qtd} fornecedores cadastrados.", rowsAffected);
    }

    private void InsereDespesaFinal()
    {
        var rowsAffected = dbContext.Database.ExecuteSqlRaw(@"
				INSERT INTO senado.sf_despesa (
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
                    cast(concat(d.ano, LPAD(d.mes::text, 2, '0')) as bigint),
                    d.ano,
                    d.mes,
                    d.documento,
                    d.data,
                    d.detalhamento,
                    d.valor_reembolsado,
                    d.hash
                FROM temp.sf_despesa_temp d
                INNER JOIN senado.sf_senador p ON COALESCE(p.nome_importacao, p.nome) ILIKE d.senador and p.exerceu_mandato = true
                INNER JOIN senado.sf_despesa_tipo dt ON dt.descricao ILIKE d.tipo_despesa
                INNER JOIN fornecedor.fornecedor f ON f.cnpj_cpf = d.cnpj_cpf;
			");


        if (rowsAffected > 0)
            logger.LogInformation("{Qtd} despesas cadastradas.", rowsAffected);

        var totalTemp = dbContext.Database.GetDbConnection().ExecuteScalar<int>("select count(1) from temp.sf_despesa_temp");
        if (rowsAffected != totalTemp)
            logger.LogWarning("Há {Qtd} registros que não foram importados corretamente!", totalTemp - rowsAffected);
    }

    private void LimpaDespesaTemporaria()
    {
        dbContext.Database.ExecuteSqlRaw(@"
			    truncate table temp.sf_despesa_temp;
		    ");
    }

    public void AtualizaParlamentarValores()
    {
        dbContext.Database.ExecuteSqlRaw("UPDATE sf_senador SET valor_total_ceaps=0;");

        var dt = dbContext.Database.GetDbConnection().Query<int>(@"
select id FROM senado.sf_senador
WHERE id IN (
    select distinct id_sf_senador
    FROM senado.sf_despesa
)", new NpgsqlParameter("ano", DateTime.Now.Year));

        foreach (var id in dt)
        {
            var valor_total_ceaps = dbContext.Database.GetDbConnection().ExecuteScalar<int>(
                "select sum(valor) FROM senado.sf_despesa where id_sf_senador=@id_sf_senador",
                new NpgsqlParameter("id_sf_senador", id));

            dbContext.Database.ExecuteSqlRaw(
                @"update sf_senador set valor_total_ceaps=@valor_total_ceaps where id=@id_sf_senador",
                new NpgsqlParameter("valor_total_ceaps", valor_total_ceaps),
                new NpgsqlParameter("id_sf_senador", id));
        }
    }

    public void AtualizaCampeoesGastos()
    {
        var strSql = @"
TRUNCATE TABLE sf_senador_campeao_gasto;
INSERT INTO sf_senador_campeao_gasto
SELECT l1.id_sf_senador, d.nome, l1.valor_total, p.sigla, e.sigla
FROM (
    SELECT l.id_sf_senador, SUM(l.valor) as valor_total
    FROM  sf_despesa l
    WHERE l.ano_mes >= 202302 
    GROUP BY l.id_sf_senador
    ORDER BY valor_total desc 
    LIMIT 4
) l1 
JOIN sf_senador d on d.id = l1.id_sf_senador
LEFT JOIN partido p on p.id = d.id_partido
LEFT JOIN estado e on e.id = d.id_estado;";

        dbContext.Database.ExecuteSqlRaw(strSql);
    }

    public void AtualizaResumoMensal()
    {
        var strSql = @"
TRUNCATE TABLE sf_despesa_resumo_mensal;

INSERT INTO sf_despesa_resumo_mensal (ano, mes, valor)
SELECT ano, mes, sum(valor)
FROM senado.sf_despesa
GROUP BY ano, mes";

        dbContext.Database.ExecuteSqlRaw(strSql);
    }

    public void AtualizarDatasImportacaoDespesas(DateTime? dInicio = null, DateTime? dFim = null)
    {
        var importacao = dbContext.Importacoes.FirstOrDefault(x => x.Chave == "Senado");
        if (importacao == null)
        {
            importacao = new Importacao() { Chave = "Senado" };
            dbContext.Importacoes.Add(importacao);
        }

        if (dInicio != null)
        {
            importacao.DespesasInicio = dInicio.Value;
            importacao.DespesasFim = null;
        }

        if (dFim != null)
        {
            var primeiraEmissao = dbContext.DespesasSenado.Min(x => x.DataEmissao);
            var ultimaEmissao = dbContext.DespesasSenado.Max(x => x.DataEmissao);

            importacao.DespesasFim = dFim.Value;
            importacao.PrimeiraDespesa = primeiraEmissao;
            importacao.UltimaDespesa = ultimaEmissao;
        }

        dbContext.SaveChanges();
    }
}