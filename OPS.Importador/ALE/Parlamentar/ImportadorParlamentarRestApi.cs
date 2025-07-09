using System;

namespace OPS.Importador.ALE.Parlamentar
{
    public abstract class ImportadorParlamentarRestApi : ImportadorParlamentarBase
    {

        public ImportadorParlamentarRestApi(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
