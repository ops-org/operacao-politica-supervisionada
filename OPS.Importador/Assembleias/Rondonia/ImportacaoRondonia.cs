using OPS.Importador.Assembleias.Rondonia;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.Rondonia
{
    public class ImportacaoRondonia : ImportadorBase
    {
        public ImportacaoRondonia(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarRondonia(serviceProvider);
            importadorDespesas = new ImportadorDespesasRondonia(serviceProvider);
        }
    }
}
