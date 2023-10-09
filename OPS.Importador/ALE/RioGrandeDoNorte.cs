using System;
using AngleSharp;
using AngleSharp.Dom;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

public class RioGrandeDoNorte : ImportadorBase
{
    public RioGrandeDoNorte(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        //importadorParlamentar = new ImportadorParlamentarRioGrandeDoNorte(serviceProvider);
        //importadorDespesas = new ImportadorDespesasRioGrandeDoNorte(serviceProvider);
    }
}

public class ImportadorDespesasRioGrandeDoNorte : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasRioGrandeDoNorte(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "",
            Estado = Estado.RioGrandeDoNorte,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        throw new NotImplementedException();
    }
}

public class ImportadorParlamentarRioGrandeDoNorte : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarRioGrandeDoNorte(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "",
            SeletorListaParlamentares = "",
            Estado = Estado.RioGrandeDoNorte,
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

