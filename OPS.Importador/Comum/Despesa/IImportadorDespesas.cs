namespace OPS.Importador.Comum.Despesa
{
    public interface IImportadorDespesas
    {
        public Task Importar(int ano, CancellationToken ct = default);

        public void AtualizarDatasImportacaoDespesas(DateTime? dInicio = null, DateTime? dFim = null);
    }
}
