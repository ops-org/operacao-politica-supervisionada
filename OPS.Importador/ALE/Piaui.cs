using System;
using AngleSharp;
using AngleSharp.Dom;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

public class Piaui : ImportadorBase
{
    public Piaui(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        //importadorParlamentar = new ImportadorParlamentarPiaui(serviceProvider);
        //importadorDespesas = new ImportadorDespesasPiaui(serviceProvider);
    }
}

public class ImportadorDespesasPiaui : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasPiaui(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "",
            Estado = Estado.Piaui,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        throw new NotImplementedException();
    }
}

public class ImportadorParlamentarPiaui : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarPiaui(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "",
            SeletorListaParlamentares = "",
            Estado = Estado.Piaui,
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
