using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.SantaCatarina
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
