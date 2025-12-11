using System;

namespace OPS.Importador.Assembleias.Parlamentar
{
    public abstract class ImportadorParlamentarRestApi : ImportadorParlamentarBase
    {

        public ImportadorParlamentarRestApi(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
