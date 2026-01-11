using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Pernambuco
{
    public class ImportacaoPernambuco : ImportadorBase
    {
        public ImportacaoPernambuco(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarPernambuco(serviceProvider);
            importadorDespesas = new ImportadorDespesasPernambuco(serviceProvider);
        }
    }
}
