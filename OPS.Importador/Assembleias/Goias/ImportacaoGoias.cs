using OPS.Importador.Assembleias.Goias;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Goias
{
    public class ImportacaoGoias : ImportadorBase
    {
        public ImportacaoGoias(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarGoias(serviceProvider);
            importadorDespesas = new ImportadorDespesasGoias(serviceProvider);
        }
    }
}
