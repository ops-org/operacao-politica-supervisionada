using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.EspiritoSanto
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
