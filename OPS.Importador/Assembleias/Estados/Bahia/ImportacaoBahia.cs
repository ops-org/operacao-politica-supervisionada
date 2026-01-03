using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Bahia
{
    public class ImportacaoBahia : ImportadorBase
    {
        public ImportacaoBahia(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarBahia(serviceProvider);
            importadorDespesas = new ImportadorDespesasBahia(serviceProvider);
        }
    }
}
