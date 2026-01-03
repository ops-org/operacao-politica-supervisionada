using System;
using AngleSharp;
using Microsoft.Extensions.Logging;
using OPS.Core.Enumerator;
using OPS.Importador.Assembleias.Despesa;

namespace OPS.Importador.Assembleias.Estados.Acre
{
    public class ImportadorDespesasAcre : ImportadorDespesasRestApiAnual
    {
        public ImportadorDespesasAcre(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://app.al.ac.leg.br/financas/despesas/despesas-por-classificacao", // TODO: Gastos totais ANUAL apenas
                Estado = Estado.Acre,
                ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
            };
        }

        public override void ImportarDespesas(IBrowsingContext context, int ano)
        {
            logger.LogWarning("Portal sem dados detalhados!");
        }
    }
}
