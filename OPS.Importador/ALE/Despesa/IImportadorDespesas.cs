using System;

namespace OPS.Importador.ALE.Despesa
{
    public interface IImportadorDespesas
    {
        public void Importar(int ano);

        public void AtualizarDatasImportacaoDespesas(DateTime? dInicio = null, DateTime? dFim = null);
    }
}
