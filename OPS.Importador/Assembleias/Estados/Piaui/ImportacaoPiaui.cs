using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Piaui
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
