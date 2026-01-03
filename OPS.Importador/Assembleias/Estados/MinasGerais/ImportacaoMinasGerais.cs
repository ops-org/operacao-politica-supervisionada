using System;
using OPS.Importador.Assembleias.Comum;

namespace OPS.Importador.Assembleias.Estados.MinasGerais
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
