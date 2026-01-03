using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.Rondonia
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
