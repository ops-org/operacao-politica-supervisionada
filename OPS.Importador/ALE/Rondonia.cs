using System;
using AngleSharp;
using AngleSharp.Dom;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

public class Rondonia : ImportadorBase
{
    public Rondonia(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        //importadorParlamentar = new ImportadorParlamentarRondonia(serviceProvider);
        //importadorDespesas = new ImportadorDespesasRondonia(serviceProvider);
    }
}

public class ImportadorDespesasRondonia : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasRondonia(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "",
            Estado = Estado.Rondonia,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        throw new NotImplementedException();
    }
}

public class ImportadorParlamentarRondonia : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarRondonia(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "",
            SeletorListaParlamentares = "",
            Estado = Estado.Rondonia,
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
