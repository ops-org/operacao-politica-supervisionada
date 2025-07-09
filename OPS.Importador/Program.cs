using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using MySqlConnector.Logging;
using OPS.Core;
using OPS.Core.Repository;
using OPS.Core.Utilities;
using OPS.Importador.ALE;
using OPS.Importador.ALE.Comum;
using OPS.Importador.Utilities;
using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace OPS.Importador
{
    internal class Program
    {
        public static void Main(string[] args)
        {
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
            services.AddTransient<IDbConnection>(_ => new MySqlConnection(configuration["ConnectionStrings:AuditoriaContext"]));

            services.AddScoped<Senado>();
            services.AddScoped<CamaraFederal>();

            services.AddScoped<Acre>();
            services.AddScoped<Alagoas>();
            services.AddScoped<Amapa>();
            services.AddScoped<Amazonas>();
            services.AddScoped<Bahia>();
            services.AddScoped<Ceara>();
            services.AddScoped<DistritoFederal>();
            services.AddScoped<EspiritoSanto>();
            services.AddScoped<Goias>();
            services.AddScoped<Maranhao>();
            services.AddScoped<MatoGrosso>();
            services.AddScoped<MatoGrossoDoSul>();
            services.AddScoped<MinasGerais>();
            services.AddScoped<Para>();
            services.AddScoped<Paraiba>();
            services.AddScoped<Parana>();
            services.AddScoped<Pernambuco>();
            services.AddScoped<Piaui>();
            services.AddScoped<RioDeJaneiro>();
            services.AddScoped<RioGrandeDoNorte>();
            services.AddScoped<RioGrandeDoSul>();
            services.AddScoped<Rondonia>();
            services.AddScoped<Roraima>();
            services.AddScoped<SantaCatarina>();
            services.AddScoped<SaoPaulo>();
            services.AddScoped<Sergipe>();
            services.AddScoped<Tocantins>();

            services.AddScoped<ImportadorDespesasCamaraFederal>();
            services.AddScoped<ImportadorDespesasSenado>();

            //services.AddScoped<Presidencia>();

            services.AddScoped<Fornecedor>();
            services.AddScoped<HttpLogger>();
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

            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
            SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());


            Padrao.ConnectionString = configuration.GetConnectionString("AuditoriaContext");
            MySqlConnectorLogManager.Provider = new SerilogLoggerProvider();

            try
            {
                new ParametrosRepository().CarregarPadroes();

                var logger = serviceProvider.GetService<ILogger<Program>>();
                DapperExtensions.SetPolicies(new SqlResiliencePolicyFactory(logger, configuration).GetSqlResiliencePolicies());

                logger.LogInformation("Iniciando Importação");

                var types = new Type[]
                {
                    typeof(Senado), // csv
                    typeof(CamaraFederal), // csv
                    //typeof(Acre), // Portal sem dados detalhados por parlamentar!
                    typeof(Alagoas), // Dados em PDF scaneado e de baixa qualidade!
                    typeof(Amapa), // crawler mensal/deputado (Apenas BR)
                    typeof(Amazonas), // crawler mensal/deputado (Apenas BR)
                    typeof(Bahia), // crawler anual
                    typeof(Ceara), // csv mensal
                    typeof(DistritoFederal), // xlsx  (Apenas BR)
                    typeof(EspiritoSanto),  // crawler mensal/deputado (Apenas BR)
                    typeof(Goias), // crawler mensal/deputado
                    typeof(Maranhao), // Valores mensais por categoria
                    //typeof(MatoGrosso),
                    typeof(MatoGrossoDoSul), // crawler anual
                    typeof(MinasGerais), // xml api mensal/deputado (Apenas BR)
                    typeof(Para), // json api anual
                    typeof(Paraiba), // arquivo ods mensal/deputado
                    //typeof(Parana), // json api mensal/deputado <-------- capcha
                    typeof(Pernambuco), // json api mensal/deputado
                    typeof(Piaui), // csv por legislatura <<<<<< ------------------------------------------------------------------ >>>>>>> (download manual)
                    typeof(RioDeJaneiro), // json api mensal/deputado
                    typeof(RioGrandeDoNorte), // crawler & pdf mensal/deputado
                    typeof(RioGrandeDoSul), // crawler mensal/deputado (Apenas BR)
                    typeof(Rondonia), // crawler mensal/deputado
                    typeof(Roraima), // crawler & odt mensal/deputado
                    typeof(SantaCatarina), // csv anual
                    typeof(SaoPaulo), // xml anual
                    typeof(Sergipe), // crawler & pdf mensal/deputado
                    typeof(Tocantins), // crawler & pdf mensal/deputado
                };

                var tasks = new List<Task>();
                foreach (var type in types)
                {
                    tasks.Add(Task.Factory.StartNew(() =>
                    {

                        using (logger.BeginScope(new Dictionary<string, object> { ["Estado"] = type.Name, ["ProcessIdentifier"] = Guid.NewGuid().ToString() }))
                        {
                            var watch = System.Diagnostics.Stopwatch.StartNew();

                            var importador = (ImportadorBase)serviceProvider.GetService(type);
                            importador.ImportarCompleto();

                            watch.Stop();
                            logger.LogInformation("Processamento do(a) {Estado} finalizado em {TimeElapsed:c}", type.Name, watch.Elapsed);
                        }
                    }));
                }
                Task.WaitAll(tasks.ToArray());

                //var importador = serviceProvider.GetService<ImportadorDespesasCamaraFederal>();

                //var mesAtual = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
                //var mesConsulta = new DateTime(2023, 02, 01);

                //do
                //{
                //    importador.ConsultaRemuneracao(mesConsulta.Year, mesConsulta.Month);

                //    mesConsulta = mesConsulta.AddMonths(1);
                //} while (mesConsulta < mesAtual);

                //importador.ColetaDadosDeputados();
                //importador.ColetaRemuneracaoSecretarios();


                //var importador = serviceProvider.GetService<ImportadorDespesasSenado>();
                //var mesAtual = DateTime.Today.AddDays(-(DateTime.Today.Day - 1));
                //var mesConsulta = new DateTime(2022, 09, 01);

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

                Fornecedor objFornecedor = serviceProvider.GetService<Fornecedor>();
                objFornecedor.ConsultarDadosCNPJ().Wait();
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

        private static void ImportarPartidos()
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");
            var sb = new StringBuilder();
            var file = @"C:\Users\Lenovo\Downloads\convertcsv.csv";

            int indice = 0;
            int Legenda = indice++;
            int Imagem = indice++;
            int Sigla = indice++;
            int Nome = indice++;
            int Sede = indice++;
            int Fundacao = indice++;
            int RegistroSolicitacao = indice++;
            int RegistroProvisorio = indice++;
            int RegistroDefinitivo = indice++;
            int Extincao = indice++;
            int Motivo = indice++;

            using (var banco = new AppDb())
            {
                using (var reader = new StreamReader(file, Encoding.GetEncoding("UTF-8")))
                {
                    using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR")))
                    {
                        //csv.Configuration.Delimiter = ",";

                        using (var client = new System.Net.Http.HttpClient())
                        {
                            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; OPS_bot/1.0; +https://ops.net.br)"); //Other

                            while (csv.Read())
                            {
                                if (csv[Imagem] == "LOGO") continue;

                                if (csv[Imagem] != "")
                                {
                                    try
                                    {
                                        MatchCollection m1 = Regex.Matches(csv[Imagem], @"<a\s+(?:[^>]*?\s+)?href=""([^""]*)""", RegexOptions.Singleline);
                                        if (m1.Count > 0)
                                        {
                                            var link = m1[0].Groups[1].Value;

                                            var arquivo = @"C:\ProjetosVanderlei\operacao-politica-supervisionada\OPS\wwwroot\partidos\" + csv[Sigla].ToLower() + ".png";
                                            if (!File.Exists(arquivo))
                                                client.DownloadFile(link, arquivo).Wait();
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                    }
                                }

                                banco.AddParameter("legenda", csv[Legenda] != "-" ? csv[Legenda] : null);
                                banco.AddParameter("sigla", csv[Sigla] != "??" ? csv[Sigla] : null);
                                banco.AddParameter("nome", csv[Nome]);
                                banco.AddParameter("sede", csv[Sede] != "??" ? csv[Sede] : null);
                                banco.AddParameter("fundacao", AjustarData(csv[Fundacao]));
                                banco.AddParameter("registro_solicitacao", AjustarData(csv[RegistroSolicitacao]));
                                banco.AddParameter("registro_provisorio", AjustarData(csv[RegistroProvisorio]));
                                banco.AddParameter("registro_definitivo", AjustarData(csv[RegistroDefinitivo]));
                                banco.AddParameter("extincao", AjustarData(csv[Extincao]));
                                banco.AddParameter("motivo", csv[Motivo]);

                                banco.ExecuteNonQuery(
                                    @"INSERT INTO partido_todos (
                                        legenda, sigla, nome, sede, fundacao, registro_solicitacao, registro_provisorio, registro_definitivo, extincao, motivo
                                    ) VALUES (
                                        @legenda, @sigla, @nome, @sede, @fundacao, @registro_solicitacao, @registro_provisorio, @registro_definitivo, @extincao, @motivo
                                    )");
                            }
                        }
                    }
                }
            }
        }

        private static DateTime? AjustarData(string d)
        {
            if (!d.Contains("??/??/??") && d != "ATUAL" && d != "-")
            {
                d = d.Replace("??", "01");
                if (d.Length == 10)
                    return DateTime.Parse(d);
                else
                    return DateTime.ParseExact(d, "dd/MM/yy", CultureInfo.InvariantCulture);
            }

            return null;
        }
    }
}