using OPS.Importador.Assembleias.Parana;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Parana
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
