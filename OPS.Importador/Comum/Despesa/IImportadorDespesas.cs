namespace OPS.Importador.Comum.Despesa
{
    public interface IImportadorDespesas
    {
        public Task Importar(int ano);

        public void AtualizarDatasImportacaoDespesas(DateTime? dInicio = null, DateTime? dFim = null);
    }
}
