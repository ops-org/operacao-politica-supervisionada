﻿using System;
using System.Collections.Generic;
using AngleSharp;
using OPS.Importador.Utilities;

namespace OPS.Importador.ALE.Despesa
{
    public abstract class ImportadorDespesasRestApiAnual : ImportadorDespesasBase, IImportadorDespesas
    {
        public ImportadorDespesasRestApiAnual(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public void Importar(int ano)
        {
            using (logger.BeginScope(new Dictionary<string, object> { ["Ano"] = ano }))
            {
                CarregarHashes(ano);

                var context = httpClientResilient.CreateAngleSharpContext();

                ImportarDespesas(context, ano);
                ProcessarDespesas(ano);
            }
        }

        public abstract void ImportarDespesas(IBrowsingContext context, int ano);
    }

    //public class ImportadorDespesasRestApiConfig
    //{
    //    public string BaseAddress { get; internal set; }
    //    public Estado Estado { get; internal set; }
    //}
}
