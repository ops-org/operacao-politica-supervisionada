using Microsoft.Extensions.Logging;
using OPS.Core.Enumerators;

namespace OPS.Importador.Comum.Despesa
{
    public abstract class ImportadorDespesasArquivo : ImportadorDespesasBase, IImportadorDespesas
    {
        public ImportadorDespesasArquivo(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public virtual async Task Importar(int ano, CancellationToken ct = default)
        {
            using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano }))
            {
                CarregarHashes(ano);

                Dictionary<string, string> arquivos = DefinirUrlOrigemCaminhoDestino(ano);

                for (int i = 0; i < arquivos.Count(); i++)
                {
                    var arquivo = arquivos.ElementAt(i);
                    var _urlOrigem = arquivo.Key;
                    var caminhoArquivo = arquivo.Value;

                    using (logger.BeginScope(new Dictionary<string, object> { ["Url"] = _urlOrigem, ["Arquivo"] = System.IO.Path.GetFileName(caminhoArquivo) }))
                    {
                        var novoArquivoBaixado = await fileManager.BaixarArquivo(dbContext, _urlOrigem, caminhoArquivo, config.Estado);
                        if (!appSettings.ForceImport && !novoArquivoBaixado && arquivos.Count == 1 && config.Estado != Estados.Piaui)
                        {
                            logger.LogInformation("Importação ignorada para arquivo previamente importado!");
                            return;
                        }

                        try
                        {
                            ImportarDespesas(caminhoArquivo, ano, i + 1);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, ex.Message);
                            fileManager.MoverArquivoComErro(caminhoArquivo);
                        }
                    }
                }

                ProcessarDespesas(ano);
            }
        }

        public abstract void ImportarDespesas(string caminhoArquivo, int ano, int? mes = null);
    }
}
