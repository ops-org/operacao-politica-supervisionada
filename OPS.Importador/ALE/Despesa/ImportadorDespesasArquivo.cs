﻿using System;
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
                        if(System.IO.File.Exists(caminhoArquivo))
                            System.IO.File.Delete(caminhoArquivo);
#endif

                            }
                        }
                    }
                }

                ProcessarDespesas(ano);
            }
        }

        public abstract void ImportarDespesas(string caminhoArquivo, int ano);
    }
}
