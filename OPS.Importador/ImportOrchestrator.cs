using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace OPS.Importador
{
    internal static class ImportOrchestrator
    {
        public static async Task RunAppAsync(IServiceProvider serviceProvider)
        {

            var logger = serviceProvider.GetService<ILogger<Program>>();
            logger.LogInformation("Iniciando Importação");

            //var ipcaImportador = serviceProvider.GetRequiredService<IndiceInflacaoImportador>();
            //ipcaImportador.ImportarIpca().Wait();

            var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
            await InitializeDatabaseAsync(dbContext);

            var crawler = new SeleniumScraper(serviceProvider);
            //crawler.BaixarArquivosParana();
            crawler.BaixarArquivosPiaui();

            await RunImportersAsync(serviceProvider, logger);

            //var importador = serviceProvider.GetService<CamaraFederal.ImportacaoCamaraFederal>();

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
            await objFornecedor.ConsultarDadosCNPJ();
            //objFornecedor.ConsultarDadosCNPJ(somenteNovos: false).Wait();
        }

        private static async Task RunImportersAsync(IServiceProvider serviceProvider, ILogger logger)
        {
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
                tasks.Add(Task.Run(async () =>
                {
                    var estado = type.Name.Replace("Importador", "");
                    using (logger.BeginScope(new Dictionary<string, object> { ["Code"] = Utils.GetStateCode(estado), ["Estado"] = estado, ["ProcessIdentifier"] = Guid.NewGuid().ToString() }))
                    {
                        logger.LogInformation("Iniciando importação do(a) {Estado}.", estado);
                        var watch = System.Diagnostics.Stopwatch.StartNew();

                        var importador = (ImportadorBase)serviceProvider.GetService(type);
                        await importador.ImportarCompleto();

                        watch.Stop();
                        logger.LogInformation("Processamento do(a) {Estado} finalizado em {TimeElapsed:c}", type.Name, watch.Elapsed);
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }

        private static async Task InitializeDatabaseAsync(AppDbContext dbContext)
        {
            await dbContext.Database.ExecuteSqlRawAsync(@"
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
                ON CONFLICT DO NOTHING;");
        }
    }
}
