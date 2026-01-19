using OPS.Importador.Assembleias.Pernambuco;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Pernambuco
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
