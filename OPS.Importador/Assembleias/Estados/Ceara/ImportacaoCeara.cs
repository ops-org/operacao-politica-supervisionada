using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Ceara
{
    public class ImportacaoCeara : ImportadorBase
    {
        public ImportacaoCeara(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarCeara(serviceProvider);
            importadorDespesas = new ImportadorDespesasCeara(serviceProvider);
        }
    }
}
