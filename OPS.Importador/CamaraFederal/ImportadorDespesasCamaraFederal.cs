using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using AngleSharp;
using AngleSharp.Html.Dom;
using CsvHelper;
using Dapper;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPS.Core.Exceptions;
using OPS.Core.Utilities;
using OPS.Importador.Comum;
using OPS.Importador.Comum.Despesa;
using OPS.Importador.Comum.Utilities;
using OPS.Importador.Fornecedores;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Entities.CamaraFederal;
using RestSharp;

namespace OPS.Importador.CamaraFederal;

public class ImportadorDespesasCamaraFederal : IImportadorDespesas
{
    protected readonly ILogger<ImportadorDespesasCamaraFederal> logger;
    protected readonly AppSettings appSettings;
    protected readonly FileManager fileManager;

    protected IDbConnection connection { get { return dbContext.Database.GetDbConnection(); } }
    protected readonly AppDbContext dbContext;

    private int itensProcessadosAno { get; set; }
    private decimal valorTotalProcessadoAno { get; set; }

    private const int legislaturaAtual = 57;

    public HttpClient httpClient { get; }
    public CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

    public ImportadorDespesasCamaraFederal(IServiceProvider serviceProvider)
    {
        appSettings = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
        dbContext = serviceProvider.GetRequiredService<AppDbContext>();
        fileManager = serviceProvider.GetRequiredService<FileManager>();
        logger = serviceProvider.GetRequiredService<ILogger<ImportadorDespesasCamaraFederal>>();
        httpClient = serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("ResilientClient");
    }

    public void AtualizarDatasImportacaoDespesas(DateTime? dInicio = null, DateTime? dFim = null)
    {
        var importacao = dbContext.Importacoes.FirstOrDefault(x => x.Chave == "Camara Federal");
        if (importacao == null)
        {
            importacao = new Importacao()
            {
                Chave = "Camara Federal"
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

            var sql = "select min(data_emissao) as primeira_despesa, max(data_emissao) as ultima_despesa from camara.cf_despesa";
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

    #region Importação Dados CEAP CSV

    public async Task Importar(int ano)
    {
        logger.LogDebug("Despesas do(a) Camara Federal de {Ano}", ano);

        Dictionary<string, string> arquivos = DefinirUrlOrigemCaminhoDestino(ano);

        foreach (var arquivo in arquivos)
        {
            var urlOrigem = arquivo.Key;
            var caminhoArquivo = arquivo.Value;

            bool novoArquivoBaixado = await fileManager.BaixarArquivo(dbContext, urlOrigem, caminhoArquivo, null);
            if (!appSettings.ForceImport && !novoArquivoBaixado)
            {
                logger.LogInformation("Importação ignorada para arquivo previamente importado!");
                return;
            }

            try
            {
                if (caminhoArquivo.EndsWith(".zip"))
                {
                    DescompactarArquivo(caminhoArquivo);
                    caminhoArquivo = caminhoArquivo.Replace(".zip", "");
                }

                ImportarDespesas(caminhoArquivo, ano);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                fileManager.MoverArquivoComErro(caminhoArquivo);
            }
        }
    }

    protected void DescompactarArquivo(string caminhoArquivo)
    {
        logger.LogDebug("Descompactando Arquivo '{CaminhoArquivo}'", caminhoArquivo);

        var zip = new FastZip();
        zip.ExtractZip(caminhoArquivo, Path.GetDirectoryName(caminhoArquivo), null);
    }

    /// <summary>
    /// Baixa e Importa os Dados da CEAP
    /// </summary>
    /// <param name="atualDir"></param>
    /// <param name="ano"></param>
    /// <param name="completo"></param>
    public Dictionary<string, string> DefinirUrlOrigemCaminhoDestino(int ano)
    {
        var downloadUrl = "https://www.camara.leg.br/cotas/Ano-" + ano + ".csv.zip";
        var fullFileNameZip = Path.Combine(appSettings.TempFolder, "CamaraFederal", $"Ano-{ano}.csv.zip");

        Dictionary<string, string> arquivos = new();
        arquivos.Add(downloadUrl, fullFileNameZip);

        return arquivos;
    }

    private readonly string[] ColunasCEAP = {
            "txNomeParlamentar","cpf","ideCadastro","nuCarteiraParlamentar","nuLegislatura","sgUF","sgPartido","codLegislatura",
            "numSubCota","txtDescricao","numEspecificacaoSubCota","txtDescricaoEspecificacao","txtFornecedor","txtCNPJCPF",
            "txtNumero","indTipoDocumento","datEmissao","vlrDocumento","vlrGlosa","vlrLiquido","numMes","numAno","numParcela",
            "txtPassageiro","txtTrecho","numLote","numRessarcimento","datPagamentoRestituicao", "vlrRestituicao","nuDeputadoId","ideDocumento","urlDocumento"
        };

    public void ImportarDespesas(string file, int ano)
    {
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        LimpaDespesaTemporaria();

        itensProcessadosAno = 0;
        valorTotalProcessadoAno = 0;
        var totalColunas = ColunasCEAP.Length;
        var lstHash = new Dictionary<string, int>();

        //using (var transaction = dbContext.Database.BeginTransaction())
        {
            var sql = $"select id, hash FROM camara.cf_despesa where ano={ano} and hash IS NOT NULL";
            IEnumerable<dynamic> lstHashDB = connection.Query(sql);

            foreach (IDictionary<string, object> dReader in lstHashDB)
            {
                var hex = Convert.ToHexString((byte[])dReader["hash"]);
                if (!lstHash.ContainsKey(hex))
                    lstHash.Add(hex, (int)dReader["id"]);
                else
                    logger.LogError("Hash {HASH} esta duplicada na base de dados.", hex);
            }

            logger.LogInformation("{Total} Hashes Carregados", lstHash.Count());

            var despesasTemp = new List<DeputadoFederalDespesaTemp>();
            var cultureInfo = new CultureInfo("en-US");

            var config = new CsvHelper.Configuration.CsvConfiguration(cultureInfo);
            config.BadDataFound = null;
            config.Delimiter = ";";
            //config.MissingFieldFound = null;
            ////config.TrimOptions = TrimOptions.Trim;
            //config.HeaderValidated = ConfigurationFunctions.HeaderValidated;

            using (var reader = new StreamReader(file, Encoding.GetEncoding("UTF-8")))
            using (var csv = new CsvReader(reader, config))
            {
                if (csv.Read())
                {
                    for (int i = 0; i < totalColunas - 1; i++)
                    {
                        if (csv[i] != ColunasCEAP[i])
                        {
                            throw new Exception("Mudança de integraçao detectada para o Câmara Federal");
                        }
                    }
                }

                while (csv.Read())
                {
                    var idxColuna = 0;
                    var despesaTemp = new DeputadoFederalDespesaTemp();
                    despesaTemp.NomeParlamentar = csv.GetField(idxColuna++);
                    despesaTemp.Cpf = Utils.RemoveCaracteresNaoNumericos(csv.GetField(idxColuna++)).NullIfEmpty();
                    despesaTemp.IdDeputado = csv.GetField<long?>(idxColuna++);
                    despesaTemp.NumeroCarteiraParlamentar = csv.GetField<int?>(idxColuna++);
                    despesaTemp.Legislatura = csv.GetField<int?>(idxColuna++);
                    despesaTemp.SiglaUF = csv.GetField(idxColuna++).Replace("NA", string.Empty).NullIfEmpty();
                    despesaTemp.SiglaPartido = csv.GetField(idxColuna++).NullIfEmpty();
                    despesaTemp.CodigoLegislatura = csv.GetField<int?>(idxColuna++);
                    despesaTemp.NumeroSubCota = csv.GetField<int?>(idxColuna++);
                    despesaTemp.Descricao = csv.GetField(idxColuna++);
                    despesaTemp.NumeroEspecificacaoSubCota = csv.GetField<int?>(idxColuna++)?.NullIf(0);
                    despesaTemp.DescricaoEspecificacao = csv.GetField(idxColuna++).NullIfEmpty();
                    despesaTemp.Fornecedor = csv.GetField(idxColuna++);
                    despesaTemp.CnpjCpf = Utils.RemoveCaracteresNaoNumericos(csv.GetField(idxColuna++));
                    despesaTemp.Numero = csv.GetField(idxColuna++);
                    despesaTemp.TipoDocumento = csv.GetField<int?>(idxColuna++) ?? 0;
                    despesaTemp.DataEmissao = csv.GetField<DateOnly?>(idxColuna++); ;
                    try
                    {
                        despesaTemp.ValorDocumento = Convert.ToDecimal(csv.GetField(idxColuna++), cultureInfo);
                    }
                    catch (Exception)
                    { }
                    despesaTemp.ValorGlosa = Convert.ToDecimal(csv.GetField(idxColuna++), cultureInfo);
                    despesaTemp.ValorLiquido = Convert.ToDecimal(csv.GetField(idxColuna++), cultureInfo);
                    despesaTemp.Mes = csv.GetField<short>(idxColuna++);
                    despesaTemp.Ano = csv.GetField<short>(idxColuna++);
                    despesaTemp.Parcela = csv.GetField<int?>(idxColuna++)?.NullIf(0);
                    despesaTemp.Passageiro = csv.GetField(idxColuna++).NullIfEmpty();
                    despesaTemp.Trecho = csv.GetField(idxColuna++).NullIfEmpty();
                    despesaTemp.Lote = csv.GetField<int?>(idxColuna++);
                    despesaTemp.Ressarcimento = csv.GetField<int?>(idxColuna++);
                    despesaTemp.DataPagamentoRestituicao = csv.GetField<DateOnly?>(idxColuna++);
                    despesaTemp.Restituicao = csv.GetField<decimal?>(idxColuna++);
                    despesaTemp.NumeroDeputadoID = csv.GetField<int?>(idxColuna++);
                    despesaTemp.IdDocumento = csv.GetField(idxColuna++);
                    despesaTemp.UrlDocumento = csv.GetField(idxColuna++);

                    if (despesaTemp.ValorDocumento == null)
                        despesaTemp.ValorDocumento = despesaTemp.ValorLiquido + despesaTemp.ValorGlosa;

                    if (despesaTemp.UrlDocumento.Contains("/documentos/publ"))
                        despesaTemp.UrlDocumento = "1"; // Ex: https://www.camara.leg.br/cota-parlamentar/documentos/publ/3453/2022/7342370.pdf
                    else if (despesaTemp.UrlDocumento.Contains("/nota-fiscal-eletronica"))
                        despesaTemp.UrlDocumento = "2"; // Ex: https://www.camara.leg.br/cota-parlamentar/nota-fiscal-eletronica?ideDocumentoFiscal=7321395
                    else
                    {
                        if (!string.IsNullOrEmpty(despesaTemp.UrlDocumento))
                            logger.LogError("Documento '{Valor}' não reconhecido!", despesaTemp.UrlDocumento);

                        despesaTemp.UrlDocumento = "0";
                    }

                    if (!string.IsNullOrEmpty(despesaTemp.Passageiro))
                    {
                        despesaTemp.Passageiro = despesaTemp.Passageiro.ToString().Split(";")[0];
                        string[] partes = despesaTemp.Passageiro.ToString().Split(new[] { '/', ';' });
                        if (partes.Length > 1)
                        {
                            var antes = despesaTemp.Passageiro;
                            despesaTemp.Passageiro = "";
                            for (int y = partes.Length - 1; y >= 0; y--)
                            {
                                despesaTemp.Passageiro += " " + partes[y];
                            }
                        }
                    }

                    if (despesaTemp.DataEmissao == null)
                        despesaTemp.DataEmissao = new DateOnly(despesaTemp.Ano, despesaTemp.Mes.Value, 1);
                    else
                    {
                        // Copy from ImportadorDespesasBase.InserirDespesaTemp
                        var endMonthDate = new DateOnly(despesaTemp.Ano, despesaTemp.Mes ?? despesaTemp.DataEmissao.Value.Month, 1).AddMonths(1).AddDays(-1);

                        if (despesaTemp.DataEmissao?.Year != despesaTemp.Ano && despesaTemp.DataEmissao?.AddMonths(3).Year != despesaTemp.Ano)
                        {
                            // Validar ano com 3 meses de tolerancia.
                            //logger.LogWarning("Despesa com ano incorreto: {@Despesa}", despesaTemp);

                            var dt = despesaTemp.DataEmissao!.Value;
                            var monthLastDay = DateTime.DaysInMonth(despesaTemp.Ano, despesaTemp.Mes.Value);
                            despesaTemp.DataEmissao = new DateOnly(despesaTemp.Ano, dt.Month, Math.Min(dt.Day, monthLastDay));
                        }
                        else if (despesaTemp.DataEmissao > endMonthDate)
                        {
                            // Validar despesa com data futura.
                            //logger.LogWarning("Despesa com data incorreta/futura: {@Despesa}", despesaTemp);

                            var dt = despesaTemp.DataEmissao!.Value;
                            // Tentamos trocar apenas o ano.
                            despesaTemp.DataEmissao = new DateOnly(despesaTemp.Ano, dt.Month, dt.Day);

                            // Caso a data permaneça invalida, alteramos também o mês, se possivel.
                            if (despesaTemp.DataEmissao > endMonthDate && despesaTemp.Mes.HasValue)
                            {
                                var monthLastDay = DateTime.DaysInMonth(despesaTemp.Ano, despesaTemp.Mes.Value);
                                despesaTemp.DataEmissao = new DateOnly(despesaTemp.Ano, despesaTemp.Mes.Value, Math.Min(dt.Day, monthLastDay));
                            }
                        }
                    }


                    if (despesaTemp.CnpjCpf.StartsWith("000000000000"))
                    {
                        if (despesaTemp.CnpjCpf != "00000000000001" // CELULAR FUNCIONAL
                            && despesaTemp.CnpjCpf != "00000000000002" // LINHA DIRETA
                            && despesaTemp.CnpjCpf != "00000000000003" // IMÓVEL FUNCIONAL
                            && despesaTemp.CnpjCpf != "00000000000006" // RAMAL
                            && despesaTemp.CnpjCpf != "00000000000007" // CORREIOS - SEDEX CONVENCIONAL
                            && despesaTemp.CnpjCpf != "00000000000008" // PEDÁGIO
                            && despesaTemp.CnpjCpf != "00000000000009" // TAXI
                            )
                        {
                            if (despesaTemp.CnpjCpf != "00000000000010") // Empresa internacional genérica
                                logger.LogWarning("Validar CPNJ '{CnpjCpf} - {NomeEmpresa}'.", despesaTemp.CnpjCpf, despesaTemp.Fornecedor);

                            despesaTemp.CnpjCpf = null;
                        }

                    }
                    else if (despesaTemp.CnpjCpf.Length == 14 && !ImportacaoFornecedor.validarCNPJ(despesaTemp.CnpjCpf))
                    {
                        logger.LogWarning("CPNJ '{CnpjCpf} - {NomeEmpresa}' Invalido.", despesaTemp.CnpjCpf, despesaTemp.Fornecedor);
                    }

                    // Zerar o valor para ignora-lo (somente aqui) para agrupar os itens iguals e com valores diferentes.
                    // Para armazenamento na base de dados a hash é gerado com o valor, para que mudanças no total provoquem uma atualização.
                    var options = new JsonSerializerOptions
                    {
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                        TypeInfoResolver = new DefaultJsonTypeInfoResolver
                        {
                            Modifiers = { IgnoreNumericValues },
                        }
                    };

                    var str = JsonSerializer.Serialize(despesaTemp, options);
                    despesaTemp.Hash = Utils.SHA1Hash(str);

                    despesasTemp.Add(despesaTemp);
                    itensProcessadosAno++;
                    valorTotalProcessadoAno += despesaTemp.ValorLiquido ?? 0;
                }
            }

            if (itensProcessadosAno > 0)
                InsereDespesasTemp(despesasTemp, lstHash);

            if (lstHash.Any())
            {
                var deletar = lstHash.Values.Select(x => (long)x).ToList();
                dbContext.DeputadoFederalDespesaTemps
                    .AsNoTracking()
                    .Where(u => deletar.Contains(u.Id))
                    .ExecuteDelete();
            }

            if (itensProcessadosAno > 0)
            {
                ProcessarDespesasTemp(ano);
            }

            ValidaImportacao(ano);

            //dbContext.SaveChanges();
            //dbContext.Database.CommitTransaction();

            dbContext.ChangeTracker.Clear();
        }


        logger.LogWarning("Importados {Items} registros com valor de {ValorTotal}", itensProcessadosAno, valorTotalProcessadoAno);


        if (ano == DateTime.Now.Year && (itensProcessadosAno > 0 || lstHash.Count > 0))
        {
            InsereDespesaLegislatura();

            AtualizaParlamentarValoresCEAP();
            AtualizaCampeoesGastos();
            AtualizaResumoMensal();

            connection.Execute(@"UPDATE parametros SET cf_deputado_ultima_atualizacao=NOW();");
        }
    }

    private void InsereDespesasTemp(List<DeputadoFederalDespesaTemp> despesasTemp, Dictionary<string, int> lstHash)
    {
        JsonSerializerOptions options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };

        List<IGrouping<string, DeputadoFederalDespesaTemp>> results = despesasTemp.GroupBy(x => Convert.ToHexString(x.Hash)).ToList();
        itensProcessadosAno = results.Count();
        valorTotalProcessadoAno = results.Sum(x => x.Sum(x => x.ValorLiquido ?? 0));

        logger.LogInformation("Processando {Itens} despesas agrupadas em {Unicos} unicas com valor total de {ValorTotal}.", despesasTemp.Count(), itensProcessadosAno, valorTotalProcessadoAno);

        var despesaInserida = 0;
        var despesasParaInserir = new List<DeputadoFederalDespesaTemp>();
        foreach (var despesaGroup in results)
        {
            DeputadoFederalDespesaTemp despesa = despesaGroup.FirstOrDefault();
            despesa.ValorDocumento = despesaGroup.Sum(x => x.ValorDocumento);
            despesa.ValorGlosa = despesaGroup.Sum(x => x.ValorGlosa);
            despesa.ValorLiquido = despesaGroup.Sum(x => x.ValorLiquido);
            despesa.Restituicao = despesaGroup.Sum(x => x.Restituicao);

            var str = JsonSerializer.Serialize(despesa, options);
            var hash = Utils.SHA1Hash(str);
            var key = Convert.ToHexString(hash);

            if (lstHash.Remove(key)) continue;

            despesa.Hash = hash;

            despesasParaInserir.Add(despesa);
            despesaInserida++;
        }

        var bulkService = new BulkInsertService<DeputadoFederalDespesaTemp>();
        bulkService.BulkInsertNoTracking(dbContext, despesasParaInserir);

        //var uploader = new NpgsqlBulkUploader(dbContext);
        //uploader.Insert(despesasParaInserir);

        //dbContext.BulkInsert(despesasParaInserir);

        if (despesaInserida > 0)
            logger.LogInformation("{Itens} despesas inseridas na tabela temporaria.", despesaInserida);
    }

    static void IgnoreNumericValues(JsonTypeInfo typeInfo)
    {
        foreach (JsonPropertyInfo propertyInfo in typeInfo.Properties)
        {
            if (propertyInfo.PropertyType == typeof(decimal))
            {
                propertyInfo.ShouldSerialize = static (obj, value) => false;
            }
        }
    }

    private void ProcessarDespesasTemp(int ano)
    {
        CorrigeDespesas();
        InsereDeputadoFaltante();
        InserePassageiroFaltante();
        InsereTrechoViagemFaltante();
        //InsereTipoDespesaFaltante();
        //InsereTipoEspecificacaoFaltante();
        InsereMandatoFaltante();
        InsereLegislaturaFaltante();
        InsereFornecedorFaltante();
        InsereDespesaFinal(ano);
        LimpaDespesaTemporaria();
    }

    public virtual void ValidaImportacao(int ano)
    {
        logger.LogDebug("Validar importação");

        var sql = @$"
    select 
        count(1) as itens, 
        coalesce(sum(valor_liquido), 0) as valor_total 
    FROM camara.cf_despesa d 
    where d.ano = {ano}";

        int itensTotalFinal = default;
        decimal somaTotalFinal = default;
        using (IDataReader reader = connection.ExecuteReader(sql))
        {
            if (reader.Read())
            {
                itensTotalFinal = Convert.ToInt32(reader["itens"].ToString());
                somaTotalFinal = Convert.ToDecimal(reader["valor_total"].ToString());
            }
        }

        if (itensProcessadosAno != itensTotalFinal)
            logger.LogError("Totais divergentes! Arquivo: [Itens: {LinhasArquivo}; Valor: {ValorTotalArquivo}] DB: [Itens: {LinhasDB}; Valor: {ValorTotalFinal}]",
                itensProcessadosAno, valorTotalProcessadoAno, itensTotalFinal, somaTotalFinal);

        var despesasSemParlamentar = connection.ExecuteScalar<int>(@$"
    select count(1) from temp.cf_despesa_temp where id_deputado not in (select id FROM camara.cf_deputado);");

        if (despesasSemParlamentar > 0)
            logger.LogError("Há deputados não identificados!");
    }

    private void CorrigeDespesas()
    {
        connection.Execute(@"
UPDATE temp.cf_despesa_temp SET numero = NULL WHERE numero = 'S/N' OR numero = '';
UPDATE temp.cf_despesa_temp SET numero_deputado_id = NULL WHERE numero_deputado_id = 0;
UPDATE temp.cf_despesa_temp SET cnpj_cpf = NULL WHERE cnpj_cpf = '';

UPDATE temp.cf_despesa_temp SET sigla_uf = NULL WHERE sigla_uf = 'NA';
UPDATE temp.cf_despesa_temp SET numero_especificacao_sub_cota = NULL WHERE numero_especificacao_sub_cota = 0;
UPDATE temp.cf_despesa_temp SET lote = NULL WHERE lote = 0;
UPDATE temp.cf_despesa_temp SET ressarcimento = NULL WHERE ressarcimento = 0;
UPDATE temp.cf_despesa_temp SET id_documento = NULL WHERE id_documento = '0';
UPDATE temp.cf_despesa_temp SET parcela = NULL WHERE parcela = 0;
UPDATE temp.cf_despesa_temp SET sigla_partido = 'PP' WHERE sigla_partido = 'PP**';
UPDATE temp.cf_despesa_temp SET sigla_partido = null WHERE sigla_partido = 'NA';
    	");
    }

    public void InsereDeputadoFaltante()
    {
        // Atualiza os deputados já existentes quando efetuarem os primeiros gastos com a cota
        var affected = connection.Execute(@"
UPDATE camara.cf_deputado d
SET id_deputado = dt.numero_deputado_id
FROM temp.cf_despesa_temp dt
WHERE d.id = dt.id_deputado
AND d.id_deputado IS NULL;
        ");

        if (affected > 0)
        {
            //AtualizaInfoDeputados();
            //AtualizaInfoDeputadosCompleto();

            logger.LogInformation("{Itens} parlamentares atualizados!", affected);
        }

        // Insere um novo deputado ou liderança
        affected = connection.Execute(@"
INSERT INTO camara.cf_deputado (id, id_deputado, nome_parlamentar, cpf)
SELECT id_deputado, numero_deputado_id, nome_parlamentar, max(cpf)
FROM temp.cf_despesa_temp
WHERE numero_deputado_id  not in (
   	SELECT id_deputado 
 			FROM camara.cf_deputado
   	WHERE id_deputado IS NOT null
)
AND numero_deputado_id IS NOT null
GROUP BY id_deputado, numero_deputado_id, nome_parlamentar;
        ");

        //DownloadFotosDeputados(@"C:\GitHub\operacao-politica-supervisionada\OPS\Content\images\Parlamentares\DEPFEDERAL\");

        if (affected > 0)
        {
            //AtualizaInfoDeputados();
            //AtualizaInfoDeputadosCompleto();

            logger.LogInformation("{Itens} parlamentares incluidos!", affected);
        }
    }

    private string InserePassageiroFaltante()
    {
        var affected = connection.Execute(@"
            INSERT INTO pessoa (nome)
            SELECT DISTINCT passageiro
            FROM temp.cf_despesa_temp
            WHERE coalesce(passageiro, '') <> ''
            AND passageiro not in (
                SELECT nome FROM pessoa
            );
        ");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} passageiros incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereTrechoViagemFaltante()
    {
        var affected = connection.Execute(@"
            INSERT INTO trecho_viagem (descricao)
            SELECT DISTINCT trecho
            FROM temp.cf_despesa_temp
            WHERE coalesce(trecho, '') <> ''
            AND trecho not in (
            	SELECT descricao FROM trecho_viagem
            );
        ");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} trechos viagem incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereTipoDespesaFaltante()
    {
        var affected = connection.Execute(@"
            INSERT INTO cf_despesa_tipo (id, descricao)
            select distinct numero_sub_cota, descricao
            from temp.cf_despesa_temp
            where numero_sub_cota  not in (
            	select id FROM camara.cf_despesa_tipo
            );
        ");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} tipos de despesa incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereTipoEspecificacaoFaltante()
    {
        var affected = connection.Execute(@"
            INSERT INTO cf_especificacao_tipo (id_cf_despesa_tipo, id_cf_especificacao, descricao)
            select distinct numero_sub_cota, numero_especificacao_sub_cota, ""descricaoEspecificacao""
            from temp.cf_despesa_temp dt
            left join cf_especificacao_tipo tp on tp.id_cf_despesa_tipo = dt.numero_sub_cota
                and tp.id_cf_especificacao = dt.numero_especificacao_sub_cota
            where numero_especificacao_sub_cota <> 0
            AND tp.descricao = null;
        ");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} tipo especificação incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereMandatoFaltante()
    {
        var affected = connection.Execute(@"
UPDATE camara.cf_mandato m
SET id_carteira_parlamantar = dt.numero_carteira_parlamentar
FROM (
    SELECT DISTINCT 
        numero_deputado_id, legislatura, numero_carteira_parlamentar, codigo_legislatura, sigla_uf, sigla_partido
    FROM temp.cf_despesa_temp
) dt
INNER JOIN camara.cf_deputado d ON d.id_deputado = dt.numero_deputado_id
LEFT JOIN estado e ON e.sigla = dt.sigla_uf
LEFT JOIN partido p ON p.sigla = dt.sigla_partido
WHERE m.id_cf_deputado = d.id
AND m.id_legislatura = dt.codigo_legislatura
AND dt.codigo_legislatura <> 0
AND m.id_carteira_parlamantar IS NULL;
    	");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} mandatos atualizados!", affected);
        }

        // TODO: Parlamentar pode ter mais de um partido por mandato, isso se reflete na CEAP
        affected = connection.Execute(@"
INSERT INTO camara.cf_mandato (id_cf_deputado, id_legislatura, id_carteira_parlamantar, id_estado, id_partido)
SELECT DISTINCT d.id, dt.codigo_legislatura, dt.numero_carteira_parlamentar, e.id, p.id 
FROM ( 
    SELECT DISTINCT 
        numero_deputado_id, legislatura, numero_carteira_parlamentar, codigo_legislatura, sigla_uf, sigla_partido
    FROM temp.cf_despesa_temp
) dt
join camara.cf_deputado d on d.id_deputado = dt.numero_deputado_id
LEFT JOIN estado e ON e.sigla = dt.sigla_uf
LEFT JOIN partido p ON p.sigla = dt.sigla_partido
LEFT JOIN camara.cf_mandato m ON m.id_cf_deputado = d.id
    AND m.id_legislatura = dt.codigo_legislatura 
WHERE dt.codigo_legislatura <> 0
AND m.id IS NULL
ON CONFLICT DO NOTHING
    	");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} mandatos incluidos!", affected);
        }

        return string.Empty;
    }

    private string InsereLegislaturaFaltante()
    {
        var affected = connection.Execute(@"
    		INSERT INTO camara.cf_legislatura (id, ano)
    		select distinct codigo_legislatura, legislatura
    		from temp.cf_despesa_temp dt
    		where codigo_legislatura <> 0
    		AND codigo_legislatura  not in (
    			select id FROM camara.cf_legislatura
    		);
    	");

        if (affected > 0)
        {
            logger.LogInformation("{Itens} legislatura incluidos!", affected);
        }

        return string.Empty;
    }

    public void InsereFornecedorFaltante()
    {
        ResolverFornecedor();

        var affected = connection.Execute(@"
    		INSERT INTO fornecedor.fornecedor (nome, cnpj_cpf)
    		select MAX(dt.fornecedor), dt.cnpj_cpf
    		from temp.cf_despesa_temp dt
    		where dt.id_fornecedor is null
            and dt.cnpj_cpf is not null
    		GROUP BY dt.cnpj_cpf;
    	");

        if (affected > 0)
            ResolverFornecedorPreExistente();


        List<string> despesasSemFornecedor = dbContext.DeputadoFederalDespesaTemps
            .Where(x => x.IdFornecedor == null)
            .Select(x => x.Fornecedor)
            .Distinct()
            .ToList();

        if (despesasSemFornecedor.Any())
        {
            dbContext.ChangeTracker.Clear();

            foreach (var nomeFornecedor in despesasSemFornecedor)
            {
                var fornecedor = new Infraestrutura.Entities.Fornecedores.Fornecedor()
                {
                    Nome = nomeFornecedor
                };
                dbContext.Fornecedores.Add(fornecedor);
                dbContext.SaveChanges();


                dbContext.FornecedorDeParas.Add(new FornecedorDePara()
                {
                    NomeFornecedor = nomeFornecedor,
                    IdFornecedorIncorreto = fornecedor.Id
                });

                dbContext.DeputadoFederalDespesaTemps
                    .Where(x => x.IdFornecedor == null && x.Fornecedor == nomeFornecedor)
                    .ExecuteUpdate(setters => setters.SetProperty(x => x.IdFornecedor, fornecedor.Id));
            }

            dbContext.SaveChanges();
            dbContext.ChangeTracker.Clear();
            affected += despesasSemFornecedor.Count();

            ResolverFornecedorPreExistente();
        }
        if (affected > 0)
        {
            ResolverFornecedor();
            logger.LogInformation("{Itens} fornecedores incluidos!", affected);
        }
    }

    private void ResolverFornecedor()
    {
        ResolverFornecedorPreExistente();
        ResolverFornecedorDepara();
    }

    private void ResolverFornecedorPreExistente()
    {
        connection.Execute(@$"
-- Por CNPJ para fornecedor pré-existente
UPDATE temp.cf_despesa_temp dt
set id_fornecedor = f.id
FROM fornecedor.fornecedor f 
WHERE dt.id_fornecedor IS NULL
AND f.cnpj_cpf = dt.cnpj_cpf;
");
    }

    private void ResolverFornecedorDepara()
    {
        connection.Execute(@$"
-- Por Nome exato e CPNJ
UPDATE temp.cf_despesa_temp dt
set id_fornecedor = f.id_fornecedor_correto
FROM temp.fornecedor_de_para f 
WHERE dt.id_fornecedor IS NULL
AND f.cnpj_incorreto = dt.cnpj_cpf
AND f.nome ILIKE dt.fornecedor;

-- Por CNPJ (Pode ser parcial ou invalido)
UPDATE temp.cf_despesa_temp dt
set id_fornecedor = f.id_fornecedor_correto
FROM temp.fornecedor_de_para f 
WHERE dt.id_fornecedor IS NULL
AND f.cnpj_incorreto = dt.cnpj_cpf;

-- Por Nome exato (accent-insensitive matching)
UPDATE temp.cf_despesa_temp dt
set id_fornecedor = f.id_fornecedor_correto, cnpj_cpf = COALESCE(dt.cnpj_cpf, f.cnpj_correto)
FROM temp.fornecedor_de_para f 
WHERE dt.id_fornecedor IS NULL
AND (lower(f.nome)) = (lower(dt.fornecedor));
");
    }

    public void InsereDespesaFinal(int ano)
    {
        var affected = connection.Execute(@$"
INSERT INTO camara.cf_despesa (
    ano,
    mes,
    id_documento,
    id_cf_deputado,
    id_cf_legislatura,
    id_cf_mandato,
    id_cf_despesa_tipo,
    id_cf_especificacao,
    id_fornecedor,
    id_passageiro,
    numero_documento,
    tipo_documento,
    data_emissao,
    valor_documento,
    valor_glosa,
    valor_liquido,
    valor_restituicao,
    id_trecho_viagem,
    tipo_link,
    ano_mes,
    hash
)
SELECT 
    dt.ano,
    dt.mes,
    CAST( dt.id_documento as INT4),
    d.id,
    dt.codigo_legislatura,
    m.id,
    numero_sub_cota,
    numero_especificacao_sub_cota,
    dt.id_fornecedor,
    p.id,
    numero,
    CAST(tipo_documento as SMALLINT),
    data_emissao,
    valor_documento,
    valor_glosa,
    valor_liquido,
    restituicao,
    tv.id,
    CAST(coalesce(url_documento, '0') as SMALLINT),
    concat(ano::text, mes::text)::INT8,
    dt.hash
from temp.cf_despesa_temp dt
LEFT JOIN camara.cf_deputado d on d.id_deputado = dt.numero_deputado_id
LEFT JOIN pessoa p on p.nome = dt.passageiro
LEFT JOIN trecho_viagem tv on tv.descricao = dt.trecho
left join camara.cf_mandato m on m.id_cf_deputado = d.id
    and m.id_legislatura = dt.codigo_legislatura
    and m.id_carteira_parlamantar = numero_carteira_parlamentar;
    	", 3600);

        var totalTemp = connection.ExecuteScalar<int>("select count(1) from temp.cf_despesa_temp");
        if (affected != totalTemp)
            logger.LogWarning("{Itens} despesas incluidas. Há {Qtd} despesas que foram ignoradas!", affected, totalTemp - affected);
        else if (affected > 0)
            logger.LogInformation("{Itens} despesas incluidas!", affected);
    }

    private void InsereDespesaLegislatura()
    {
        //  var Legislaturas = connection.Query<int>(@"SELECT DISTINCT codigo_legislatura FROM temp.cf_despesa_temp");
        //  if (!Legislaturas.Any())
        //  {
        //      Legislaturas = new List<int>() { legislaturaAtual - 2, legislaturaAtual - 1, legislaturaAtual };
        //  }
        //  ;

        //  foreach (var legislatura in Legislaturas)
        //  {
        //      connection.Execute(@$"
        //          TRUNCATE TABLE cf_despesa_{legislatura};

        //          INSERT INTO cf_despesa_{legislatura} 
        //              (id, id_cf_deputado, id_cf_despesa_tipo, id_fornecedor, ano_mes, data_emissao, valor_liquido)
        //          SELECT 
        //               id, id_cf_deputado, id_cf_despesa_tipo, id_fornecedor, concat(ano, LPAD(mes, 2, '0')), data_emissao, valor_liquido 
        //          FROM camara.cf_despesa
        //          WHERE id_cf_legislatura = {legislatura};
        //", 3600);
        //  }
    }

    public void LimpaDespesaTemporaria()
    {
        connection.Execute(@"truncate table temp.cf_despesa_temp;");
    }

    #endregion Importaçao Dados CEAP CSV


    #region Processar Resumo CEAP

    public void AtualizaParlamentarValoresCEAP()
    {
        var dt = connection.Query("select id FROM camara.cf_deputado where processado != 2");
        object valor_total_ceap;

        foreach (var dr in dt)
        {
            try
            {
                var obj = new { id_cf_deputado = Convert.ToInt32(dr.id) };
                valor_total_ceap = connection.ExecuteScalar("select sum(valor_liquido) FROM camara.cf_despesa where id_cf_deputado=@id_cf_deputado;", obj);
                if (Convert.IsDBNull(valor_total_ceap)) valor_total_ceap = 0;
                else if (valor_total_ceap == null || (decimal)valor_total_ceap < 0) valor_total_ceap = 0;

                connection.Execute(@"update cf_deputado set valor_total_ceap = @valor_total_ceap where id=@id_cf_deputado", new
                {
                    valor_total_ceap = valor_total_ceap,
                    id_cf_deputado = dr.id
                }
                );
            }
            catch (Exception ex)
            {
                if (!ex.GetBaseException().Message.Contains("Out of range"))
                    throw;
            }
        }
    }

    /// <summary>
    /// Atualiza indicador 'Campeões de gastos',
    /// Os 4 deputados que mais gastaram com a CEAP desde o ínicio do mandato 55 (02/2015)
    /// </summary>
    public void AtualizaCampeoesGastos()
    {
        var strSql =
            @"truncate table cf_deputado_campeao_gasto;
    				insert into cf_deputado_campeao_gasto
    				SELECT l1.id_cf_deputado, d.nome_parlamentar, l1.valor_total, p.sigla, e.sigla
    				FROM (
    					SELECT 
    						l.id_cf_deputado,
    						sum(l.valor_liquido) as valor_total
    					FROM  cf_despesa_57 l
    					GROUP BY l.id_cf_deputado
    					order by valor_total desc 
    					limit 4
    				) l1 
    				INNER JOIN cf_deputado d on d.id = l1.id_cf_deputado 
    				LEFT JOIN partido p on p.id = d.id_partido
    				LEFT JOIN estado e on e.id = d.id_estado;";

        connection.Execute(strSql);
    }

    public void AtualizaResumoMensal()
    {
        var strSql =
            @"truncate table cf_despesa_resumo_mensal;
    				insert into cf_despesa_resumo_mensal
    				(ano, mes, valor) (
    					select ano, mes, sum(valor_liquido)
    					FROM camara.cf_despesa
    					group by ano, mes
    				);";

        connection.Execute(strSql);
    }

    #endregion Processar Resumo CEAP


    //    #region Importação Remuneração
    //    // Não atualizado
    //    public void ConsultaRemuneracao(int ano, int mes)
    //    {
    //        var meses = new string[] { "janeiro", "fevereiro", "marco", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro" };

    //        string urlOrigem;
    //        if (ano == 2012)
    //            urlOrigem = string.Format("https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/{0}/{1}-{0}-csv.csv", ano, meses[mes - 1]);
    //        else if (ano == 2013 && mes == 2)
    //            urlOrigem = "https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/2013/fevereiro-de-2013-1";
    //        else if (ano == 2013 && mes == 3)
    //            urlOrigem = "https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/2013/Marco-2013-csv";
    //        else if (ano >= 2013 && mes == 5)
    //            urlOrigem = "https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/2013/RemuneracaoMensalServidores052013.csv";
    //        else if (ano == 2016 && mes == 7)
    //            urlOrigem = "https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/2016/copy_of_RemuneracaoMensalServidores072016.csv";
    //        else if (ano == 2015)
    //            urlOrigem = string.Format("https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/copy_of_2014/{1}-de-{0}-csv", ano, meses[mes - 1]);
    //        else if (ano > 2013 || (ano == 2013 && (mes >= 7 || mes == 4)))
    //            urlOrigem = string.Format("https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/{0}/{1}-de-{0}-csv", ano, meses[mes - 1]);
    //        else
    //            urlOrigem = string.Format("https://www2.camara.leg.br/transparencia/recursos-humanos/remuneracao/relatorios-consolidados-por-ano-e-mes/{0}/{1}-de-{0}-csv", ano, meses[mes - 1]);

    //        var caminhoArquivo = System.IO.Path.Combine(appSettings.TempFolder, $"CF/RM{ano}{mes:00}.csv");

    //        try
    //        {
    //            bool novoArquivoBaixado = BaixarArquivo(urlOrigem, caminhoArquivo);
    //            if (!novoArquivoBaixado) return;

    //            CarregaRemuneracaoCsv(caminhoArquivo, Convert.ToInt32(ano.ToString() + mes.ToString("00")));
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.LogError(ex, ex.Message);

    //            if (File.Exists(caminhoArquivo))
    //                File.Delete(caminhoArquivo);
    //        }
    //    }

    //    private string CarregaRemuneracaoCsv(string file, int anomes)
    //    {
    //        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
    //        var sb = new StringBuilder();
    //        string sResumoValores = string.Empty;

    //        int indice = 0;
    //        int CargoIndividualizadodoServidor = indice++;
    //        int GrupoFuncional = indice++;
    //        int FolhadePagamento = indice++;
    //        int AnoIngresso = indice++;
    //        int RemuneracaoFixa = indice++;
    //        int VantagensdeNaturezaPessoal = indice++;
    //        int FunçaoCargoComissao = indice++;
    //        int GratificacaoNatalina = indice++;
    //        int Ferias = indice++;
    //        int OutrasRemuneracoesEventuaisProvisorias = indice++;
    //        int AbonodePermanencia = indice++;
    //        int RedutorConstitucional = indice++;
    //        int ConstribuicaoPrevidenciária = indice++;
    //        int ImpostodeRenda = indice++;
    //        int RemuneracaoAposDescontosObrigatorios = indice++;
    //        int Diarias = indice++;
    //        int Auxilios = indice++;
    //        int VantagensIndenizatórias = indice++;

    //        //int linhaAtual = 0;

    //        using (var banco = new AppDb())
    //        {
    //            //var lstHash = new List<string>();
    //            //using (var dReader = banco.ExecuteReader("select hash FROM senado.sf_despesa where ano=" + ano))
    //            //{
    //            //    while (dReader.Read())
    //            //    {
    //            //        try
    //            //        {
    //            //            lstHash.Add(dReader["hash"].ToString());
    //            //        }
    //            //        catch (Exception)
    //            //        {
    //            //            // Vai ter duplicado mesmo
    //            //        }
    //            //    }
    //            //}

    //            //using (var dReader = banco.ExecuteReader("select sum(valor) as valor, count(1) as itens FROM senado.sf_despesa where ano=" + ano))
    //            //{
    //            //    if (dReader.Read())
    //            //    {
    //            //        sResumoValores = string.Format("[{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
    //            //    }
    //            //}

    //            LimpaRemuneracaoTemporaria(banco);

    //            using (var reader = new StreamReader(file, Encoding.GetEncoding("ISO-8859-1")))
    //            {
    //                short count = 0;

    //                using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")))
    //                {
    //                    while (csv.Read())
    //                    {
    //                        count++;
    //                        if (count == 1)
    //                        {
    //                            if (
    //                                    (csv[CargoIndividualizadodoServidor] != "Cargo Individualizado do Servidor") ||
    //                                    (csv[VantagensIndenizatórias].Trim() != "Vantagens Indenizatórias")
    //                                )
    //                            {
    //                                throw new Exception("Mudança de integração detectada para o CamaraFederal Federal");
    //                            }

    //                            // Pular linha de titulo
    //                            continue;
    //                        }

    //                        var cargo = csv[CargoIndividualizadodoServidor].Split(" ");
    //                        var id = cargo[cargo.Length - 1];
    //                        if (cargo[0].Trim() == "(*)")
    //                        {
    //                            break;
    //                        }
























    //                        //string hash = banco.ParametersHash();
    //                        //if (lstHash.Remove(hash))
    //                        //{
    //                        //    banco.ClearParameters();
    //                        //    continue;
    //                        //}



    //                        banco.ExecuteNonQuery(
    //                            @"INSERT INTO temp.cf_remuneracao_temp (
    //								id, ano_mes, cargo,grupo_funcional,tipo_folha,admissao,
    //                                remun_basica ,vant_pessoais ,func_comissionada ,grat_natalina ,ferias ,outras_eventuais ,abono_permanencia ,
    //                                reversao_teto_const ,imposto_renda ,previdencia ,rem_liquida ,diarias ,auxilios ,vant_indenizatorias
    //							) VALUES (
    //								@id, @ano_mes,@cargo,@grupo_funcional,@tipo_folha,@admissao,
    //                                @remun_basica ,@vant_pessoais ,@func_comissionada ,@grat_natalina ,@ferias ,@outras_eventuais ,@abono_permanencia ,
    //                                @reversao_teto_const ,@imposto_renda ,@previdencia ,@rem_liquida ,@diarias ,@auxilios ,@vant_indenizatorias
    //							)");
    //                    }
    //                }

    //                //if (++linhaAtual % 100 == 0)
    //                //{
    //                //    Console.WriteLine(linhaAtual);
    //                //}
    //            }

    //            //if (lstHash.Count == 0 && linhaAtual == 0)
    //            //{
    //            //    sb.AppendFormat("<p>Não há novos itens para importar! #2</p>");
    //            //    return sb.ToString();
    //            //}

    //            //if (lstHash.Count > 0)
    //            //{
    //            //    foreach (var hash in lstHash)
    //            //    {
    //            //        banco.ExecuteNonQuery(string.Format("delete FROM senado.sf_despesa where hash = '{0}'", hash));
    //            //    }


    //            //    Console.WriteLine("Registros para exluir: " + lstHash.Count);
    //            //    sb.AppendFormat("<p>{0} registros excluidos</p>", lstHash.Count);
    //            //}

    //            //sb.Append(ProcessarDespesasTemp(banco, completo));
    //        }

    //        //if (ano == DateTime.Now.Year)
    //        //{
    //        //    AtualizaCamaraFederalrValores();
    //        //    AtualizaCampeoesGastos();
    //        //    AtualizaResumoMensal();
    //        //}

    //        //using (var banco = new AppDb())
    //        //{
    //        //    using (var dReader = banco.ExecuteReader("select sum(valor) as valor, count(1) as itens FROM senado.sf_despesa where ano=" + ano))
    //        //    {
    //        //        if (dReader.Read())
    //        //        {
    //        //            sResumoValores += string.Format(" -> [{0}]={1}", dReader["itens"], Utils.FormataValor(dReader["valor"]));
    //        //        }
    //        //    }

    //        //    sb.AppendFormat("<p>Resumo atualização: {0}</p>", sResumoValores);
    //        //}

    //        return sb.ToString();
    //    }

    //private void LimpaRemuneracaoTemporaria(AppDb banco)
    //{
    //    var affected = banco.Execute(@"
    //				truncate table temp.cf_remuneracao_temp;
    //			");
    //}

    public void ColetaDadosDeputados()
    {
        // var sqlDeputados = @"
        // SELECT DISTINCT cd.id as id_cf_deputado
        // FROM camara.cf_deputado cd 
        // JOIN cf_funcionario_contratacao c ON c.id_cf_deputado = cd.id AND c.periodo_ate IS NULL
        // -- WHERE id_legislatura = 57
        // -- and cd.id >= 141428
        // and cd.processado = 0
        // order by cd.id
        // ";

        ConcurrentQueue<Dictionary<string, object>> queue;
        // Migrated to IDbConnection - removed AppDb usage
        {
            // Executar para colocar todas na fila de processamento
            // connection.Execute("update cf_deputado set processado=0, secretarios_ativos=0 where processado=1;");

            var sqlDeputados = @"
    SELECT cd.id as id_cf_deputado, nome_parlamentar, cd.situacao -- DISTINCT
    FROM camara.cf_deputado cd 
    -- JOIN cf_mandato m ON m.id_cf_deputado = cd.id
    -- WHERE id_legislatura = 57                                
    WHERE cd.processado = 0
    -- where cd.situacao = 'Exercício'
    -- order by cd.id desc
    ";
            var dcDeputado = connection.Query<Dictionary<string, object>>(sqlDeputados).ToList();
            queue = new ConcurrentQueue<Dictionary<string, object>>(dcDeputado);
        }

        int totalRecords = 40;
        Task[] tasks = new Task[totalRecords];
        for (int i = 0; i < totalRecords; ++i)
        {
            tasks[i] = ProcessaFilaColetaDadosDeputados(queue);
        }

        Task.WaitAll(tasks);

        AtualizaParlamentarValores();
    }


    private readonly string[] meses = new string[] { "JAN", "FEV", "MAR", "ABR", "MAI", "JUN", "JUL", "AGO", "SET", "OUT", "NOV", "DEZ" };

    private async Task ProcessaFilaColetaDadosDeputados(ConcurrentQueue<Dictionary<string, object>> queue)
    {
        var anoCorte = 2008;
        var cultureInfoBR = CultureInfo.CreateSpecificCulture("pt-BR");
        var cultureInfoUS = CultureInfo.CreateSpecificCulture("en-US");

        Dictionary<string, object> deputado = null;
        var context = httpClient.CreateAngleSharpContext();

        var sqlInsertImovelFuncional = "insert ignore into cf_deputado_imovel_funcional (id_cf_deputado, uso_de, uso_ate, total_dias) values (@id_cf_deputado, @uso_de, @uso_ate, @total_dias)";

        // Migrated to IDbConnection - removed AppDb usage
        {
            while (queue.TryDequeue(out deputado))
            {
                using (logger.BeginScope(new Dictionary<string, object> { ["Parlamentar"] = deputado["nome_parlamentar"].ToString() }))
                {
                    var idDeputado = Convert.ToInt32(deputado["id_cf_deputado"]);
                    bool possuiPassaporteDiplimatico = false;
                    int qtdSecretarios = 0;
                    var processado = 1;

                    try
                    {
                        var address = $"https://www.camara.leg.br/deputados/{idDeputado}";
                        var document = await context.OpenAsyncAutoRetry(address);
                        if (document.StatusCode != HttpStatusCode.OK)
                        {
                            logger.LogError($"{address} {document.StatusCode}");
                        }
                        else
                        {
                            var anosmandatos = document
                                .QuerySelectorAll(".linha-tempo li")
                                .Select(x => Convert.ToInt32(x.TextContent.Trim()))
                                .Where(x => x >= anoCorte);
                            if (anosmandatos.Any())
                            {
                                foreach (var ano in anosmandatos)
                                {
                                    using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano }))
                                    {
                                        await ColetaSalarioDeputado(context, idDeputado, ano);

                                        address = $"https://www.camara.leg.br/deputados/{idDeputado}?ano={ano}";
                                        document = await context.OpenAsyncAutoRetry(address);
                                        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
                                        {
                                            logger.LogError("{StatusCode}: {Url}", document.StatusCode, document.Url);
                                            continue;
                                        }

                                        var lstInfo = document.QuerySelectorAll(".recursos-beneficios-deputado-container .beneficio");
                                        if (!lstInfo.Any())
                                        {
                                            logger.LogError("Nenhuma informação de beneficio: {address}", address);
                                            continue;
                                        }

                                        var verbaGabineteMensal = document.QuerySelectorAll("#gastomensalverbagabinete tbody tr");
                                        foreach (var item in verbaGabineteMensal)
                                        {
                                            var colunas = item.QuerySelectorAll("td");

                                            var mes = Array.IndexOf(meses, colunas[0].TextContent) + 1;
                                            var valor = Convert.ToDecimal(colunas[1].TextContent, cultureInfoBR);
                                            var percentual = Convert.ToDecimal(colunas[2].TextContent.Replace("%", ""), cultureInfoUS);

                                            connection.Execute("insert ignore into cf_deputado_verba_gabinete (id_cf_deputado, ano, mes, valor, percentual) values (@id_cf_deputado, @ano, @mes, @valor, @percentual);");
                                        }

                                        var cotaParlamentarMensal = document.QuerySelectorAll("#gastomensalcotaparlamentar tbody tr");
                                        foreach (var item in cotaParlamentarMensal)
                                        {
                                            var colunas = item.QuerySelectorAll("td");

                                            var mes = Array.IndexOf(meses, colunas[0].TextContent) + 1;
                                            var valor = Convert.ToDecimal(colunas[1].TextContent, cultureInfoBR);
                                            decimal? percentual = null;
                                            if (!string.IsNullOrEmpty(colunas[2].TextContent.Replace("%", "")))
                                                percentual = Convert.ToDecimal(colunas[2].TextContent.Replace("%", ""), cultureInfoUS);

                                            connection.Execute("insert ignore into cf_deputado_cota_parlamentar (id_cf_deputado, ano, mes, valor, percentual) values (@id_cf_deputado, @ano, @mes, @valor, @percentual);");
                                        }

                                        foreach (var info in lstInfo)
                                        {
                                            try
                                            {
                                                var titulo = info.QuerySelector(".beneficio__titulo")?.TextContent?.Replace(" ?", "")?.Trim();
                                                var valor = info.QuerySelector(".beneficio__info")?.TextContent?.Trim(); //Replace \n

                                                switch (titulo)
                                                {
                                                    case "Pessoal de gabinete":
                                                        if (valor != "0 pessoas")
                                                        {
                                                            if ((ano == DateTime.Today.Year))
                                                                qtdSecretarios = Convert.ToInt32(valor.Split(' ')[0]);

                                                            await ColetaPessoalGabinete(context, idDeputado, ano);
                                                        }
                                                        break;
                                                    case "Salário mensal bruto":
                                                        // Nada: Deve Consultar por ano
                                                        break;
                                                    case "Imóvel funcional":
                                                        if (!valor.StartsWith("Não fez") && !valor.StartsWith("Não faz") && !valor.StartsWith("Não há dado"))
                                                        {
                                                            var lstImovelFuncional = valor.Split("\n");
                                                            foreach (var item in lstImovelFuncional)
                                                            {
                                                                var periodos = item.Trim().Split(" ");

                                                                var dataInicial = DateTime.Parse(periodos[3]);
                                                                DateTime? dataFinal = null;
                                                                int? dias = null;

                                                                if (!item.Contains("desde"))
                                                                {
                                                                    dataFinal = DateTime.Parse(periodos[5]);
                                                                    dias = (int)dataFinal.Value.Subtract(dataInicial).TotalDays;
                                                                }





                                                                connection.Execute(sqlInsertImovelFuncional);
                                                            }
                                                        }
                                                        break;
                                                    case "Auxílio-moradia":
                                                        if (valor != "Não recebe o auxílio")
                                                        {
                                                            await ColetaAuxilioMoradia(context, idDeputado, ano);
                                                        }
                                                        break;
                                                    case "Viagens em missão oficial":
                                                        if (valor != "0")
                                                        {
                                                            await ColetaMissaoOficial(context, idDeputado, ano);
                                                        }
                                                        break;
                                                    case "Passaporte diplomático":
                                                        if (valor != "Não possui")
                                                        {
                                                            possuiPassaporteDiplimatico = true;
                                                        }
                                                        break;
                                                    default:
                                                        throw new NotImplementedException($"Vefificar beneficios do parlamentar {idDeputado} para o ano {ano}.");
                                                }

                                                // Console.WriteLine($"{titulo}: {valor}");
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.LogError(ex, ex.Message);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                processado = 2; // Não coletar novamente
                            }
                        }
                    }
                    catch (BusinessException)
                    {
                        //processado = 2; // Não coletar novamente
                        processado = 3; // Verificar
                    }
                    catch (Exception ex)
                    {
                        processado = 0;
                        logger.LogError(ex, ex.Message);
                    }



                    //connection.Execute("update cf_deputado set processado = @processado where id = @id_cf_deputado");





                    connection.Execute("update cf_deputado set passaporte_diplomatico = @passaporte, secretarios_ativos = @secretarios_ativos, processado = @processado where id = @id_cf_deputado");
                }
            }
        }
    }

    private async Task ColetaSalarioDeputado(IBrowsingContext context, int idDeputado, int ano)
    {
        var address = $"https://www.camara.leg.br/deputados/{idDeputado}/remuneracao?ano={ano}";
        var document = await context.OpenAsyncAutoRetry(address);
        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
        {
            logger.LogError("{StatusCode}: {Url}", document.StatusCode, document.Url);
            return;
        }

        var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
        if (tabelas.Length == 0)
        {
            logger.LogError("Nenhuma tabela encontrada! Url: {Url}", address);
            return;
        }

        foreach (var tabela in tabelas)
        {
            var linhas = tabela.QuerySelectorAll("tbody tr");
            if (linhas.Length == 0)
            {
                logger.LogError("Nenhuma linha encontrada! Url: {Url}", address);
                return;
            }

            foreach (var linha in linhas)
            {
                var colunas = linha.QuerySelectorAll("td");

                var mes = Convert.ToInt16(colunas[0].TextContent.Trim());
                var valor = Convert.ToDecimal(colunas[1].TextContent.Trim(), cultureInfo);
                var link = (colunas[1].FirstElementChild as IHtmlAnchorElement).Href; // todo?

                //connection.Execute("insert ignore into cf_deputado_salario (id_cf_deputado, ano, mes, valor) values (@id_cf_deputado, @ano, @mes, @valor);");

                // Não considera gratificação natalina e outros adicionais
                var salario = new DeputadoRemuneracao()
                {
                    IdDeputado = (int)idDeputado,
                    Ano = (short)ano,
                    Mes = mes,
                    Valor = valor
                };

                try
                {
                    dbContext.DeputadoRemuneracoes.Add(salario);
                    logger.LogDebug("Inserida remuneração de {Mes}/{Ano} do tipo {TipoRemuneracao}", mes, ano, idDeputado);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("Duplicate") && !ex.Message.Contains("Unable to cast object of type 'System.UInt32' to type 'System.Nullable`1[System.Int32]"))
                        throw;
                    else
                        logger.LogWarning("Registro duplicado ignorado: {@Salario}", salario);
                }

                await ColetaSalarioDeputadoDetalhado(context, idDeputado, ano, mes);
            }
        }

        dbContext.SaveChanges();
    }

    private async Task ColetaSalarioDeputadoDetalhado(IBrowsingContext context, int idDeputado, int ano, short mes)
    {
        var address = $"https://www.camara.leg.br/deputados/{idDeputado}/remuneracao-deputado-detalhado?mesAno={mes:00}{ano}";
        var document = await context.OpenAsyncAutoRetry(address);
        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
        {
            logger.LogError("{StatusCode}: {Url}", document.StatusCode, document.Url);
            return;
        }

        var folhasPagamento = document.QuerySelectorAll(".remuneracao-funcionario__info");
        if (!folhasPagamento.Any())
        {
            logger.LogWarning("Dados indisponiveis: " + address);
            return; // Erro no funcionario, abortar coleta dele
        }

        foreach (var folhaPagamento in folhasPagamento)
        {
            var titulo = folhaPagamento.QuerySelector(".remuneracao-funcionario__mes-ano").TextContent;

            using (logger.BeginScope(new Dictionary<string, object> { ["Folha"] = titulo }))
            {
                if (titulo.Split("�")[0].Trim() != $"{mes:00}{ano}") // todo:
                {
                    throw new NotImplementedException("Algo esta errado!");
                }

                var folha = new DeputadoFederalRemuneracaoDetalhes();
                IEnumerable<PropertyInfo> props = folha.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                var cabecalho = folhaPagamento.QuerySelectorAll(".remuneracao-funcionario__info-item");
                var categoriaFuncional = cabecalho.FirstOrDefault(x => x.TextContent.StartsWith("Categoria funcional"))?.TextContent.Split(":")[1].Trim();
                var dataExercicio = cabecalho.FirstOrDefault(x => x.TextContent.StartsWith("Data de exerc�cio"))?.TextContent.Split(":")[1].Trim();
                var cargo = cabecalho.FirstOrDefault(x => x.TextContent.StartsWith("Cargo"))?.TextContent.Split(":")[1].Trim();

                var linhas = folhaPagamento.QuerySelectorAll(".tabela-responsiva--remuneracao-funcionario tbody tr");
                foreach (var linha in linhas)
                {
                    var colunas = linha.QuerySelectorAll("td");
                    if (!colunas.Any()) continue;

                    var descricao = colunas[0].TextContent.Trim();
                    var valor = Convert.ToDecimal(colunas[1].TextContent.Trim());

                    var propInfo = folha.GetType().GetProperty(colunasBanco[descricao]);
                    propInfo.SetValue(folha, valor, null);
                }

                byte tipo = 0;
                switch (titulo.Split("�")[1].Trim()) // TODO
                {
                    case "FOLHA NORMAL":
                        tipo = 1;
                        break;
                    case "FOLHA COMPLEMENTAR":
                        tipo = 2;
                        break;
                    case "FOLHA COMPLEMENTAR ESPECIAL":
                        tipo = 3;
                        break;
                    case "FOLHA DE ADIANTAMENTO GRATIFICAÇÃO NATALINA":
                        tipo = 4;
                        break;
                    case "FOLHA DE GRATIFICAÇÃO NATALINA":
                        tipo = 5;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                folha.Tipo = tipo;
                folha.Referencia = new DateTime(ano, mes, 1);

                if (!string.IsNullOrEmpty(dataExercicio))
                    folha.Contratacao = DateOnly.Parse(dataExercicio, cultureInfo);

                folha.ValorBruto =
                    folha.RemuneracaoFixa +
                    folha.VantagensNaturezaPessoal +
                    folha.FuncaoOuCargoEmComissao +
                    folha.GratificacaoNatalina +
                    folha.Ferias +
                    folha.OutrasRemuneracoes +
                    folha.AbonoPermanencia;

                var dataContratacao = Convert.ToDateTime(dataExercicio, cultureInfo);
                folha.ValorOutros = folha.ValorDiarias + folha.ValorAuxilios + folha.ValorVantagens;
                folha.ValorTotal = folha.ValorBruto + folha.ValorOutros;

                try
                {
                    lock (padlock)
                        dbContext.DeputadoFederalRemuneracaoDetalhes.Add(folha);

                    logger.LogDebug("Inserida remuneração Detalhada de {Mes}/{Ano} do tipo {TipoRemuneracao}", mes, ano, titulo);
                }
                catch (Exception ex)
                {
                    if (!ex.Message.Contains("Duplicate"))
                        throw;
                    else
                        logger.LogWarning("Registro duplicado ignorado: {@Folha}", folha);
                }
            }
        }

        dbContext.SaveChanges();
    }

    private async Task ColetaPessoalGabinete(IBrowsingContext context, int idDeputado, int ano)
    {
        var address = $"https://www.camara.leg.br/deputados/{idDeputado}/pessoal-gabinete?ano={ano}";
        var document = await context.OpenAsyncAutoRetry(address);
        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
        {
            logger.LogError("{StatusCode}: {Url}", document.StatusCode, document.Url);
            return;
        }

        var deputado = document.QuerySelector(".titulo-internal").TextContent;
        var anoColeta = document.QuerySelector(".subtitulo-internal").TextContent;
        logger.LogDebug("{Ano} para o deputado {Deputado}", anoColeta, deputado);

        var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
        if (tabelas.Length == 0)
        {
            logger.LogError("Nenhuma tabela encontrada! Url: {Url}", address);
            return;
        }

        foreach (var tabela in tabelas)
        {
            var linhas = tabela.QuerySelectorAll("tbody tr");
            if (linhas.Length == 0)
            {
                logger.LogError("Nenhuma linha encontrada! Url: {Url}", address);
                return;
            }

            foreach (var linha in linhas)
            {
                var colunas = linha.QuerySelectorAll("td");

                var periodo = colunas[3].TextContent.Trim();
                var periodoPartes = periodo.Split(" ");
                DateTime dataInicial = Convert.ToDateTime(periodoPartes[1]);
                DateTime? dataFinal = periodoPartes.Length == 4 ? Convert.ToDateTime(periodoPartes[3]) : null;

                var link = (colunas[4].FirstElementChild as IHtmlAnchorElement).Href; // todo?
                var chave = link.Replace("https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/", "");

                var funcionario = new CamaraFederalDeputadoFuncionarioTemp()
                {
                    IdDeputado = idDeputado,
                    Chave = chave,
                    Nome = colunas[0].TextContent.Trim(),
                    GrupoFuncional = colunas[1].TextContent.Trim(),
                    Nivel = colunas[2].TextContent.Trim(),
                    PeriodoDe = dataInicial,
                    PeriodoAte = dataFinal,
                };

                dbContext.CamaraEstadualDeputadoFuncionarioTemps.Add(funcionario);
            }
        }

        dbContext.SaveChanges();
    }

    //    //    private async Task ColetaPessoalGabinete(IBrowsingContext context, int idDeputado, int ano)
    //    //    {
    //    //        var dataControle = DateTime.Today.ToString("yyyy-MM-dd");
    //    //        var address = $"https://www.camara.leg.br/deputados/{idDeputado}/pessoal-gabinete?ano={ano}";
    //    //        var document = await context.OpenAsyncAutoRetry(address);
    //    //        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
    //    //        {
    //    //            logger.LogError("{StatusCode}: {Url}", document.StatusCode, document.Url);
    //    //            return;
    //    //        };

    //    //        var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
    //    //        foreach (var tabela in tabelas)
    //    //        {
    //    //            var linhas = tabela.QuerySelectorAll("tbody tr");
    //    //            foreach (var linha in linhas)
    //    //            {
    //    //                int idSecretario;
    //    //                var colunas = linha.QuerySelectorAll("td");
    //    //                var nome = colunas[0].TextContent.Trim();
    //    //                var grupo = colunas[1].TextContent.Trim();
    //    //                var cargo = colunas[2].TextContent.Trim();
    //    //                var periodo = colunas[3].TextContent.Trim();
    //    //                var link = (colunas[4].FirstElementChild as IHtmlAnchorElement).Href; // todo?
    //    //                var chave = link.Replace("https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/", "");

    //    //                var periodoPartes = periodo.Split(" ");
    //    //                DateTime? dataInicial = Convert.ToDateTime(periodoPartes[1]);
    //    //                DateTime? dataFinal = periodoPartes.Length == 4 ? Convert.ToDateTime(periodoPartes[3]) : null;

    //    //                var sqlSelectSecretario = "select id FROM camara.cf_funcionario where chave = @chave";

    //    //                var objId = ExecuteScalar(sqlSelectSecretario);
    //    //                if (objId is not null)
    //    //                {
    //    //                    idSecretario = Convert.ToInt32(objId);


    //    //                    connection.Execute($"UPDATE cf_funcionario SET controle='{dataControle}', processado=0 WHERE id=@id;");
    //    //                }
    //    //                else
    //    //                {
    //    //                    var sqlInsertSecretario = "insert into cf_funcionario (chave, nome, processado, controle) values (@chave, @nome, 0, @controle);";



    //    //                    connection.Execute(sqlInsertSecretario);

    //    //                    idSecretario = Convert.ToInt32(ExecuteScalar("select LAST_INSERT_ID()"));
    //    //                }

    //    //                var sqlInsertSecretarioContratacao = @"
    //    //insert IGNORE into cf_funcionario_contratacao (id_cf_funcionario, id_cf_deputado, id_cf_funcionario_grupo_funcional, id_cf_funcionario_nivel, periodo_de, periodo_ate) 
    //    //values (@id_cf_funcionario, @id_cf_deputado, (select id FROM camara.cf_funcionario_grupo_funcional where nome ILIKE @grupo), (select id FROM camara.cf_funcionario_nivel where nome ILIKE @nivel), @periodo_de, @periodo_ate)
    //    //-- ON DUPLICATE KEY UPDATE periodo_ate = @periodo_ate";








    //    //                connection.Execute(sqlInsertSecretarioContratacao);

    //    //                //if (objId is not null && (dataFinal == null || dataFinal >= new DateTime(2021, 6, 1)))
    //    //                //{
    //    //                //    var sqlUpdateSecretario = "UPDATE cf_funcionario set processado = 0 where id = @idSecretario;";

    //    //                //    connection.Execute(sqlUpdateSecretario);
    //    //                //}
    //    //            }
    //    //        }
    //    //    }

    private async Task ColetaAuxilioMoradia(IBrowsingContext context, int idDeputado, int ano)
    {
        var sqlInsertAuxilioMoradia = "insert ignore into cf_deputado_auxilio_moradia (id_cf_deputado, ano, mes, valor) values (@id_cf_deputado, @ano, @mes, @valor)";
        var address = $"https://www.camara.leg.br/deputados/{idDeputado}/auxilio-moradia/?ano={ano}";
        var document = await context.OpenAsyncAutoRetry(address);
        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
        {
            Console.WriteLine(document.StatusCode + ":" + document.Url);
            return;
        }

        var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
        foreach (var tabela in tabelas)
        {
            var linhas = tabela.QuerySelectorAll("tbody tr");
            foreach (var linha in linhas)
            {
                var colunas = linha.QuerySelectorAll("td");
                var mes = Convert.ToInt32(colunas[0].TextContent.Trim());
                var valor = Convert.ToDecimal(colunas[1].TextContent.Trim());





                connection.Execute(sqlInsertAuxilioMoradia);
            }
        }
    }

    private async Task ColetaMissaoOficial(IBrowsingContext context, int idDeputado, int ano)
    {
        var sqlInsertAuxilioMoradia =
            "insert ignore into cf_deputado_missao_oficial (id_cf_deputado, periodo, assunto, destino, passagens, diarias, relatorio) values (@id_cf_deputado, @periodo, @assunto, @destino, @passagens, @diarias, @relatorio)";
        var address = $"https://www.camara.leg.br/deputados/{idDeputado}/missao-oficial/?ano={ano}";
        var document = await context.OpenAsyncAutoRetry(address);
        if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
        {
            Console.WriteLine(document.StatusCode + ":" + document.Url);
            return;
        }

        var tabelas = document.QuerySelectorAll(".secao-conteudo .table");
        foreach (var tabela in tabelas)
        {
            var linhas = tabela.QuerySelectorAll("tbody tr");
            foreach (var linha in linhas)
            {
                var colunas = linha.QuerySelectorAll("td");
                var periodo = colunas[0].TextContent.Trim();
                var assunto = colunas[1].TextContent.Trim();
                var destino = colunas[2].TextContent.Trim();
                var passagens = Convert.ToDecimal(colunas[3].TextContent.Trim());
                var diarias = Convert.ToDecimal(colunas[4].TextContent.Trim());
                var relatorio = (colunas[5].FirstElementChild as IHtmlAnchorElement)?.Href;








                connection.Execute(sqlInsertAuxilioMoradia);
            }
        }
    }

    private Dictionary<string, string> colunasBanco = new Dictionary<string, string>();
    //public void DefineColunasRemuneracaoSecretarios()
    //{
    //    colunasBanco = new Dictionary<string, string>();
    //    // 1 - Remuneração Básica
    //    colunasBanco.Add("a - Remuneração Fixa", nameof(DeputadoFederalFuncionarioRemuneracao.RemuneracaoFixa));
    //    colunasBanco.Add("b - Vantagens de Natureza Pessoal", nameof(DeputadoFederalFuncionarioRemuneracao.VantagensNaturezaPessoal));
    //    // 2 - Remuneração Eventual/Provisória
    //    colunasBanco.Add("a - Função ou Cargo em Comissão", nameof(DeputadoFederalFuncionarioRemuneracao.FuncaoOuCargoEmComissao));
    //    colunasBanco.Add("b - Gratificação Natalina", nameof(DeputadoFederalFuncionarioRemuneracao.GratificacaoNatalina));
    //    colunasBanco.Add("c - Férias (1/3 Constitucional)", nameof(DeputadoFederalFuncionarioRemuneracao.Ferias));
    //    colunasBanco.Add("d - Outras Remunerações Eventuais/Provisórias(*)", nameof(DeputadoFederalFuncionarioRemuneracao.OutrasRemuneracoes));
    //    // 3 - Abono Permanência
    //    colunasBanco.Add("a - Abono Permanência", nameof(DeputadoFederalFuncionarioRemuneracao.AbonoPermanencia));
    //    // 4 - Descontos Obrigatórios(-)
    //    colunasBanco.Add("a - Redutor Constitucional", nameof(DeputadoFederalFuncionarioRemuneracao.RedutorConstitucional));
    //    colunasBanco.Add("b - Contribuição Previdenciária", nameof(DeputadoFederalFuncionarioRemuneracao.ContribuicaoPrevidenciaria));
    //    colunasBanco.Add("c - Imposto de Renda", nameof(DeputadoFederalFuncionarioRemuneracao.ImpostoRenda));
    //    // 5 - Remuneração após Descontos Obrigatórios
    //    colunasBanco.Add("a - Remuneração após Descontos Obrigatórios", nameof(DeputadoFederalFuncionarioRemuneracao.ValorLiquido));
    //    // 6 - Outros
    //    colunasBanco.Add("a - Diárias", nameof(DeputadoFederalFuncionarioRemuneracao.ValorDiarias));
    //    colunasBanco.Add("b - Auxílios", nameof(DeputadoFederalFuncionarioRemuneracao.ValorAuxilios));
    //    colunasBanco.Add("c - Vantagens Indenizatórias", nameof(DeputadoFederalFuncionarioRemuneracao.ValorVantagens));
    //}

    //    public void ColetaRemuneracaoSecretarios()
    //    {
    //        // 1 - Remuneração Básica
    //        colunasBanco.Add("a - Remuneração Fixa", nameof(DeputadoFederalFuncionarioRemuneracao.RemuneracaoFixa));
    //        colunasBanco.Add("b - Vantagens de Natureza Pessoal", nameof(DeputadoFederalFuncionarioRemuneracao.VantagensNaturezaPessoal));
    //        // 2 - Remuneração Eventual/Provisória
    //        colunasBanco.Add("a - Função ou Cargo em Comissão", nameof(DeputadoFederalFuncionarioRemuneracao.FuncaoOuCargoEmComissao));
    //        colunasBanco.Add("b - Gratificação Natalina", nameof(DeputadoFederalFuncionarioRemuneracao.GratificacaoNatalina));
    //        colunasBanco.Add("c - Férias (1/3 Constitucional)", nameof(DeputadoFederalFuncionarioRemuneracao.Ferias));
    //        colunasBanco.Add("d - Outras Remunerações Eventuais/Provisórias(*)", nameof(DeputadoFederalFuncionarioRemuneracao.OutrasRemuneracoes));
    //        // 3 - Abono Permanência
    //        colunasBanco.Add("a - Abono Permanência", nameof(DeputadoFederalFuncionarioRemuneracao.AbonoPermanencia));
    //        // 4 - Descontos Obrigatórios(-)
    //        colunasBanco.Add("a - Redutor Constitucional", nameof(DeputadoFederalFuncionarioRemuneracao.RedutorConstitucional));
    //        colunasBanco.Add("b - Contribuição Previdenciária", nameof(DeputadoFederalFuncionarioRemuneracao.ContribuicaoPrevidenciaria));
    //        colunasBanco.Add("c - Imposto de Renda", nameof(DeputadoFederalFuncionarioRemuneracao.ImpostoRenda));
    //        // 5 - Remuneração após Descontos Obrigatórios
    //        colunasBanco.Add("a - Remuneração após Descontos Obrigatórios", nameof(DeputadoFederalFuncionarioRemuneracao.ValorLiquido));
    //        // 6 - Outros
    //        colunasBanco.Add("a - Diárias", nameof(DeputadoFederalFuncionarioRemuneracao.ValorDiarias));
    //        colunasBanco.Add("b - Auxílios", nameof(DeputadoFederalFuncionarioRemuneracao.ValorAuxilios));
    //        colunasBanco.Add("c - Vantagens Indenizatórias", nameof(DeputadoFederalFuncionarioRemuneracao.ValorVantagens));

    //        var sqlDeputados = @"
    //SELECT f.id as id_cf_funcionario, f.chave 
    //FROM camara.cf_funcionario f
    //WHERE f.processado = 0
    //order BY f.id
    //";

    //        ConcurrentQueue<Dictionary<string, object>> queue;
    //        // Migrated to IDbConnection - removed AppDb usage
    //        {
    //            var dcDeputado = connection.Query<Dictionary<string, object>>(sqlDeputados).ToList();
    //            queue = new ConcurrentQueue<Dictionary<string, object>>(dcDeputado);
    //        }

    //        int totalRecords = 10;
    //        Task[] tasks = new Task[totalRecords];
    //        for (int i = 0; i < totalRecords; ++i)
    //        {
    //            tasks[i] = ProcessaFilaColetaRemuneracaoSecretarios(queue);
    //        }

    //        Task.WaitAll(tasks);
    //    }

    //    private async Task ProcessaFilaColetaRemuneracaoSecretarios(ConcurrentQueue<Dictionary<string, object>> queue)
    //    {
    //        Dictionary<string, object> secretario = null;
    //        var context = httpClient.CreateAngleSharpContext();

    //        while (queue.TryDequeue(out secretario))
    //        {
    //            if (secretario is null) continue;

    //            logger.LogDebug("Inciando coleta da remuneração para os funcionario {IdFuncionario}", secretario["id_cf_funcionario"]);
    //            await ConsultarRemuneracaoSecretario(colunasBanco, secretario, context);
    //        }
    //    }

    static readonly object padlock = new object();
    //    private async Task ConsultarRemuneracaoSecretario(Dictionary<string, string> colunasBanco, Dictionary<string, object> secretario, IBrowsingContext context)
    //    {
    //        var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
    //        var dataCorte = new DateTime(2008, 2, 1);

    //        using (logger.BeginScope(new Dictionary<string, object> { ["Funcionario"] = Convert.ToInt32(secretario["id_cf_funcionario"]) }))
    //        {
    //            try
    //            {
    //                var erroColeta = false;
    //                var idFuncionario = Convert.ToInt32(secretario["id_cf_funcionario"]);
    //                //var idFuncionarioContratacao = Convert.ToInt32(secretario["id_cf_funcionario_contratacao"]);
    //                //var idDeputado = Convert.ToInt32(secretario["id_cf_deputado"]);
    //                var chave = secretario["chave"].ToString();

    //                var folhas = new List<DeputadoFederalFuncionarioRemuneracao>();
    //                DateTime dtUltimaRemuneracaoColetada;
    //                IEnumerable<DeputadoFederalFuncionarioContratacao> contratacoes;
    //                lock (padlock)
    //                {
    //                    var sqlUltimaRemuneracao = @"SELECT MAX(referencia) FROM camara.cf_funcionario_remuneracao WHERE id_cf_funcionario = @id_cf_funcionario";
    //                    var objUltimaRemuneracaoColetada = connection.ExecuteScalar(sqlUltimaRemuneracao, new { id_cf_funcionario = idFuncionario });
    //                    dtUltimaRemuneracaoColetada = !Convert.IsDBNull(objUltimaRemuneracaoColetada) ? Convert.ToDateTime(objUltimaRemuneracaoColetada) : DateTime.MinValue;

    //                    contratacoes = connection.GetList<DeputadoFederalFuncionarioContratacao>(new { id_cf_funcionario = idFuncionario });
    //                }

    //                var address = $"https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/{chave}";
    //                var document = await context.OpenAsyncAutoRetry(address);
    //                if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
    //                {
    //                    logger.LogError(document.StatusCode + ":" + address);
    //                    return;
    //                }

    //                int anoSelecionado = 0, mesSelecionado = 0;
    //                try
    //                {
    //                    Regex myRegex = new Regex(@"ano=(\d{4})&mes=(\d{1,2})", RegexOptions.Singleline);
    //                    // window.location.href = 'https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/BDEdGyYrxGdG42MO7egk?ano=2021&mes=7';
    //                    var linkMatch = myRegex.Match(document.Scripts[1].InnerHtml);
    //                    if (linkMatch.Length > 0)
    //                    {
    //                        anoSelecionado = Convert.ToInt32(linkMatch.Groups[1].Value);
    //                        mesSelecionado = Convert.ToInt32(linkMatch.Groups[2].Value);

    //                        var dataColeta = new DateTime(anoSelecionado, mesSelecionado, 1);
    //                        if (dataColeta < dataCorte)
    //                        {
    //                            return;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        if (document.QuerySelector(".remuneracao-funcionario") != null)
    //                        {
    //                            logger.LogError("Remuneração indisponível! Mensagem: {MensagemErro}", document.QuerySelector(".remuneracao-funcionario").TextContent.Trim());
    //                            MarcarComoProcessado(idFuncionario);
    //                        }

    //                        return;
    //                    }
    //                }
    //                catch (Exception ex)
    //                {
    //                    logger.LogError(idFuncionario + ":" + ex.Message);
    //                    if (document.QuerySelector(".remuneracao-funcionario") != null)
    //                        logger.LogError(document.QuerySelector(".remuneracao-funcionario").TextContent.Trim());

    //                    //MarcarComoProcessado(db, idFuncionario);
    //                    return;
    //                }

    //                var addressFull = address + $"?ano={anoSelecionado}&mes={mesSelecionado}";
    //                if (document.Location.Href != addressFull)
    //                {
    //                    document = await context.OpenAsyncAutoRetry(addressFull);
    //                    if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
    //                    {
    //                        logger.LogError(document.StatusCode + ":" + addressFull);
    //                        return;
    //                    }
    //                }

    //                var anos = (document.GetElementById("anoRemuneracao") as IHtmlSelectElement).Options.OrderByDescending(x => Convert.ToInt32(x.Text));
    //                if (!anos.Any())
    //                {
    //                    logger.LogError("Anos indisponiveis:" + document.Location.Href);
    //                    return;
    //                }


    //                foreach (var ano in anos)
    //                {
    //                    using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano.Value }))
    //                    {
    //                        if (ano.OuterHtml.Contains("display: none;")) continue;
    //                        if (Convert.ToInt32(ano.InnerHtml) < dtUltimaRemuneracaoColetada.Year) continue;

    //                        //Console.WriteLine();
    //                        //Console.WriteLine($"Ano: {ano.Text}");
    //                        address = $"https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/{ano.Value}";
    //                        if (document.Location.Href != address)
    //                        {
    //                            document = await context.OpenAsyncAutoRetry(address);

    //                            if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
    //                            {
    //                                logger.LogError(document.StatusCode + ":" + address);
    //                                erroColeta = true;
    //                                continue;
    //                            }
    //                        }

    //                        var calendario = document.QuerySelectorAll(".linha-tempo li");
    //                        var meses = (document.GetElementById("mesRemuneracao") as IHtmlSelectElement).Options
    //                            .Select(x => Convert.ToInt32(x.Value))
    //                            .OrderByDescending(x => x);

    //                        if (!meses.Any())
    //                        {
    //                            logger.LogError("Meses indisponiveis:" + document.Location.Href);
    //                            erroColeta = true;
    //                            continue;
    //                        }

    //                        foreach (var mes in meses)
    //                        {
    //                            using (logger.BeginScope(new Dictionary<string, object> { ["Mes"] = mes }))
    //                            {
    //                                var dataReferencia = new DateTime(Convert.ToInt32(ano.Text), mes, 1);
    //                                if (dataReferencia < dataCorte)
    //                                {
    //                                    logger.LogDebug("Data de coleta {DataColeta:yyyy-MM-dd} fora do pediodo de corte da coleta {DataCorte:yyyy-MM-dd}", dataReferencia, dataCorte);
    //                                    continue;
    //                                }

    //                                var elMesSelecionadoNoCalendario = (calendario[mes - 1] as IHtmlListItemElement);
    //                                if (elMesSelecionadoNoCalendario.IsHidden || elMesSelecionadoNoCalendario.OuterHtml.Contains("display: none;") || mes > mesSelecionado)
    //                                {
    //                                    logger.LogDebug(
    //                                        "Não há remuneração para {MesRemuneracaoExtenso}[{MesRemuneracao:00}]/{AnoRemuneracao} #1",
    //                                        elMesSelecionadoNoCalendario.TextContent.Trim().Split(" ")[1], mes, ano.Text);
    //                                    continue;
    //                                }
    //                                if (new DateTime(Convert.ToInt32(ano.InnerHtml), mes, 1) <= dtUltimaRemuneracaoColetada)
    //                                {
    //                                    logger.LogDebug("Remuneração ja coletada para {MesRemuneracao}/{AnoRemuneracao}", mes, ano.Text);
    //                                    continue;
    //                                }

    //                                address = $"https://www.camara.leg.br/transparencia/recursos-humanos/remuneracao/{chave}?ano={ano.Text}&mes={mes}";
    //                                if (document.Location.Href != address)
    //                                {
    //                                    document = await context.OpenAsyncAutoRetry(address);
    //                                    if (document.StatusCode != HttpStatusCode.OK || document.Url.Contains("error"))
    //                                    {
    //                                        logger.LogError(document.StatusCode + ":" + address);
    //                                        erroColeta = true;
    //                                        continue;
    //                                    }
    //                                }

    //                                var dadosIndisponiveis = document.QuerySelectorAll(".remuneracao-funcionario");
    //                                if (dadosIndisponiveis.Any() && dadosIndisponiveis[0].TextContent.Trim() == "Ainda não há dados disponíveis.")
    //                                {
    //                                    logger.LogDebug(
    //                                        "Não há remuneração para {MesRemuneracaoExtenso}[{MesRemuneracao:00}]/{AnoRemuneracao} #2",
    //                                        elMesSelecionadoNoCalendario.TextContent.Trim().Split(" ")[1], mes, ano.Text);
    //                                    continue;
    //                                }

    //                                var folhasPagamento = document.QuerySelectorAll(".remuneracao-funcionario__info");
    //                                if (!folhasPagamento.Any())
    //                                {
    //                                    logger.LogWarning("Dados indisponiveis: " + address);
    //                                    erroColeta = true;
    //                                    return; // Erro no funcionario, abortar coleta dele
    //                                }

    //                                foreach (var folhaPagamento in folhasPagamento)
    //                                {
    //                                    var titulo = folhaPagamento.QuerySelector(".remuneracao-funcionario__mes-ano").TextContent;

    //                                    using (logger.BeginScope(new Dictionary<string, object> { ["Folha"] = titulo }))
    //                                    {
    //                                        if (titulo.Split("Permanência")[0].Trim() != $"{mes:00}{ano.Text}")
    //                                        {
    //                                            throw new NotImplementedException("Algo esta errado!");
    //                                        }

    //                                        var folha = new DeputadoFederalFuncionarioRemuneracao();
    //                                        IEnumerable<PropertyInfo> props = folha.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

    //                                        var cabecalho = folhaPagamento.QuerySelectorAll(".remuneracao-funcionario__info-item");
    //                                        var categoriaFuncional = cabecalho.FirstOrDefault(x => x.TextContent.StartsWith("Categoria funcional"))?.TextContent.Split(":")[1].Trim();
    //                                        var dataExercicio = cabecalho.FirstOrDefault(x => x.TextContent.StartsWith("Data de exercício"))?.TextContent.Split(":")[1].Trim();
    //                                        var cargo = cabecalho.FirstOrDefault(x => x.TextContent.StartsWith("Cargo"))?.TextContent.Split(":")[1].Trim();
    //                                        var nivel = cabecalho.FirstOrDefault(x => x.TextContent.StartsWith("Função/cargo em comissão"))?.TextContent.Split(":")[1].Trim();

    //                                        var linhas = folhaPagamento.QuerySelectorAll(".tabela-responsiva--remuneracao-funcionario tbody tr");
    //                                        foreach (var linha in linhas)
    //                                        {
    //                                            var colunas = linha.QuerySelectorAll("td");
    //                                            if (!colunas.Any()) continue;

    //                                            var descricao = colunas[0].TextContent.Trim();
    //                                            var valor = Convert.ToDecimal(colunas[1].TextContent.Trim());

    //                                            var propInfo = folha.GetType().GetProperty(colunasBanco[descricao]);
    //                                            propInfo.SetValue(folha, valor, null);
    //                                        }

    //                                        byte tipo = 0;
    //                                        switch (titulo.Split("�")[1].Trim())
    //                                        {
    //                                            case "FOLHA NORMAL":
    //                                                tipo = 1;
    //                                                break;
    //                                            case "FOLHA COMPLEMENTAR":
    //                                                tipo = 2;
    //                                                break;
    //                                            case "FOLHA COMPLEMENTAR ESPECIAL":
    //                                                tipo = 3;
    //                                                break;
    //                                            case "FOLHA DE ADIANTAMENTO GRATIFICAÇÃO NATALINA":
    //                                                tipo = 4;
    //                                                break;
    //                                            case "FOLHA DE GRATIFICAÇÃO NATALINA":
    //                                                tipo = 5;
    //                                                break;
    //                                            default:
    //                                                throw new NotImplementedException();
    //                                        }

    //                                        folha.IdFuncionario = idFuncionario;

    //                                        folha.Tipo = tipo;
    //                                        folha.Nivel = nivel;
    //                                        folha.Referencia = dataReferencia;

    //                                        if (!string.IsNullOrEmpty(dataExercicio))
    //                                            folha.Contratacao = DateOnly.Parse(dataExercicio, cultureInfo);

    //                                        folha.ValorBruto =
    //                                            folha.RemuneracaoFixa +
    //                                            folha.VantagensNaturezaPessoal +
    //                                            folha.FuncaoOuCargoEmComissao +
    //                                            folha.GratificacaoNatalina +
    //                                            folha.Ferias +
    //                                            folha.OutrasRemuneracoes +
    //                                            folha.AbonoPermanencia;

    //                                        var dataContratacao = Convert.ToDateTime(dataExercicio, cultureInfo);
    //                                        folha.ValorOutros = folha.ValorDiarias + folha.ValorAuxilios + folha.ValorVantagens;
    //                                        folha.ValorTotal = folha.ValorBruto + folha.ValorOutros;

    //                                        var contratacao = contratacoes.FirstOrDefault(c => c.PeriodoDe == dataContratacao);
    //                                        if (contratacao == null)
    //                                            contratacao = contratacoes.FirstOrDefault(c => dataReferencia.IsBetween(c.PeriodoDe, c.PeriodoAte));

    //                                        if (contratacao != null)
    //                                        {
    //                                            folha.IdFuncionarioContratacao = contratacao.Id;
    //                                            folha.IdDeputado = contratacao.IdDeputado;

    //                                            if (contratacao.IdCargo == null)
    //                                            {
    //                                                lock (padlock)
    //                                                {
    //                                                    contratacao.IdCargo = connection.QuerySingleOrDefault<byte?>("SELECT id FROM camara.cf_funcionario_cargo WHERE nome ILIKE @nome", new { nome = cargo });

    //                                                    if (contratacao.IdGrupoFuncional == null && !string.IsNullOrEmpty(categoriaFuncional))
    //                                                        contratacao.IdGrupoFuncional = connection.QuerySingleOrDefault<byte?>("SELECT id FROM camara.cf_funcionario_grupo_funcional WHERE nome ILIKE @nome", new { nome = categoriaFuncional });

    //                                                    if (contratacao.IdNivel == null && !string.IsNullOrEmpty(nivel))
    //                                                        contratacao.IdNivel = connection.QuerySingleOrDefault<byte?>("SELECT id FROM camara.cf_funcionario_nivel WHERE nome ILIKE @nome", new { nome = nivel });

    //                                                    repositoryService.Update(contratacao);
    //                                                }
    //                                            }
    //                                        }
    //                                        else
    //                                        {
    //                                            logger.LogWarning("Não foi identificada a contratação do funcionario {IdFuncionario} em {Mes}/{Ano}", idFuncionario, mes, ano.Text);
    //                                        }

    //                                        folhas.Add(folha);

    //                                        logger.LogDebug("Inserida remuneração de {Mes}/{Ano} do tipo {TipoRemuneracao}", mes, ano.Text, titulo);
    //                                    }
    //                                }
    //                            }
    //                        }
    //                    }
    //                }

    //                if (!erroColeta)
    //                {
    //                    if (folhas.Any())
    //                    {
    //                        lock (padlock)
    //                        {
    //                            foreach (var folha in folhas)
    //                            {
    //                                try
    //                                {
    //                                    repositoryService.Insert(folha);
    //                                }
    //                                catch (Exception ex)
    //                                {
    //                                    if (!ex.Message.Contains("Duplicate"))
    //                                        throw;
    //                                    else
    //                                        logger.LogWarning("Registro duplicado ignorado: {@Folha}", folha);
    //                                }
    //                            }
    //                        }

    //                        logger.LogDebug("Coleta finalizada para o funcionario {IdFuncionario} com {Registros} registros", idFuncionario, folhas.Count());
    //                    }
    //                    else
    //                    {
    //                        logger.LogWarning("Coleta finalizada para o funcionario {IdFuncionario} sem registros", idFuncionario);
    //                    }

    //                    MarcarComoProcessado(idFuncionario);
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                Console.WriteLine(secretario["id_cf_funcionario"].ToString() + ":" + ex.Message);
    //                Thread.Sleep(TimeSpan.FromMinutes(1));
    //            }
    //        }
    //    }

    //    private void MarcarComoProcessado(int idFuncionario)
    //    {
    //        lock (padlock)
    //            connection.Execute("update cf_funcionario set processado = 1 where id = @id", new { id = idFuncionario });
    //    }

    //    public async Task ColetaDadosFuncionarios()
    //    {
    //        var addresses = new string[]
    //        {
    //            "https://www.camara.leg.br/transparencia/recursos-humanos/funcionarios?search=&areaDeAtuacao=&categoriaFuncional=&lotacao=&situacao=Em%20exerc%C3%ADcio&pagina=",
    //            "https://www.camara.leg.br/transparencia/recursos-humanos/funcionarios?search=&categoriaFuncional=&areaDeAtuacao=&situacao=Aposentado&pagina=",
    //            "https://www.camara.leg.br/transparencia/recursos-humanos/funcionarios?search=&categoriaFuncional=&areaDeAtuacao=&situacao=Licenciado&pagina=",
    //            "https://www.camara.leg.br/transparencia/recursos-humanos/funcionarios?search=&categoriaFuncional=&areaDeAtuacao=&situacao=Cedido&pagina="
    //        };

    //        var sqlInsert = @"
    //insert ignore into cf_funcionario_temp (chave, nome, categoria_funcional, area_atuacao, situacao{0})
    //values (@chave, @nome, @categoria_funcional, @area_atuacao, @situacao{1})
    //";
    //        var context = httpClient.CreateAngleSharpContext();

    //        // Migrated to IDbConnection - removed AppDb usage
    //        {
    //            connection.Execute("truncate table cf_funcionario_temp");

    //            foreach (var address in addresses)
    //            {
    //                var pagina = 0;
    //                while (true)
    //                {
    //                    var document = await context.OpenAsyncAutoRetry(address + (++pagina).ToString());
    //                    if (document.StatusCode != HttpStatusCode.OK)
    //                    {
    //                        Console.WriteLine(document.StatusCode);
    //                        Thread.Sleep(TimeSpan.FromSeconds(30));

    //                        document = await context.OpenAsyncAutoRetry(address + (++pagina).ToString());
    //                        if (document.StatusCode != HttpStatusCode.OK)
    //                        {
    //                            Console.WriteLine(document.StatusCode);
    //                            break;
    //                        }
    //                        ;
    //                    }
    //                    ;

    //                    var linhas = document.QuerySelectorAll(".l-busca__secao--resultados table.tabela-responsiva tbody tr");
    //                    foreach (var linha in linhas)
    //                    {
    //                        string colunasDB = "", valoresDB = "";

    //                        var colunas = linha.QuerySelectorAll("td");
    //                        var nome = colunas[0].TextContent.Trim();
    //                        var categoria = colunas[1].TextContent.Trim();
    //                        var area = colunas[2].TextContent.Trim();
    //                        var situacao = colunas[3].TextContent.Trim();

    //                        var chave = (colunas[0].FirstElementChild as IHtmlAnchorElement).GetAttribute("data-target");

    //                        if (categoria.StartsWith("Deputado")) categoria = "Deputado";
    //                        if (categoria.StartsWith("Ex-deputado")) categoria = "Ex-deputado";
    //                        if (area == "�") area = null;

    //                        Console.WriteLine();
    //                        Console.WriteLine($"Funcionario: {nome}");

    //                        var lstInfo = document.QuerySelectorAll(".modal--funcionario" + chave + " .lista-funcionario__item");
    //                        foreach (var info in lstInfo)
    //                        {
    //                            try
    //                            {
    //                                var titulo = info.QuerySelector(".lista-funcionario__titulo")?.TextContent.Trim();
    //                                var valor = info?.TextContent?.Replace(titulo, "").Trim();

    //                                switch (titulo.Replace(":", ""))
    //                                {
    //                                    case "Cargo":
    //                                        colunasDB += ", cargo";
    //                                        valoresDB += ", @cargo";

    //                                        break;
    //                                    case "Nível":
    //                                        colunasDB += ", nivel";
    //                                        valoresDB += ", @nivel";

    //                                        break;
    //                                    case "Função comissionada":
    //                                        colunasDB += ", funcao_comissionada";
    //                                        valoresDB += ", @funcao_comissionada";

    //                                        break;
    //                                    case "Local de trabalho":
    //                                        colunasDB += ", local_trabalho";
    //                                        valoresDB += ", @local_trabalho";

    //                                        break;
    //                                    case "Data da designação da função":
    //                                        colunasDB += ", data_designacao_funcao";
    //                                        valoresDB += ", @data_designacao_funcao";

    //                                        break;
    //                                    case "Situação":
    //                                    case "Categoria funcional":
    //                                    case "área de atuação":
    //                                        break;
    //                                    default:
    //                                        throw new NotImplementedException($"Vefificar beneficios do funcionario {nome}.");
    //                                }

    //                                Console.WriteLine($"{titulo}: {valor}");
    //                            }
    //                            catch (Exception ex)
    //                            {
    //                                logger.LogError(ex, ex.Message);
    //                            }
    //                        }






    //                        connection.Execute(string.Format(sqlInsert, colunasDB, valoresDB));
    //                    }

    //                    if (linhas.Count() < 20)
    //                    {
    //                        logger.LogDebug("Parando na pagina {Pagina}!", pagina);
    //                        break;
    //                    }
    //                }
    //            }



    //            ExecuteNonQuery(@"
    //INSERT ignore INTO cf_funcionario(chave, nome)
    //SELECT chave, nome FROM camara.cf_funcionario_temp t");

    //            ExecuteNonQuery(@"
    //INSERT INTO cf_funcionario_contratacao 
    //	(id_cf_deputado, id_cf_funcionario, id_cf_funcionario_grupo_funcional, id_cf_funcionario_cargo, id_cf_funcionario_nivel, id_cf_funcionario_funcao_comissionada, 
    //		id_cf_funcionario_area_atuacao, id_cf_funcionario_local_trabalho, id_cf_funcionario_situacao, periodo_de, periodo_ate)
    //SELECT NULL, f.id, gf.id, c.id, n.id, fc.id, aa.id, lt.id, s.id, t.data_designacao_funcao, null 
    //FROM camara.cf_funcionario_temp t
    //JOIN cf_funcionario f ON f.chave = t.chave
    //LEFT JOIN cf_funcionario_grupo_funcional gf ON gf.nome = t.categoria_funcional
    //LEFT JOIN cf_funcionario_cargo c ON c.nome = t.cargo
    //LEFT JOIN cf_funcionario_nivel n ON n.nome = t.nivel
    //LEFT JOIN cf_funcionario_funcao_comissionada fc ON fc.nome = t.funcao_comissionada
    //LEFT JOIN cf_funcionario_area_atuacao aa ON aa.nome = t.area_atuacao
    //LEFT JOIN cf_funcionario_local_trabalho lt ON lt.nome = t.local_trabalho
    //LEFT JOIN cf_funcionario_situacao s ON s.nome = t.situacao
    //LEFT JOIN cf_funcionario_contratacao ct ON ct.id_cf_funcionario = f.id
    //WHERE ct.id IS NULL");
    //        }
    //    }

    //    #endregion Importação Remuneração

    public void AtualizaParlamentarValores()
    {
        var dt = connection.Query("select id FROM camara.cf_deputado where processado != 2");
        object secretarios_ativos = 0;
        object valor_total_remuneracao;
        object valor_total_salario;
        object valor_total_auxilio_moradia;

        foreach (var dr in dt)
        {
            try
            {
                var obj = new { id_cf_deputado = Convert.ToInt32(dr.id) };
                valor_total_salario = connection.ExecuteScalar("select sum(valor) FROM camara.cf_deputado_remuneracao where id_cf_deputado=@id_cf_deputado;", obj);
                valor_total_auxilio_moradia = connection.ExecuteScalar("select sum(valor) FROM camara.cf_deputado_auxilio_moradia where id_cf_deputado=@id_cf_deputado;", obj);
                valor_total_remuneracao = connection.ExecuteScalar("select sum(valor) FROM camara.cf_deputado_verba_gabinete where id_cf_deputado=@id_cf_deputado;", obj);

                if (Convert.IsDBNull(valor_total_remuneracao) || Convert.ToDecimal(valor_total_remuneracao) == 0)
                    valor_total_remuneracao = connection.ExecuteScalar("select sum(valor_total) FROM camara.cf_funcionario_remuneracao where id_cf_deputado=@id_cf_deputado;", obj);

                connection.Execute(@"
update cf_deputado set 
    valor_total_remuneracao = @valor_total_remuneracao, 
    valor_total_salario = @valor_total_salario,
    valor_total_auxilio_moradia = @valor_total_auxilio_moradia 
where id=@id_cf_deputado", new
                {
                    valor_total_remuneracao = valor_total_remuneracao,
                    valor_total_salario = valor_total_salario,
                    valor_total_auxilio_moradia = valor_total_auxilio_moradia,
                    id_cf_deputado = dr.id
                }
                );
            }
            catch (Exception ex)
            {
                if (!ex.GetBaseException().Message.Contains("Out of range"))
                    throw;
            }
        }
    }
}
