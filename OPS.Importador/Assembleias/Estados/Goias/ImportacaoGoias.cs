using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Goias
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
