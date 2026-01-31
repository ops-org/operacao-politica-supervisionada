using OPS.Importador.Assembleias.Acre;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Acre
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
