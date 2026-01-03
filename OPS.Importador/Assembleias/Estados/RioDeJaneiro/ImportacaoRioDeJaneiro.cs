using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.RioDeJaneiro
{
    public class ImportacaoRioDeJaneiro : ImportadorBase
    {
        public ImportacaoRioDeJaneiro(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarRioDeJaneiro(serviceProvider);
            importadorDespesas = new ImportadorDespesasRioDeJaneiro(serviceProvider);
        }
    }
}
