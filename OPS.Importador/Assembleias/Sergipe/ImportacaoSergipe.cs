using OPS.Importador.Assembleias.Sergipe;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Sergipe
{
    public class ImportacaoSergipe : ImportadorBase
    {
        public ImportacaoSergipe(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarSergipe(serviceProvider);
            importadorDespesas = new ImportadorDespesasSergipe(serviceProvider);
        }
    }
}
