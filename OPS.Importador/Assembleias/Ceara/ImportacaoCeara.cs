using OPS.Importador.Assembleias.Ceara;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Ceara
{
    public class ImportacaoCeara : ImportadorBase
    {
        public ImportacaoCeara(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarCeara(serviceProvider);
            importadorDespesas = new ImportadorDespesasCeara(serviceProvider);
        }
    }
}
