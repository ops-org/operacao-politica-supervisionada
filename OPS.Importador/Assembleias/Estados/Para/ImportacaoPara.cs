using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Para
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
