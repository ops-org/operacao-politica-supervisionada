using OPS.Importador.Assembleias.Amazonas;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Amazonas
{
    public class ImportacaoAmazonas : ImportadorBase
    {
        public ImportacaoAmazonas(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarAmazonas(serviceProvider);
            importadorDespesas = new ImportadorDespesasAmazonas(serviceProvider);
        }
    }
}
