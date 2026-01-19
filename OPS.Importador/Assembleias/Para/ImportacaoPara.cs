using OPS.Importador.Assembleias.Para;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Para
{
    public class ImportacaoPara : ImportadorBase
    {
        public ImportacaoPara(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarPara(serviceProvider);
            importadorDespesas = new ImportadorDespesasPara(serviceProvider);
        }
    }
}
