using OPS.Importador.Assembleias.Amapa;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Amapa
{
    public class ImportacaoAmapa : ImportadorBase
    {
        public ImportacaoAmapa(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarAmapa(serviceProvider);
            importadorDespesas = new ImportadorDespesasAmapa(serviceProvider);
        }
    }
}
