using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.MatoGrossoDoSul
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
