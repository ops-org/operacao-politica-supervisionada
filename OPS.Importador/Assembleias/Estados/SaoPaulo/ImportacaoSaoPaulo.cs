using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.SaoPaulo
{
    public class ImportacaoSaoPaulo : ImportadorBase
    {
        public ImportacaoSaoPaulo(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarSaoPaulo(serviceProvider);
            importadorDespesas = new ImportadorDespesasSaoPaulo(serviceProvider);
        }
    }
}
