using OPS.Importador.Assembleias.DistritoFederal;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.DistritoFederal
{
    public class ImportacaoDistritoFederal : ImportadorBase
    {
        public ImportacaoDistritoFederal(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarDistritoFederal(serviceProvider);
            importadorDespesas = new ImportadorDespesasDistritoFederal(serviceProvider);
        }
    }
}
