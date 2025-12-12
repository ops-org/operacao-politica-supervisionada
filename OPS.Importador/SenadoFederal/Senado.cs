using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.SenadoFederal
{
    public class Senado : ImportadorBase
    {
        public Senado(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            importadorParlamentar = new ImportadorParlamentarSenado(serviceProvider);
            importadorDespesas = new ImportadorDespesasSenado(serviceProvider);
        }
    }
}