using System.Threading.Tasks;

namespace OPS.Importador.ALE.Parlamentar
{
    public interface IImportadorParlamentar
    {
        public Task Importar();

        public Task DownloadFotos();
    }
}
