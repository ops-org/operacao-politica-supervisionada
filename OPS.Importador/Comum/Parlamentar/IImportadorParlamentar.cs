namespace OPS.Importador.Comum.Parlamentar
{
    public interface IImportadorParlamentar
    {
        public Task Importar();

        public Task DownloadFotos();

        public void AtualizarDatasImportacaoParlamentar(DateTime? pInicio = null, DateTime? pFim = null);
    }
}
