using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Amazonas
{
    public class ImportacaoAmazonas : ImportadorBase
    {
        public ImportacaoAmazonas(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarAmazonas(serviceProvider);
            importadorDespesas = new ImportadorDespesasAmazonas(serviceProvider);
        }
    }
}
