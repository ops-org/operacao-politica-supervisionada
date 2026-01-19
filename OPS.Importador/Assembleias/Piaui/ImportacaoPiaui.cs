using OPS.Importador.Assembleias.Piaui;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Piaui
{
    public class ImportacaoPiaui : ImportadorBase
    {
        public ImportacaoPiaui(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarPiaui(serviceProvider);
            importadorDespesas = new ImportadorDespesasPiaui(serviceProvider);
        }
    }
}
