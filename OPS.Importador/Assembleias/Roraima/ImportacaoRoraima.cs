using OPS.Importador.Assembleias.Roraima;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Roraima
{
    public class ImportacaoRoraima : ImportadorBase
    {
        public ImportacaoRoraima(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarRoraima(serviceProvider);
            importadorDespesas = new ImportadorDespesasRoraima(serviceProvider);
        }
    }
}
