using Microsoft.Extensions.Logging;

namespace OPS.Importador.Assembleias.Despesa
{
    public abstract class ImportadorDespesasArquivo : ImportadorDespesasBase, IImportadorDespesas
    {
        public ImportadorDespesasArquivo(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public Task Importar(int ano)
        {
            var anoAtual = DateTime.Today.Year;
            using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano }))
            {
                CarregarHashes(ano);

                Dictionary<string, string> arquivos = DefinirUrlOrigemCaminhoDestino(ano);

                foreach (var arquivo in arquivos)
                {
                    var _urlOrigem = arquivo.Key;
                    var caminhoArquivo = arquivo.Value;

                    using (logger.BeginScope(new Dictionary<string, object> { ["Url"] = _urlOrigem, ["Arquivo"] = System.IO.Path.GetFileName(caminhoArquivo) }))
                    {
                        var novoArquivoBaixado = BaixarArquivo(_urlOrigem, caminhoArquivo);
                        if (anoAtual != ano && importacaoIncremental && !novoArquivoBaixado && arquivos.Count == 1 && config.Estado != Estado.Piaui)
                        {
                            logger.LogInformation("Importação ignorada para arquivo previamente importado!");
                            return Task.CompletedTask;
                        }

                        try
                        {
                            ImportarDespesas(caminhoArquivo, ano);
                        }
                        catch (Exception ex)
                        {

                            logger.LogError(ex, ex.Message);

#if !DEBUG
                        //Excluir o arquivo para tentar importar novamente na proxima execução
                        if(System.IO.File.Exists(caminhoArquivo))
                            System.IO.File.Delete(caminhoArquivo);
#endif

                        }
                    }
                }

                ProcessarDespesas(ano);
            }

            return Task.CompletedTask;
        }

        public abstract void ImportarDespesas(string caminhoArquivo, int ano);
    }
}
