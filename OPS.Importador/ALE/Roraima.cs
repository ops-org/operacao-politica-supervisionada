using System;
using AngleSharp;
using AngleSharp.Dom;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

public class Roraima : ImportadorBase
{
    public Roraima(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        //importadorParlamentar = new ImportadorParlamentarRoraima(serviceProvider);
        //importadorDespesas = new ImportadorDespesasRoraima(serviceProvider);
    }
}

public class ImportadorDespesasRoraima : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasRoraima(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "",
            Estado = Estado.Roraima,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        throw new NotImplementedException();
    }
}

public class ImportadorParlamentarRoraima : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarRoraima(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "",
            SeletorListaParlamentares = "",
            Estado = Estado.Roraima,
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
