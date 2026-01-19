using OPS.Importador.Assembleias.Paraiba;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Paraiba
{
    public class ImportacaoParaiba : ImportadorBase
    {
        public ImportacaoParaiba(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarParaiba(serviceProvider);
            importadorDespesas = new ImportadorDespesasParaiba(serviceProvider);
        }
    }
}
