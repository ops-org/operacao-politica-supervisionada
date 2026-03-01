using Microsoft.Extensions.Hosting;
using OPS.Infraestrutura;
using Serilog;

namespace OPS.Importador
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.AddServiceDefaults();
            builder.AddNpgsqlDbContext<AppDbContext>("AuditoriaContext");

            ConfigureApp.SetupEnvironment();
            ConfigureApp.ConfigureServices(builder.Services, builder.Configuration);

            var host = builder.Build();

            try
            {
                await ImportOrchestrator.RunAppAsync(host.Services);
            }
            catch (Exception ex)
            {
                HandleFatalException(ex);
            }
            finally
            {
                Log.CloseAndFlush();
            }

            Console.WriteLine("Concluido! Tecle [ENTER] para sair.");
            Console.ReadKey();
        }

        private static void HandleFatalException(Exception ex)
        {
            if (Log.Logger == null || Log.Logger.GetType().Name == "SilentLogger")
            {
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();
            }

            Log.Fatal(ex, "Host terminated unexpectedly");
        }
    }
}