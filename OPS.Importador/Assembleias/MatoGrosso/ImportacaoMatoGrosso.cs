using OPS.Importador.Assembleias.MatoGrosso;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.MatoGrosso
{
    public class ImportacaoMatoGrosso : ImportadorBase
    {
        public ImportacaoMatoGrosso(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarMatoGrosso(serviceProvider);
            importadorDespesas = new ImportadorDespesasMatoGrosso(serviceProvider);
        }
    }
}
