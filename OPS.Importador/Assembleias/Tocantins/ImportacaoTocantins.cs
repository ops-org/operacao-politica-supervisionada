using OPS.Importador.Assembleias.Tocantins;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Tocantins
{
    public class ImportacaoTocantins : ImportadorBase
    {
        public ImportacaoTocantins(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarTocantins(serviceProvider);
            importadorDespesas = new ImportadorDespesasTocantins(serviceProvider);
        }
    }
}
