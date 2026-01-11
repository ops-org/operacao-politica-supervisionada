using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.DistritoFederal
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
