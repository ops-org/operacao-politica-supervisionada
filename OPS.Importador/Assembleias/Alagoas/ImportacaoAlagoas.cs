using OPS.Importador.Assembleias.Alagoas;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Alagoas
{
    public class ImportacaoAlagoas : ImportadorBase
    {
        public ImportacaoAlagoas(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarAlagoas(serviceProvider);
            importadorDespesas = new ImportadorDespesasAlagoas(serviceProvider);
        }
    }
}
