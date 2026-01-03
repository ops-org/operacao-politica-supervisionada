using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Sergipe
{
    public class ImportacaoSergipe : ImportadorBase
    {
        public ImportacaoSergipe(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarSergipe(serviceProvider);
            importadorDespesas = new ImportadorDespesasSergipe(serviceProvider);
        }
    }
}
