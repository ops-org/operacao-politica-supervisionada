using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.MatoGrosso
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
