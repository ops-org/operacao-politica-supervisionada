using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Paraiba
{
    public class ImportacaoParaiba : ImportadorBase
    {
        public ImportacaoParaiba(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarParaiba(serviceProvider);
            importadorDespesas = new ImportadorDespesasParaiba(serviceProvider);
        }
    }
}
