using System.Globalization;
using System.Net;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using OfficeOpenXml;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Acre;
using OPS.Importador.Assembleias.Alagoas;
using OPS.Importador.Assembleias.Amapa;
using OPS.Importador.Assembleias.Amazonas;
using OPS.Importador.Assembleias.Bahia;
using OPS.Importador.Assembleias.Ceara;
using OPS.Importador.Assembleias.DistritoFederal;
using OPS.Importador.Assembleias.EspiritoSanto;
using OPS.Importador.Assembleias.Goias;
using OPS.Importador.Assembleias.Maranhao;
using OPS.Importador.Assembleias.MatoGrosso;
using OPS.Importador.Assembleias.MatoGrossoDoSul;
using OPS.Importador.Assembleias.MinasGerais;
using OPS.Importador.Assembleias.Para;
using OPS.Importador.Assembleias.Paraiba;
using OPS.Importador.Assembleias.Parana;
using OPS.Importador.Assembleias.Pernambuco;
using OPS.Importador.Assembleias.Piaui;
using OPS.Importador.Assembleias.RioDeJaneiro;
using OPS.Importador.Assembleias.RioGrandeDoNorte;
using OPS.Importador.Assembleias.RioGrandeDoSul;
using OPS.Importador.Assembleias.Rondonia;
using OPS.Importador.Assembleias.Roraima;
using OPS.Importador.Assembleias.SantaCatarina;
using OPS.Importador.Assembleias.SaoPaulo;
using OPS.Importador.Assembleias.Sergipe;
using OPS.Importador.Assembleias.Tocantins;
using OPS.Importador.Comum;
using OPS.Importador.Comum.Utilities;
using OPS.Infraestrutura;
using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace OPS.Importador
{
    internal class Program
    {
        public static Task Main(string[] args)
        {
            ExcelPackage.License.SetNonCommercialOrganization("OPS: Operação Política Supervisionada");

            //SqlMapper.AddTypeHandler(typeof(DateTime), new NpgsqlDateTimeHandler());
            //SqlMapper.AddTypeHandler(typeof(DateTime?), new NpgsqlDateTimeHandler());

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            Serilog.Debugging.SelfLog.Enable(Console.Error);

            CultureInfo ci = new CultureInfo("pt-BR");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(environmentName)) environmentName = "Development";

            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("Identifier", Guid.NewGuid().ToString())
                .Enrich.With<InvocationContextEnricher>();

            Log.Logger = loggerConfiguration.CreateLogger();

            var services = new ServiceCollection();
            services.AddOptions<AppSettings>().Bind(configuration.GetSection("AppSettings"));

            services.AddScoped<SenadoFederal.Senado>();
            services.AddScoped<CamaraFederal.Camara>();

            services.AddScoped<ImportacaoAcre>();
            services.AddScoped<ImportacaoAlagoas>();
            services.AddScoped<ImportacaoAmapa>();
            services.AddScoped<ImportacaoAmazonas>();
            services.AddScoped<ImportacaoBahia>();
            services.AddScoped<ImportacaoCeara>();
            services.AddScoped<ImportacaoDistritoFederal>();
            services.AddScoped<ImportacaoEspiritoSanto>();
            services.AddScoped<ImportacaoGoias>();
            services.AddScoped<ImportacaoMaranhao>();
            services.AddScoped<ImportacaoMatoGrosso>();
            services.AddScoped<ImportacaoMatoGrossoDoSul>();
            services.AddScoped<ImportacaoMinasGerais>();
            services.AddScoped<ImportacaoPara>();
            services.AddScoped<ImportacaoParaiba>();
            services.AddScoped<ImportacaoParana>();
            services.AddScoped<ImportacaoPernambuco>();
            services.AddScoped<ImportacaoPiaui>();
            services.AddScoped<ImportacaoRioDeJaneiro>();
            services.AddScoped<ImportacaoRioGrandeDoNorte>();
            services.AddScoped<ImportacaoRioGrandeDoSul>();
            services.AddScoped<ImportacaoRondonia>();
            services.AddScoped<ImportacaoRoraima>();
            services.AddScoped<ImportacaoSantaCatarina>();
            services.AddScoped<ImportacaoSaoPaulo>();
            services.AddScoped<ImportacaoSergipe>();
            services.AddScoped<ImportacaoTocantins>();

            services.AddScoped<CamaraFederal.ImportadorDespesasCamaraFederal>();
            services.AddScoped<SenadoFederal.ImportadorDespesasSenado>();

            //services.AddScoped<Presidencia>();

            services.AddScoped<Fornecedores.ImportacaoFornecedor>();
            services.AddScoped<FileManager>();
            services.AddScoped<HttpLogger>();
            services.AddDbContext<AppDbContext>(options => options
                   .UseNpgsql(configuration.GetConnectionString("AuditoriaContext"), sqlOptions => sqlOptions.CommandTimeout(120)),
                   ServiceLifetime.Transient);

            //services.AddRedaction();

            services.AddHttpClient<HttpClient>("ResilientClient", config =>
                {
                    //config.BaseAddress = new Uri("https://localhost:5001/api/");
                    config.Timeout = TimeSpan.FromHours(1);
                    config.DefaultRequestHeaders.Clear();
                    config.DefaultRequestHeaders.Add("User-Agent", Utils.DefaultUserAgent);
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    var handler = new HttpClientHandler()
                    {
                        AllowAutoRedirect = false,
                        MaxAutomaticRedirections = 1,
                    };

                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) =>
                        {
                            return true;
                        };

                    return handler;
                })
                //.ConfigurePrimaryHttpMessageHandler(() =>
                //{
                //    return new SocketsHttpHandler()
                //    {
                //        PooledConnectionLifetime = TimeSpan.FromSeconds(60),
                //        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(20),
                //        MaxConnectionsPerServer = 10
                //    };
                //})
                .AddPolicyHandler((services, request) => HttpPolicyExtensions
                    .HandleTransientHttpError() // HttpRequestException, 5XX and 408
                                                //.OrResult(response => (int)response.StatusCode == 429) // RetryAfter
                    .Or<TaskCanceledException>()
                    .Or<OperationCanceledException>()
                    .Or<TimeoutException>()
                    .WaitAndRetryAsync(new[]
                        {
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(10),
                            TimeSpan.FromSeconds(30),
                            TimeSpan.FromSeconds(60),
                        },
                        onRetry: (message, timespan, attempt, context) =>
                        {
                            services.GetService<ILogger<HttpClient>>()?
                                .LogWarning("Delaying for {delay} seconds, then making retry {retry}. Url: {Url}", timespan.TotalSeconds, attempt, message?.Result?.RequestMessage?.RequestUri);
                        }
                    )
                )
                .AddLogger<HttpLogger>(wrapHandlersPipeline: true);

            services.AddHttpClient<HttpClient>("DefaultClient", config =>
                {
                    //config.BaseAddress = new Uri("https://localhost:5001/api/");
                    config.Timeout = TimeSpan.FromHours(1);
                    config.DefaultRequestHeaders.Clear();
                    config.DefaultRequestHeaders.Add("User-Agent", Utils.DefaultUserAgent);
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler()
                    {
                        AllowAutoRedirect = true,
                    };
                })
                .AddLogger<HttpLogger>(wrapHandlersPipeline: true);
                //.AddExtendedHttpClientLogging(options =>
                //{
                //    //options.RequestPathParameterRedactionMode = HttpRouteParameterRedactionMode.None;
                //    //options.RequestPathLoggingMode = OutgoingPathLoggingMode.Structured;
                //    //options.RequestPathParameterRedactionMode = HttpRouteParameterRedactionMode.Loose;

                //    options.LogBody = true;
                //    ////options.LogContentHeaders = true;
                //    //options.LogRequestStart = true;
                //});

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>().AddSerilog(Log.Logger, true);

            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
            SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());


            Padrao.ConnectionString = configuration.GetConnectionString("AuditoriaContext");
            // Npgsql logging is handled differently - remove MySQL-specific logging
            // MySqlConnectorLogManager.Provider = new SerilogLoggerProvider();

            try
            {
                // new ParametrosRepository().CarregarPadroes();

                var logger = serviceProvider.GetService<ILogger<Program>>();
                DapperExtensions.SetPolicies(new SqlResiliencePolicyFactory(logger).GetSqlResiliencePolicies());

                logger.LogInformation("Iniciando Importação");

                //                var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
                //                await dbContext.Database.ExecuteSqlRawAsync(@"
                //INSERT INTO temp.cl_deputado_de_para (id, nome, id_estado)
                //SELECT d.id, d.nome_parlamentar, d.id_estado 
                //FROM assembleias.cl_deputado d
                //LEFT JOIN temp.cl_deputado_de_para dp ON dp.nome ILIKE d.nome_parlamentar AND dp.id_estado = d.id_estado
                //WHERE dp.id IS NULL
                //AND d.nome_civil IS NOT NULL
                //ON CONFLICT DO NOTHING;

                //INSERT INTO temp.cl_deputado_de_para (id, nome, id_estado)
                //SELECT d.id, d.nome_civil, d.id_estado 
                //FROM assembleias.cl_deputado d
                //LEFT JOIN temp.cl_deputado_de_para dp ON dp.nome ILIKE d.nome_civil AND dp.id_estado = d.id_estado
                //WHERE dp.id IS NULL
                //AND d.nome_civil IS NOT NULL
                //ON CONFLICT DO NOTHING;");

                //var crawler = new SeleniumScraper(serviceProvider);
                //crawler.BaixarArquivosParana();
                //crawler.BaixarArquivosPiaui();

                var types = new Type[]
                {
                    //typeof(SenadoFederal.Senado), // csv
                    //typeof(CamaraFederal.Camara), // csv
                    ////typeof(ImportacaoAcre), // Portal sem dados detalhados por parlamentar! <<<<<< ------------------------------------------------------------------ >>>>>>> sem dados detalhados por parlamentar
                    //typeof(ImportacaoAlagoas), // Dados em PDF scaneado e de baixa qualidade!
                    //typeof(ImportacaoAmapa), // crawler mensal/deputado (Apenas BR)
                    //typeof(ImportacaoAmazonas), // crawler mensal/deputado (Apenas BR)
                    //typeof(ImportacaoBahia), // crawler anual
                    //typeof(ImportacaoCeara), // csv mensal
                    //typeof(ImportacaoDistritoFederal), // xlsx  (Apenas BR)
                    //typeof(ImportacaoEspiritoSanto),  // crawler mensal/deputado (Apenas BR)
                    //typeof(ImportacaoGoias), // crawler mensal/deputado
                    //typeof(ImportacaoMaranhao), // Valores mensais por categoria
                    ////typeof(ImportacaoMatoGrosso), // <<<<<< ------------------------------------------------------------------ >>>>>>> sem dados detalhados por parlamentar
                    //typeof(ImportacaoMatoGrossoDoSul), // crawler anual
                    //typeof(ImportacaoMinasGerais), // xml api mensal/deputado (Apenas BR) 
                    //typeof(ImportacaoPara), // json api anual
                    //typeof(ImportacaoParaiba), // arquivo ods mensal/deputado
                    //typeof(ImportacaoParana), // json api mensal/deputado <<<<<< ------------------------------------------------------------------ >>>>>>> capcha
                    //typeof(ImportacaoPernambuco), // json api mensal/deputado
                    //typeof(ImportacaoPiaui), // csv por legislatura <<<<<< ------------------------------------------------------------------ >>>>>>> (download manual/Selenium)
                    //typeof(ImportacaoRioDeJaneiro), // json api mensal/deputado
                    //typeof(ImportacaoRioGrandeDoNorte), // crawler & pdf mensal/deputado
                    //typeof(ImportacaoRioGrandeDoSul), // crawler mensal/deputado (Apenas BR)
                    //typeof(ImportacaoRondonia), // crawler mensal/deputado
                    //typeof(ImportacaoRoraima), // crawler & odt mensal/deputado
                    //typeof(ImportacaoSantaCatarina), // csv anual
                    //typeof(ImportacaoSaoPaulo), // xml anual
                    //typeof(ImportacaoSergipe), // crawler & pdf mensal/deputado
                    //typeof(ImportacaoTocantins), // crawler & pdf mensal/deputado
                };

                var tasks = new List<Task>();
                foreach (var type in types)
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        var estado = type.Name.Replace("Importador", "");
                        using (logger.BeginScope(new Dictionary<string, object> { ["Code"] = Utils.GetStateCode(estado), ["Estado"] = estado, ["ProcessIdentifier"] = Guid.NewGuid().ToString() }))
                        {
                            logger.LogInformation("Iniciando importação do(a) {Estado}.", estado);
                            var watch = System.Diagnostics.Stopwatch.StartNew();

                            var importador = (ImportadorBase)serviceProvider.GetService(type);
                            importador.ImportarCompleto().Wait();

                            watch.Stop();
                            logger.LogInformation("Processamento do(a) {Estado} finalizado em {TimeElapsed:c}", type.Name, watch.Elapsed);
                        }
                    }));
                }
                Task.WaitAll(tasks.ToArray());

                //var importadorBase = serviceProvider.GetService<ImportadorDespesasBase>();
                //importadorBase.AtualizaCampeoesGastos();
                //importadorBase.AtualizaResumoMensal();
                //var importador = serviceProvider.GetService<ImportadorDespesasCamaraFederal>();

                //var mesAtual = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
                //importador.ColetaDadosDeputados();
                //importador.AtualizaParlamentarValores();
                //importador.ColetaRemuneracaoSecretarios();


                //var importador = serviceProvider.GetService<ImportadorDespesasSenado>();
                //var mesAtual = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
                //var mesConsulta = new DateTime(2024, 07, 01);

                //do
                //{
                //    importador.ImportarRemuneracao(mesConsulta.Year, mesConsulta.Month);

                //    mesConsulta = mesConsulta.AddMonths(1);
                //} while (mesConsulta < mesAtual);


                //var cand = new Candidatos();
                //cand.ImportarCandidatos(@"C:\\temp\consulta_cand_2018_BRASIL.csv");
                //cand.ImportarDespesasPagas(@"C:\\temp\despesas_pagas_candidatos_2018_BRASIL.csv");
                //cand.ImportarDespesasContratadas(@"C:\\temp\despesas_contratadas_candidatos_2018_BRASIL.csv");
                //cand.ImportarReceitas(@"C:\\temp\receitas_candidatos_2018_BRASIL.csv");
                //cand.ImportarReceitasDoadorOriginario(@"C:\\temp\receitas_candidatos_doador_originario_2018_BRASIL.csv");

                var objFornecedor = serviceProvider.GetService<Fornecedores.ImportacaoFornecedor>();
                //objFornecedor.ConsultarDadosCNPJ().Wait();
                objFornecedor.ConsultarDadosCNPJ(somenteNovos: false).Wait();
            }
            catch (Exception ex)
            {
                // Log.Logger will likely be internal type "Serilog.Core.Pipeline.SilentLogger".
                if (Log.Logger == null || Log.Logger.GetType().Name == "SilentLogger")
                {
                    // Loading configuration or Serilog failed.
                    Log.Logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .CreateLogger();
                }

                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }

            Console.WriteLine("Concluido! Tecle [ENTER] para sair.");
            Console.ReadKey();
            return Task.CompletedTask;
        }

       

        //private static void ImportarPartidos()
        //{
        //    // (outra fonnte) https://legis.senado.leg.br/dadosabertos/senador/partidos
        //    var file = @"C:\Users\Lenovo\Downloads\convertcsv.csv";

        //    var factory = new AppDbContext(null, Padrao.ConnectionString);
        //    using (var dbContext = factory.CreateDbContext())
        //    {
        //        using (var reader = new StreamReader(file, Encoding.GetEncoding("UTF-8")))
        //        {
        //            using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")))
        //            {
        //                using (var client = new System.Net.Http.HttpClient())
        //                {
        //                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; OPS_bot/1.0; +https://ops.org.br)");

        //                    while (csv.Read())
        //                    {
        //                        if (csv[2] == "LOGO") continue;

        //                        if (csv[2] != "")
        //                        {
        //                            try
        //                            {
        //                                MatchCollection m1 = Regex.Matches(csv[2], @"<a\s+(?:[^>]*?\s+)?href=""([^""]*)""", RegexOptions.Singleline);
        //                                if (m1.Count > 0)
        //                                {
        //                                    var link = m1[0].Groups[1].Value;

        //                                    var arquivo = @"C:\ProjetosVanderlei\operacao-politica-supervisionada\OPS\wwwroot\partidos\" + csv[3].ToLower() + ".png";
        //                                    if (!File.Exists(arquivo))
        //                                        client.DownloadFile(link, arquivo).Wait();
        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                Console.WriteLine(ex.Message);
        //                            }
        //                        }

        //                        var parameters = new[]
        //                        {
        //                            ("@legenda", csv[0] != "-" ? (object)csv[0] : null),
        //                            ("@sigla", csv[2] != "??" ? (object)csv[2] : null),
        //                            ("@nome", csv[3]),
        //                            ("@sede", csv[4] != "??" ? (object)csv[4] : null),
        //                            ("@fundacao", AjustarData(csv[5])),
        //                            ("@registro_solicitacao", AjustarData(csv[6])),
        //                            ("@registro_provisorio", AjustarData(csv[7])),
        //                            ("@registro_definitivo", AjustarData(csv[8])),
        //                            ("@extincao", AjustarData(csv[9])),
        //                            ("@motivo", csv[10])
        //                        };

        //                        dbContext.Database.ExecuteSqlRaw(@"
        //                            INSERT INTO partido_todos (
        //                                legenda, sigla, nome, sede, fundacao, registro_solicitacao, registro_provisorio, registro_definitivo, extincao, motivo
        //                            ) VALUES (
        //                                @legenda, @sigla, @nome, @sede, @fundacao, @registro_solicitacao, @registro_provisorio, @registro_definitivo, @extincao, @motivo
        //                            )", parameters);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        private static async Task BaixarNotas()
        {
            var sql = @"SELECT l.id, d.id_deputado, l.ano, l.id_documento
FROM camara.cf_despesa l
JOIN cf_deputado d ON d.id = l.id_cf_deputado
WHERE l.ano >= 2023
AND l.id_cf_despesa_tipo = 120
AND l.tipo_link = 1
AND l.id > 10501020
ORDER BY 1";

            using (var client = new System.Net.Http.HttpClient())
            using (var connection = new NpgsqlConnection(Padrao.ConnectionString))
            {
                await connection.OpenAsync();
                var results = await connection.QueryAsync(sql);
                foreach (var item in results)
                {
                    try
                    {
                        var url = $"https://www.camara.leg.br/cota-parlamentar/documentos/publ/{item.id_deputado}/{item.ano}/{item.id_documento}.pdf";
                        var arquivo = $@"C:\temp\NotasFiscais\{item.id_deputado}-{item.ano}-{item.id_documento}.pdf";

                        if (!File.Exists(arquivo))
                        {
                            Console.WriteLine($"Baixando {item.id} {url}...");
                            await client.DownloadFile(url, arquivo);
                            await Task.Delay(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        private static DateTime? AjustarData(string d)
        {
            if (!d.Contains(" ??/??/?? ") && d != "ATUAL" && d != " - ")
            {
                d = d.Replace("??", "01");
                if (d.Length == 10)
                    return DateTime.Parse(d);
                else
                    return DateTime.ParseExact(d, "dd/MM/yy", CultureInfo.InvariantCulture);
            }

            return null;
        }

        private static async Task AjustarFornecedores()
        {
            var sql = @"SELECT id, cnpj_cpf from fornecedor f
WHERE LENGTH(f.cnpj_cpf) = 14
AND f.cnpj_cpf ILIKE '***%'";

            using (var connection = new NpgsqlConnection(Padrao.ConnectionString))
            {
                await connection.OpenAsync();
                var results = await connection.QueryAsync(sql);
                foreach (var item in results)
                {
                    try
                    {
                        var cnpjIncorreto = item.cnpj_cpf.ToString();
                        var sqlFind = $@"SELECT id_fornecedor, cnpj from fornecedor_info WHERE cnpj ILIKE '{cnpjIncorreto.Replace("*", "_")}'";
                        var results1 = await connection.QueryAsync(sqlFind);

                        if (results1.Count() == 1)
                        {
                            var idFornecodorIncorreto = item.id.ToString();
                            var idFornecodorCorreto = results1.First().id_fornecedor.ToString();


                            var cnpjCorreto = results1.First().cnpj.ToString();

                            Console.WriteLine($"Corrigindo Fornecedor {idFornecodorIncorreto} -> {idFornecodorCorreto} CNPJ: {cnpjIncorreto} -> {cnpjCorreto}");

                            var sqlUpdate = $@"
UPDATE cl_despesa SET id_fornecedor = {idFornecodorCorreto}  WHERE id_fornecedor = {idFornecodorIncorreto};
UPDATE cf_despesa SET id_fornecedor = {idFornecodorCorreto}  WHERE id_fornecedor = {idFornecodorIncorreto};
UPDATE sf_despesa SET id_fornecedor = {idFornecodorCorreto}  WHERE id_fornecedor = {idFornecodorIncorreto};

UPDATE fornecedor SET 
    valor_total_ceap_camara = NULL,
    valor_total_ceap_senado = NULL,
    valor_total_ceap_assembleias = NULL
WHERE id = {idFornecodorIncorreto};

UPDATE fornecedor_de_para SET 
    id_fornecedor_correto = {idFornecodorCorreto},
    cnpj_correto = '{cnpjCorreto}'
WHERE cnpj_incorreto = '{cnpjIncorreto}';

";
                            await connection.ExecuteAsync(sqlUpdate);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}