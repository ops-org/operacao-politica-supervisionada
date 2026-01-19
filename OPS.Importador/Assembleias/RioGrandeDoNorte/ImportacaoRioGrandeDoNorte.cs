using OPS.Importador.Assembleias.RioGrandeDoNorte;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.RioGrandeDoNorte
{
    public class ImportacaoRioGrandeDoNorte : ImportadorBase
    {
        public ImportacaoRioGrandeDoNorte(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarRioGrandeDoNorte(serviceProvider);
            importadorDespesas = new ImportadorDespesasRioGrandeDoNorte(serviceProvider);
        }
    }
}
