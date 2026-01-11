using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Amapa
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
