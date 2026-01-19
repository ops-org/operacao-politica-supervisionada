using OPS.Importador.Assembleias.Bahia;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Bahia
{
    public class ImportacaoBahia : ImportadorBase
    {
        public ImportacaoBahia(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarBahia(serviceProvider);
            importadorDespesas = new ImportadorDespesasBahia(serviceProvider);
        }
    }
}
