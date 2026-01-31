using AngleSharp;
using OPS.Core.Enumerators;
using OPS.Importador.Assembleias.MatoGrosso.Entities;
using OPS.Importador.Comum.Despesa;

namespace OPS.Importador.Assembleias.MatoGrosso;

public class ImportadorDespesasMatoGrosso : ImportadorDespesasRestApiAnual
{
    public ImportadorDespesasMatoGrosso(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://almt.eloweb.net/portaltransparencia-api/empenhos",
            Estado = Estados.MatoGrosso,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override async Task ImportarDespesas(IBrowsingContext context, int ano)
    {
        var address = $"{config.BaseAddress}/lista?search=id.exercicio==%272025%27%20and%20id.entidade==%271%27%20and%20data%3E=%272025-01-01%27%20and%20data%3C=%272025-12-31%27&programatica.projeto=4491&programatica.elemento=339093&entidade=1&size=20&sort=data,DESC";
        EmpenhoResponse objEmpenhos = await RestApiGet<EmpenhoResponse>(address);


        foreach (var empenho in objEmpenhos.Content)
        {
            var objCamaraEstadualDespesaTemp = new CamaraEstadualDespesaTemp()
            {
                Ano = (short)ano,
                Mes = (short)empenho.Data.Month,
                TipoDespesa = "Indenizações e Restituições",
                Valor = empenho.ValorPago,
                DataEmissao = DateOnly.FromDateTime(empenho.Data)
            };

            InserirDespesaTemp(objCamaraEstadualDespesaTemp);
        }
    }
}


