using OPS.Importador.Assembleias.SaoPaulo;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.SaoPaulo
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
