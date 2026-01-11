using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Acre
{
    public class ImportacaoAcre : ImportadorBase
    {
        public ImportacaoAcre(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarAcre(serviceProvider);
            importadorDespesas = new ImportadorDespesasAcre(serviceProvider);
        }
    }
}
