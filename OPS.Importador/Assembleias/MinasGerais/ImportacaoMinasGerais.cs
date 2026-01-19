using OPS.Importador.Assembleias.MinasGerais;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.MinasGerais
{
    public class ImportacaoMinasGerais : ImportadorBase
    {
        public ImportacaoMinasGerais(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarMinasGerais(serviceProvider);
            importadorDespesas = new ImportadorDespesasMinasGerais(serviceProvider);
        }
    }
}
