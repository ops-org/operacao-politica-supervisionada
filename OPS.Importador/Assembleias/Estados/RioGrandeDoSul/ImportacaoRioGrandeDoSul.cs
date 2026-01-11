using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul
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
