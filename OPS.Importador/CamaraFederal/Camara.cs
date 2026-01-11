using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.CamaraFederal;

public class Camara : ImportadorBase
{
    public Camara(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarCamaraFederal(serviceProvider);
        importadorDespesas = new ImportadorDespesasCamaraFederal(serviceProvider);
    }
}
