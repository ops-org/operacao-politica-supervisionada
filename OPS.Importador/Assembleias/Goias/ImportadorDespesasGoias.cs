using System.Globalization;
using AngleSharp;
using OPS.Core.Enumerators;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Goias.Entities;
using OPS.Importador.Comum.Despesa;

namespace OPS.Importador.Assembleias.Goias;

public class ImportadorDespesasGoias : ImportadorDespesasRestApiMensal
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("en-US");

    public ImportadorDespesasGoias(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://transparencia.al.go.leg.br/",
            Estado = Estados.Goias,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override async Task ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var address = $"{config.BaseAddress}api/transparencia/verbas_indenizatorias?ano={ano}&mes={mes}&por_pagina=100";
        List<DespesasGoias> lstDeputadosRS = await RestApiGet<List<DespesasGoias>>(address);

        foreach (DespesasGoias item in lstDeputadosRS)
        {
            var id = item.Deputado.Id;

            address = $"{config.BaseAddress}api/transparencia/verbas_indenizatorias/exibir?ano={ano}&deputado_id={id}&mes={mes}";
            DespesasGoias despesa = await RestApiGet<DespesasGoias>(address);
            if (despesa.Grupos is null) continue;

            foreach (var grupo in despesa.Grupos)
                foreach (var sub in grupo.Subgrupos)
                    foreach (var lancamento in sub.Lancamentos)
                    {
                        var despesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Nome = despesa.Deputado.Nome.Trim().ToTitleCase(),
                            Cpf = id.ToString(), // gabinete
                            Ano = (short)ano,
                            TipoDespesa = grupo.Descricao,
                            Valor = Convert.ToDecimal(lancamento.Fornecedor.ValorIndenizado, cultureInfo),
                            DataEmissao = DateOnly.FromDateTime(lancamento.Fornecedor.Data),
                            CnpjCpf = Utils.RemoveCaracteresNaoNumericos(lancamento.Fornecedor.CnpjCpf),
                            NomeFornecedor = lancamento.Fornecedor.Nome,
                            Documento = lancamento.Fornecedor.Numero,
                            Origem = address
                        };

                        InserirDespesaTemp(despesaTemp);
                    }
        }
    }
}