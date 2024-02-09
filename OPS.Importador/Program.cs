using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Io;
using CsvHelper;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using MySqlConnector.Logging;
using OPS.Core;
using OPS.Importador.ALE;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.Utilities;
using Serilog;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OPS.Importador
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.IsNullOrEmpty(environmentName)) environmentName = "Development";

            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();
            var configuration = builder.Build();

            var loggerConfiguration = new LoggerConfiguration()
              .ReadFrom.Configuration(configuration)
              .Enrich.FromLogContext();

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

            //services.AddScoped<Presidencia>();

            services.AddScoped<Fornecedor>();

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<Microsoft.Extensions.Logging.ILoggerFactory>().AddSerilog(Log.Logger, true);

            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
            SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());

            CultureInfo ci = new CultureInfo("pt-BR");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            Padrao.ConnectionString = configuration.GetConnectionString("AuditoriaContext");
            MySqlConnectorLogManager.Provider = new SerilogLoggerProvider();

            try
            {
                new Core.DAO.ParametrosRepository().CarregarPadroes();

                if (environmentName == "Production")
                {
                    ImportacaoDadosCompleto(serviceProvider, configuration).Wait();
                }
                else
                {

                    //var htmlRequester = new DefaultHttpRequester();
                    //htmlRequester.Headers["User-Agent"] = "Mozilla/5.0 (compatible; OPS_bot/1.0; +https://ops.net.br)";
                    //var handler = new DefaultHttpRequester { Timeout = TimeSpan.FromMinutes(5) };

                    //var configuration1 = AngleSharp.Configuration.Default
                    //   .With(htmlRequester)
                    //   .With(handler)
                    //   .WithDefaultLoader()
                    //   .WithDefaultCookies()
                    //   .WithCulture("pt-BR");
                    //var context = BrowsingContext.New(configuration1);

                    //for (int i = 0; i < 100; i++)
                    //{
                    //    var address = $"http://www.al.ap.gov.br/pagina.php?pg=exibir_parlamentar&iddeputado={i}";
                    //    var document = context.OpenAsyncAutoRetry(address).GetAwaiter().GetResult();
                    //    var nome = document.QuerySelector("h3.ls-title-3 strong")?.TextContent;

                    //    Console.WriteLine($"{i}: {nome}");
                    //}


                    var types = new Type[]
                    {
                        typeof(Senado), // csv
                        typeof(CamaraFederal), // csv

                        typeof(Acre),
                        typeof(Alagoas),
                        typeof(Amapa), // crawler
                        typeof(Amazonas), // -- Apenas BR
                        typeof(Bahia), // crawler // TESTAR
                        typeof(Ceara), // csv mensal
                        typeof(DistritoFederal), // xlsx  -- Apenas BR
                        typeof(EspiritoSanto),
                        typeof(Goias), // crawler
                        typeof(Maranhao),
                        typeof(MatoGrosso),
                        typeof(MatoGrossoDoSul), // crawler
                        typeof(MinasGerais), // xml api -- Apenas BR
                        typeof(Para),
                        typeof(Paraiba),
                        typeof(Parana), // rest api mensal // TIMEOUT na pagina 3
                        typeof(Pernambuco),
                        typeof(Piaui),
                        typeof(RioDeJaneiro),
                        typeof(RioGrandeDoNorte),
                        typeof(RioGrandeDoSul),
                        typeof(Rondonia),
                        typeof(Roraima),
                        typeof(SantaCatarina), // csv
                        typeof(SaoPaulo), // xml
                        typeof(Sergipe),
                        typeof(Tocantins),
                    };

                    foreach (var type in types)
                    {
                        Log.Warning("Dados do(a) {CasaLegislativa}", type.Name);

                        var importador = (ImportadorBase)serviceProvider.GetService(type);
                        importador.ImportarCompleto();
                    }

                    //var importador = serviceProvider.GetService<Presidencia>();
                    //importador.ImportarArquivoDespesas(0);

                    //objCamaraSantaCatarina.ImportarParlamentares();
                    //objSenado.ImportarCompleto();
                    //objCamara.ImportarCompleto();
                    //objCamara.ImportaPresencasDeputados();
                    //objCamaraDistritoFederal.ImportarCompleto();
                    //objCamaraSantaCatarina.ImportarCompleto();
                    //objCamara.AtualizaParlamentarValores();
                    //objCamara.ColetaDadosDeputados();
                    //objCamara.ColetaRemuneracaoSecretarios();

                    //objCamaraDistritoFederal.ImportarParlamentares();
                    //objCamaraSantaCatarina.ImportarParlamentares();
                    //objCamaraSaoPaulo.ImportarParlamentares();
                    //objCamaraPernambuco.ImportarParlamentares();
                    //objCamaraMinasGerais.ImportarParlamentares();
                    //objCamaraRioGrandeDoSul.ImportarParlamentares();
                    //objCamaraGoias.ImportarParlamentares();
                    //objCamaraBahia.ImportarParlamentares();

                    //var objCamaraParana = serviceProvider.GetService<CamaraParana>();
                    //objCamaraParana.ImportarArquivoDespesas(2019);
                    //objCamaraParana.ImportarArquivoDespesas(2020);
                    //objCamaraParana.ImportarArquivoDespesas(2021);
                    //objCamaraParana.ImportarArquivoDespesas(2022);


                    //for (int ano = 2013; ano <= 2022; ano++)
                    //{
                    //    Log.Information("Despesas de {Ano}", ano);
                    //    objCamaraDistritoFederal.ImportarArquivoDespesas(ano);
                    //}

                    //for (int ano = 2011; ano <= 2022; ano++)
                    //{
                    //    Log.Information("Despesas de {Ano}", ano);
                    //    objCamaraSantaCatarina.ImportarArquivoDespesas(ano);
                    //}

                    //for (int ano = 2010; ano <= 2022; ano++)
                    //{
                    //    Log.Information("Despesas de {Ano}", ano);
                    //    objCamaraSaoPaulo.ImportarArquivoDespesas(ano);
                    //}

                    //for (int ano = 2019; ano <= 2022; ano++)
                    //{
                    //    Log.Information("Despesas de {Ano}", ano);
                    //    objCamaraMatoGrossoDoSul.ImportarArquivoDespesas(ano);
                    //}

                    //for (int ano = 2020; ano <= 2022; ano++)
                    //{
                    //    Log.Information("Despesas de {Ano}", ano);
                    //    //objCamaraGoias.ImportarArquivoDespesas(ano);
                    //    objCamaraBahia.ImportarArquivoDespesas(ano);
                    //}

                    //for (int ano = 2021; ano <= 2022; ano++)
                    //{
                    //    Log.Information("Despesas de {Ano}", ano);
                    //    objCamaraCeara.ImportarArquivoDespesas(ano);
                    //}

                    //var cand = new Candidatos();
                    //cand.ImportarCandidatos(@"C:\\temp\consulta_cand_2018_BRASIL.csv");
                    //cand.ImportarDespesasPagas(@"C:\\temp\despesas_pagas_candidatos_2018_BRASIL.csv");
                    //cand.ImportarDespesasContratadas(@"C:\\temp\despesas_contratadas_candidatos_2018_BRASIL.csv");
                    //cand.ImportarReceitas(@"C:\\temp\receitas_candidatos_2018_BRASIL.csv");
                    //cand.ImportarReceitasDoadorOriginario(@"C:\\temp\receitas_candidatos_doador_originario_2018_BRASIL.csv");

                    Fornecedor objFornecedor = serviceProvider.GetService<Fornecedor>();
                    objFornecedor.ConsultarReceitaWS().Wait();

                    Console.WriteLine("Concluido! Tecle [ENTER] para sair.");
                    Console.ReadKey();
                }
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
        }

        private static async Task ImportacaoDadosCompleto(ServiceProvider serviceProvider, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            Senado objSenado = serviceProvider.GetService<Senado>();
            CamaraFederal objCamara = serviceProvider.GetService<CamaraFederal>();
            DistritoFederal objCamaraDistritoFederal = serviceProvider.GetService<DistritoFederal>();
            SantaCatarina objCamaraSantaCatarina = serviceProvider.GetService<SantaCatarina>();
            Fornecedor objFornecedor = serviceProvider.GetService<Fornecedor>();

            try
            {
                new Core.DAO.ParametrosRepository().CarregarPadroes();

                //objSenado.ImportarCompleto();
                //objCamara.ImportarCompleto();
                //objCamara.ImportaPresencasDeputados();

                //objCamaraDistritoFederal.ImportarCompleto();
                //objCamaraSantaCatarina.ImportarCompleto();
                //objCamara.ImportaPresencasDeputados();

                objFornecedor.ConsultarReceitaWS().Wait();

                //using (WebClient client = new WebClient())
                //{
                //    await client.DownloadDataTaskAsync("http://127.0.0.1:5200/tarefa/limparcache");
                //}

                //var lstEmails = Padrao.EmailEnvioResumoImportacao.Split(';');
                //var lstEmailTo = new MailAddressCollection();
                //foreach (string email in lstEmails)
                //{
                //    lstEmailTo.Add(email);
                //}

                //var data = DateTime.Now;
                //var tempPath = configuration["AppSettings:SiteTempFolder"];
                //var log = Utils.ReadAllText($"{tempPath}/log{data:yyyyMMdd}.txt");
                //await Utils.SendMailAsync(configuration["AppSettings:SendGridAPIKey"], lstEmailTo, "OPS :: Resumo da Importação - " + data.ToString("dd/MM/yyyy HH:mm"), log, null, false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);

                await Utils.SendMailAsync(configuration["AppSettings:SendGridAPIKey"], new MailAddress(Padrao.EmailEnvioErros), "OPS :: Informe de erro na Importação", ex.GetBaseException().Message, null, false);
            }
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