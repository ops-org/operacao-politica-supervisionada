using System.Globalization;
using System.Net;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                   .UseNpgsql(configuration.GetConnectionString("AuditoriaContext"), sqlOptions => sqlOptions.CommandTimeout(120)), ServiceLifetime.Transient);

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
                    handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => { return true; };

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


            try
            {
                var logger = serviceProvider.GetService<ILogger<Program>>();
                //DapperExtensions.SetPolicies(new SqlResiliencePolicyFactory(logger).GetSqlResiliencePolicies());

                logger.LogInformation("Iniciando Importação");

                var dbContext = serviceProvider.GetRequiredService<AppDbContext>();

                dbContext.Database.ExecuteSqlRawAsync(@"
                    INSERT INTO temp.cl_deputado_de_para (id, nome, id_estado)
                    SELECT d.id, d.nome_parlamentar, d.id_estado 
                    FROM assembleias.cl_deputado d
                    LEFT JOIN temp.cl_deputado_de_para dp ON dp.nome ILIKE d.nome_parlamentar AND dp.id_estado = d.id_estado
                    WHERE dp.id IS NULL
                    AND d.nome_civil IS NOT NULL
                    ON CONFLICT DO NOTHING;

                    INSERT INTO temp.cl_deputado_de_para (id, nome, id_estado)
                    SELECT d.id, d.nome_civil, d.id_estado 
                    FROM assembleias.cl_deputado d
                    LEFT JOIN temp.cl_deputado_de_para dp ON dp.nome ILIKE d.nome_civil AND dp.id_estado = d.id_estado
                    WHERE dp.id IS NULL
                    AND d.nome_civil IS NOT NULL
                    ON CONFLICT DO NOTHING;").GetAwaiter().GetResult();

                var crawler = new SeleniumScraper(serviceProvider);
                crawler.BaixarArquivosParana();
                crawler.BaixarArquivosPiaui();

                var types = new Type[]
                    {
                        typeof(SenadoFederal.Senado), // csv
                        typeof(CamaraFederal.Camara), // csv
                        //typeof(ImportacaoAcre), // Portal sem dados detalhados por parlamentar! <<<<<< ------------------------------------------------------------------ >>>>>>> sem dados detalhados por parlamentar
                        typeof(ImportacaoAlagoas), // Dados em PDF scaneado e de baixa qualidade!
                        typeof(ImportacaoAmapa), // crawler mensal/deputado (Apenas BR)
                        typeof(ImportacaoAmazonas), // crawler mensal/deputado (Apenas BR)
                        typeof(ImportacaoBahia), // crawler anual
                        typeof(ImportacaoCeara), // crawler mensal
                        typeof(ImportacaoDistritoFederal), // xlsx  (Apenas BR)
                        typeof(ImportacaoEspiritoSanto),  // crawler mensal/deputado (Apenas BR)
                        typeof(ImportacaoGoias), // crawler mensal/deputado
                        typeof(ImportacaoMaranhao), // Valores mensais por categoria
                        //typeof(ImportacaoMatoGrosso), // <<<<<< ------------------------------------------------------------------ >>>>>>> sem dados detalhados por parlamentar
                        typeof(ImportacaoMatoGrossoDoSul), // crawler anual
                        typeof(ImportacaoMinasGerais), // xml api mensal/deputado (Apenas BR) 
                        typeof(ImportacaoPara), // json api anual
                        typeof(ImportacaoParaiba), // arquivo ods mensal/deputado
                        typeof(ImportacaoParana), // json api mensal/deputado <<<<<< ------------------------------------------------------------------ >>>>>>> capcha
                        typeof(ImportacaoPernambuco), // json api mensal/deputado
                        typeof(ImportacaoPiaui), // csv por legislatura <<<<<< ------------------------------------------------------------------ >>>>>>> (download manual/Selenium) TODO: Buscar empresa/cnpj do PDF com crawler e OCR
                        typeof(ImportacaoRioDeJaneiro), // json api mensal/deputado
                        typeof(ImportacaoRioGrandeDoNorte), // crawler & pdf mensal/deputado
                        typeof(ImportacaoRioGrandeDoSul), // crawler mensal/deputado (Apenas BR)
                        typeof(ImportacaoRondonia), // crawler mensal/deputado
                        typeof(ImportacaoRoraima), // crawler & odt mensal/deputado
                        typeof(ImportacaoSantaCatarina), // csv anual
                        typeof(ImportacaoSaoPaulo), // xml anual
                        typeof(ImportacaoSergipe), // crawler & pdf mensal/deputado
                        typeof(ImportacaoTocantins), // crawler & pdf mensal/deputado
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
            return Task.CompletedTask;
        }
    }
}