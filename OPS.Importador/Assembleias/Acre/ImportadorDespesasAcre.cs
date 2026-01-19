using AngleSharp;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerators;
using OPS.Importador.Comum.Despesa;

namespace OPS.Importador.Assembleias.Acre
{
    public class ImportadorDespesasAcre : ImportadorDespesasRestApiAnual
    {
        public ImportadorDespesasAcre(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://app.al.ac.leg.br/financas/despesas/despesas-por-classificacao", // TODO: Gastos totais ANUAL apenas
                Estado = Estados.Acre,
                ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
            };
        }

        public override Task ImportarDespesas(IBrowsingContext context, int ano)
        {
            logger.LogWarning("Portal sem dados detalhados!");

            return Task.CompletedTask;
        }
    }
}
