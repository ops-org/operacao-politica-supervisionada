﻿using System;
using AngleSharp;
using AngleSharp.Dom;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Importador.ALE.Comum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

public class MatoGrosso : ImportadorBase
{
    public MatoGrosso(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarMatoGrosso(serviceProvider);
        importadorDespesas = new ImportadorDespesasMatoGrosso(serviceProvider);
    }
}

public class ImportadorDespesasMatoGrosso : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasMatoGrosso(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "",
            Estado = Estado.MatoGrosso,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        throw new NotImplementedException();
    }
}

public class ImportadorParlamentarMatoGrosso : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarMatoGrosso(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "",
            SeletorListaParlamentares = "",
            Estado = Estado.MatoGrosso,
        });
    }

    public override DeputadoEstadual ColetarDadosLista(IElement document)
    {
        throw new NotImplementedException();
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        throw new NotImplementedException();
    }
}
