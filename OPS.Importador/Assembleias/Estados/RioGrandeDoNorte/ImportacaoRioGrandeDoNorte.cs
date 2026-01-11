using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoNorte
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
