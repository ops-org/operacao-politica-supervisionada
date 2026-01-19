using OPS.Importador.Assembleias.MatoGrossoDoSul;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.MatoGrossoDoSul
{
    public class ImportacaoMatoGrossoDoSul : ImportadorBase
    {
        public ImportacaoMatoGrossoDoSul(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarMatoGrossoDoSul(serviceProvider);
            importadorDespesas = new ImportadorDespesasMatoGrossoDoSul(serviceProvider);
        }
    }
}
