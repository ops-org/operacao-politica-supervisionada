using OPS.Importador.Assembleias.SantaCatarina;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.SantaCatarina
{
    public class ImportacaoSantaCatarina : ImportadorBase
    {
        public ImportacaoSantaCatarina(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarSantaCatarina(serviceProvider);
            importadorDespesas = new ImportadorDespesasSantaCatarina(serviceProvider);
        }
    }
}
