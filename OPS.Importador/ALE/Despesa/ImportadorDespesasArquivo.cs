using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OPS.Importador.ALE.Despesa
{
    public abstract class ImportadorDespesasArquivo : ImportadorDespesasBase, IImportadorDespesas
    {
        public ImportadorDespesasArquivo(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public virtual void Importar(int ano)
        {
            logger.LogWarning("Despesas do(a) {idEstado}:{CasaLegislativa} de {Ano}", config.Estado.GetHashCode(), config.Estado.ToString(), ano);

            CarregarHashes(ano);
            LimpaDespesaTemporaria();

            Dictionary<string, string> arquivos = DefinirUrlOrigemCaminhoDestino(ano);

            foreach (var arquivo in arquivos)
            {
                var _urlOrigem = arquivo.Key;
                var caminhoArquivo = arquivo.Value;

                if (TentarBaixarArquivo(_urlOrigem, caminhoArquivo))
                {
                    try
                    {
                        ImportarDespesas(caminhoArquivo, ano);
                    }
                    catch (Exception ex)
                    {

                        logger.LogError(ex, ex.Message);

#if !DEBUG
                        //Excluir o arquivo para tentar importar novamente na proxima execução
                        if(File.Exists(_caminhoArquivo))
                            File.Delete(_caminhoArquivo);
#endif

                    }
                }
            }

            ProcessarDespesas(ano);
        }

        public abstract void ImportarDespesas(string caminhoArquivo, int ano);
    }
}
