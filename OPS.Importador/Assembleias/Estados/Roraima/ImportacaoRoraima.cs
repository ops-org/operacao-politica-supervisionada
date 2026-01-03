using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Roraima
{
    public class ImportacaoRoraima : ImportadorBase
    {
        public ImportacaoRoraima(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarRoraima(serviceProvider);
            importadorDespesas = new ImportadorDespesasRoraima(serviceProvider);
        }
    }
}
