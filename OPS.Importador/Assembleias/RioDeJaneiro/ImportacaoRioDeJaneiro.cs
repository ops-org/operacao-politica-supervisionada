using OPS.Importador.Assembleias.RioDeJaneiro;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.RioDeJaneiro
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
