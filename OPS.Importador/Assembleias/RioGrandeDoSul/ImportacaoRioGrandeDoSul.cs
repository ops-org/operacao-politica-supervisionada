using OPS.Importador.Assembleias.RioGrandeDoSul;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.RioGrandeDoSul
{
    public class ImportacaoRioGrandeDoSul : ImportadorBase
    {
        public ImportacaoRioGrandeDoSul(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarRioGrandeDoSul(serviceProvider);
            importadorDespesas = new ImportadorDespesasRioGrandeDoSul(serviceProvider);
        }
    }
}
