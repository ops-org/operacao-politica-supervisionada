using System.Configuration;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using Dapper;
using DDDN.OdtToHtml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using OfficeOpenXml;
using OPS.Core;
using OPS.Core.Utilities;
using OPS.Importador.Utilities;
using OPS.Infraestrutura;
using OPS.Infraestrutura.Interceptors;
using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace OPS.Importador
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            ExcelPackage.License.SetNonCommercialOrganization("OPS: Operação Política Supervisionada");

            SqlMapper.AddTypeHandler(typeof(DateTime), new NpgsqlDateTimeHandler());
            SqlMapper.AddTypeHandler(typeof(DateTime?), new NpgsqlDateTimeHandler());

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
            services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace));
            services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configuration);
            services.AddTransient<IDbConnection>(_ => new NpgsqlConnection(configuration["ConnectionStrings:AuditoriaContext"]));

            services.AddScoped<SenadoFederal.Senado>();
            services.AddScoped<CamaraFederal.Camara>();

            services.AddScoped<Assembleias.Estados.Acre.ImportacaoAcre>();
            services.AddScoped<Assembleias.Estados.Alagoas.ImportacaoAlagoas>();
            services.AddScoped<Assembleias.Estados.Amapa.ImportacaoAmapa>();
            services.AddScoped<Assembleias.Estados.Amazonas.ImportacaoAmazonas>();
            services.AddScoped<Assembleias.Estados.Bahia.ImportacaoBahia>();
            services.AddScoped<Assembleias.Estados.Ceara.ImportacaoCeara>();
            services.AddScoped<Assembleias.Estados.DistritoFederal.ImportacaoDistritoFederal>();
            services.AddScoped<Assembleias.Estados.EspiritoSanto.ImportacaoEspiritoSanto>();
            services.AddScoped<Assembleias.Estados.Goias.ImportacaoGoias>();
            services.AddScoped<Assembleias.Estados.Maranhao.ImportacaoMaranhao>();
            services.AddScoped<Assembleias.Estados.MatoGrosso.ImportacaoMatoGrosso>();
            services.AddScoped<Assembleias.Estados.MatoGrossoDoSul.ImportacaoMatoGrossoDoSul>();
            services.AddScoped<Assembleias.Estados.MinasGerais.ImportacaoMinasGerais>();
            services.AddScoped<Assembleias.Estados.Para.ImportacaoPara>();
            services.AddScoped<Assembleias.Estados.Paraiba.ImportacaoParaiba>();
            services.AddScoped<Assembleias.Estados.Parana.ImportacaoParana>();
            services.AddScoped<Assembleias.Estados.Pernambuco.ImportacaoPernambuco>();
            services.AddScoped<Assembleias.Estados.Piaui.ImportacaoPiaui>();
            services.AddScoped<Assembleias.Estados.RioDeJaneiro.ImportacaoRioDeJaneiro>();
            services.AddScoped<Assembleias.Estados.RioGrandeDoNorte.ImportacaoRioGrandeDoNorte>();
            services.AddScoped<Assembleias.Estados.RioGrandeDoSul.ImportacaoRioGrandeDoSul>();
            services.AddScoped<Assembleias.Estados.Rondonia.ImportacaoRondonia>();
            services.AddScoped<Assembleias.Estados.Roraima.ImportacaoRoraima>();
            services.AddScoped<Assembleias.Estados.SantaCatarina.ImportacaoSantaCatarina>();
            services.AddScoped<Assembleias.Estados.SaoPaulo.ImportacaoSaoPaulo>();
            services.AddScoped<Assembleias.Estados.Sergipe.ImportacaoSergipe>();
            services.AddScoped<Assembleias.Estados.Tocantins.ImportacaoTocantins>();

            services.AddScoped<CamaraFederal.ImportadorDespesasCamaraFederal>();
            services.AddScoped<SenadoFederal.ImportadorDespesasSenado>();

            //services.AddScoped<Presidencia>();

            services.AddScoped<Fornecedores.ImportacaoFornecedor>();
            services.AddScoped<HttpLogger>();
            services.AddDbContext<AppDbContext>(options => options
                   .UseNpgsql(configuration.GetConnectionString("AuditoriaContext")));

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
                });

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

            // Initialize database extensions (unaccent for PostgreSQL)
            var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
            try
            {
                dbContext.InitializeDatabaseExtensions();
            }
            catch (Exception ex)
            {
                Log.Warning("Could not initialize database extensions: {Message}", ex.Message);
            }

            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
            SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());


            Padrao.ConnectionString = configuration.GetConnectionString("AuditoriaContext");
            // Npgsql logging is handled differently - remove MySQL-specific logging
            // MySqlConnectorLogManager.Provider = new SerilogLoggerProvider();

            try
            {
                // new ParametrosRepository().CarregarPadroes();

                var logger = serviceProvider.GetService<ILogger<Program>>();
                DapperExtensions.SetPolicies(new SqlResiliencePolicyFactory(logger, configuration).GetSqlResiliencePolicies());

                logger.LogInformation("Iniciando Importação");

//                var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
//                    await dbContext.Database.ExecuteSqlRawAsync(@"
//INSERT INTO temp.cl_deputado_de_para (id, nome, id_estado)
//SELECT id, nome_parlamentar, id_estado FROM assembleias.cl_deputado
//WHERE nome_parlamentar IS NOT NULL
//ON CONFLICT DO NOTHING;

//INSERT INTO temp.cl_deputado_de_para (id, nome, id_estado)
//SELECT id, nome_civil, id_estado FROM assembleias.cl_deputado
//WHERE nome_civil IS NOT NULL
//ON CONFLICT DO NOTHING;");

                //var crawler = new SeleniumScraper(serviceProvider);
                //crawler.BaixarArquivosParana(DateTime.Today.Year);
                //crawler.BaixarArquivosPiaui();

                var types = new Type[]
                {
                    typeof(SenadoFederal.Senado), // csv
                    //typeof(CamaraFederal.Camara), // csv
                    ////typeof(Assembleias.Estados.Acre.ImportacaoAcre), // Portal sem dados detalhados por parlamentar! <<<<<< ------------------------------------------------------------------ >>>>>>> sem dados detalhados por parlamentar
                    //typeof(Assembleias.Estados.Alagoas.ImportacaoAlagoas), // Dados em PDF scaneado e de baixa qualidade!
                    //typeof(Assembleias.Estados.Amapa.ImportacaoAmapa), // crawler mensal/deputado (Apenas BR)
                    //typeof(Assembleias.Estados.Amazonas.ImportacaoAmazonas), // crawler mensal/deputado (Apenas BR)
                    //typeof(Assembleias.Estados.Bahia.ImportacaoBahia), // crawler anual
                    //typeof(Assembleias.Estados.Ceara.ImportacaoCeara), // csv mensal
                    //typeof(Assembleias.Estados.DistritoFederal.ImportacaoDistritoFederal), // xlsx  (Apenas BR)
                    //typeof(Assembleias.Estados.EspiritoSanto.ImportacaoEspiritoSanto),  // crawler mensal/deputado (Apenas BR)
                    //typeof(Assembleias.Estados.Goias.ImportacaoGoias), // crawler mensal/deputado
                    //typeof(Assembleias.Estados.Maranhao.ImportacaoMaranhao), // Valores mensais por categoria
                    ////typeof(Assembleias.Estados.MatoGrosso.ImportacaoMatoGrosso), // <<<<<< ------------------------------------------------------------------ >>>>>>> sem dados detalhados por parlamentar
                    //typeof(Assembleias.Estados.MatoGrossoDoSul.ImportacaoMatoGrossoDoSul), // crawler anual
                    //typeof(Assembleias.Estados.MinasGerais.ImportacaoMinasGerais), // xml api mensal/deputado (Apenas BR) 
                    //typeof(Assembleias.Estados.Para.ImportacaoPara), // json api anual
                    //typeof(Assembleias.Estados.Paraiba.ImportacaoParaiba), // arquivo ods mensal/deputado
                    //typeof(Assembleias.Estados.Parana.ImportacaoParana), // json api mensal/deputado <<<<<< ------------------------------------------------------------------ >>>>>>> capcha
                    //typeof(Assembleias.Estados.Pernambuco.ImportacaoPernambuco), // json api mensal/deputado
                    //typeof(Assembleias.Estados.Piaui.ImportacaoPiaui), // csv por legislatura <<<<<< ------------------------------------------------------------------ >>>>>>> (download manual/Selenium)
                    //typeof(Assembleias.Estados.RioDeJaneiro.ImportacaoRioDeJaneiro), // json api mensal/deputado
                    //typeof(Assembleias.Estados.RioGrandeDoNorte.ImportacaoRioGrandeDoNorte), // crawler & pdf mensal/deputado
                    //typeof(Assembleias.Estados.RioGrandeDoSul.ImportacaoRioGrandeDoSul), // crawler mensal/deputado (Apenas BR)
                    //typeof(Assembleias.Estados.Rondonia.ImportacaoRondonia), // crawler mensal/deputado
                    //typeof(Assembleias.Estados.Roraima.ImportacaoRoraima), // crawler & odt mensal/deputado
                    //typeof(Assembleias.Estados.SantaCatarina.ImportacaoSantaCatarina), // csv anual
                    //typeof(Assembleias.Estados.SaoPaulo.ImportacaoSaoPaulo), // xml anual
                    //typeof(Assembleias.Estados.Sergipe.ImportacaoSergipe), // crawler & pdf mensal/deputado
                    //typeof(Assembleias.Estados.Tocantins.ImportacaoTocantins), // crawler & pdf mensal/deputado
                };

                var tasks = new List<Task>();
                foreach (var type in types)
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {

                        using (logger.BeginScope(new Dictionary<string, object> { ["Estado"] = type.Name, ["ProcessIdentifier"] = Guid.NewGuid().ToString() }))
                        {
                            var watch = System.Diagnostics.Stopwatch.StartNew();

                            var importador = (Assembleias.Comum.ImportadorBase)serviceProvider.GetService(type);
                            importador.ImportarCompleto();

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
                objFornecedor.ConsultarDadosCNPJ().Wait();
                //objFornecedor.ConsultarDadosCNPJ(somenteNovos: false).Wait();
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