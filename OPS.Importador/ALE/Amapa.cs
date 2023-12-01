using System;
using AngleSharp;
using AngleSharp.Dom;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

public class Amapa : ImportadorBase
{
    public Amapa(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        //importadorParlamentar = new ImportadorParlamentarAmapa(serviceProvider);
        //importadorDespesas = new ImportadorDespesasAmapa(serviceProvider);
    }
}

public class ImportadorDespesasAmapa : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasAmapa(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "",
            Estado = Estado.Amapa,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        throw new NotImplementedException();
    }
}

public class ImportadorParlamentarAmapa : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarAmapa(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "",
            SeletorListaParlamentares = "",
            Estado = Estado.Amapa,
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
