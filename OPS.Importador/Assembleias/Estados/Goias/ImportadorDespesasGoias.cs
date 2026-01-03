using System;
using System.Collections.Generic;
using System.Globalization;
using AngleSharp;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Assembleias.Estados.Goias.Entities;

namespace OPS.Importador.Assembleias.Estados.Goias;

public class ImportadorDespesasGoias : ImportadorDespesasRestApiMensal
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("en-US");

    public ImportadorDespesasGoias(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://transparencia.al.go.leg.br/",
            Estado = Estado.Goias,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano, int mes)
    {
        var address = $"{config.BaseAddress}api/transparencia/verbas_indenizatorias?ano={ano}&mes={mes}&por_pagina=100";
        List<DespesasGoias> lstDeputadosRS = RestApiGet<List<DespesasGoias>>(address);

        foreach (DespesasGoias item in lstDeputadosRS)
        {
            var id = item.Deputado.Id;

            address = $"{config.BaseAddress}api/transparencia/verbas_indenizatorias/exibir?ano={ano}&deputado_id={id}&mes={mes}";
            DespesasGoias despesa = RestApiGet<DespesasGoias>(address);
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
                            DataEmissao = lancamento.Fornecedor.Data,
                            CnpjCpf = Utils.RemoveCaracteresNaoNumericos(lancamento.Fornecedor.CnpjCpf),
                            Empresa = lancamento.Fornecedor.Nome,
                            Documento = lancamento.Fornecedor.Numero
                        };

                        InserirDespesaTemp(despesaTemp);
                    }
        }
    }
}