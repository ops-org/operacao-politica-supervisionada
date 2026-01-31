using OPS.Importador.Assembleias.Maranhao;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Maranhao
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
