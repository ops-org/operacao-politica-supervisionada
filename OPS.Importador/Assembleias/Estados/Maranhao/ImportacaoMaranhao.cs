using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Maranhao
{
    public class ImportacaoMaranhao : ImportadorBase
    {
        public ImportacaoMaranhao(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            //importadorParlamentar = new ImportadorParlamentarMaranhao(serviceProvider);
            importadorDespesas = new ImportadorDespesasMaranhao(serviceProvider);
        }
    }
}
