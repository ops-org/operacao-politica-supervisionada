using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Parana
{
    public class ImportacaoParana : ImportadorBase
    {
        public ImportacaoParana(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarParana(serviceProvider);
            importadorDespesas = new ImportadorDespesasParana(serviceProvider);
        }
    }
}
