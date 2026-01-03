using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Tocantins
{
    public class ImportacaoTocantins : ImportadorBase
    {
        public ImportacaoTocantins(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarTocantins(serviceProvider);
            importadorDespesas = new ImportadorDespesasTocantins(serviceProvider);
        }
    }
}
