using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace OPS.Importador
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            ConfigureApp.SetupEnvironment();

            var configuration = ConfigureApp.BuildConfiguration();
            ConfigureApp.SetupLogging(configuration);

            try
            {
                var services = new ServiceCollection();
                ConfigureApp.ConfigureServices(services, configuration);

                var serviceProvider = services.BuildServiceProvider();
                serviceProvider.GetService<ILoggerFactory>().AddSerilog(Log.Logger, true);

                await ImportOrchestrator.RunAppAsync(serviceProvider);
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