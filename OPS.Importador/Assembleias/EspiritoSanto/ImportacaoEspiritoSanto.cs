using OPS.Importador.Assembleias.EspiritoSanto;
using OPS.Importador.Comum;

namespace OPS.Importador.Assembleias.EspiritoSanto
{
    public class ImportacaoEspiritoSanto : ImportadorBase
    {
        public ImportacaoEspiritoSanto(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarEspiritoSanto(serviceProvider);
            importadorDespesas = new ImportadorDespesasEspiritoSanto(serviceProvider);
        }
    }
}
