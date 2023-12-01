using System;
using AngleSharp;
using AngleSharp.Dom;
using OPS.Core.Entity;
using OPS.Core.Enum;
using OPS.Importador.ALE.Despesa;
using OPS.Importador.ALE.Parlamentar;

namespace OPS.Importador.ALE;

public class Maranhao : ImportadorBase
{
    public Maranhao(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        //importadorParlamentar = new ImportadorParlamentarMaranhao(serviceProvider);
        //importadorDespesas = new ImportadorDespesasMaranhao(serviceProvider);
    }
}

public class ImportadorDespesasMaranhao : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasMaranhao(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "",
            Estado = Estado.Maranhao,
            ChaveImportacao = ChaveDespesaTemp.Indefinido
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        throw new NotImplementedException();
    }
}

public class ImportadorParlamentarMaranhao : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarMaranhao(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "",
            SeletorListaParlamentares = "",
            Estado = Estado.Maranhao,
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
