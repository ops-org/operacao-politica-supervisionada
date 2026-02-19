using System;
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
    internal static class ConfigureApp
    {
        public static void SetupEnvironment()
        {
            ExcelPackage.License.SetNonCommercialOrganization("OPS: Operação Política Supervisionada");

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            Serilog.Debugging.SelfLog.Enable(Console.Error);

            CultureInfo ci = new CultureInfo("pt-BR");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
            SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
        }

        public static IConfiguration BuildConfiguration()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static void SetupLogging(IConfiguration configuration)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("Identifier", Guid.NewGuid().ToString())
                .Enrich.With<InvocationContextEnricher>();

            Log.Logger = loggerConfiguration.CreateLogger();
        }

        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
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

            services.AddScoped<CamaraFederal.ImportacaoCamaraFederal>();
            services.AddScoped<CamaraFederal.ImportadorDespesasCamaraFederal>();
            services.AddScoped<SenadoFederal.ImportadorDespesasSenado>();

            //services.AddScoped<Presidencia>();

            services.AddScoped<Fornecedores.ImportacaoFornecedor>();
            services.AddScoped<FileManager>();
            services.AddScoped<IndiceInflacaoImportador>();
            services.AddScoped<HttpLogger>();
            services.AddDbContext<AppDbContext>(options => options
                   .UseNpgsql(configuration.GetConnectionString("AuditoriaContext"), sqlOptions => sqlOptions.CommandTimeout(120)), ServiceLifetime.Transient);

            //services.AddRedaction();

            ConfigureHttpClients(services);
        }

        private static void ConfigureHttpClients(IServiceCollection services)
        {
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
                .WaitAndRetryAsync(5, retryAttempt =>
                    {
                        // Aggressive exponential backoff: 3^(retryAttempt-1) seconds => 1,3,9,27,81
                        var baseDelaySeconds = Math.Pow(3, retryAttempt - 1);
                        // Jitter up to 100% of the base delay to avoid thundering herd
                        var jitterSeconds = System.Random.Shared.NextDouble() * baseDelaySeconds;

                        return TimeSpan.FromSeconds(baseDelaySeconds + jitterSeconds);
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
        }
    }
}
