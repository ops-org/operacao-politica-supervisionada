using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Alagoas
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
