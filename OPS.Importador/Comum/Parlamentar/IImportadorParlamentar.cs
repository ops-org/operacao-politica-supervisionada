namespace OPS.Importador.Comum.Parlamentar
{
    public interface IImportadorParlamentar
    {
        public Task Importar(CancellationToken ct = default);

        public Task DownloadFotos(CancellationToken ct = default);

        public void AtualizarDatasImportacaoParlamentar(DateTime? pInicio = null, DateTime? pFim = null);
    }
}
