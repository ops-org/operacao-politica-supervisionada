using System;
using System.Globalization;
using System.Net;
using AngleSharp;
using Microsoft.Extensions.Logging;
using OPS.Core;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Assembleias.Estados.RioDeJaneiro.Entities;

namespace OPS.Importador.Assembleias.Estados.RioDeJaneiro
{
    public class ImportadorDespesasRioDeJaneiro : ImportadorDespesasRestApiAnual
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pt-BR");

        /// <summary>
        /// https://docigp.alerj.rj.gov.br/transparencia#/
        /// </summary>
        /// <param name="serviceProvider"></param>
        public ImportadorDespesasRioDeJaneiro(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://docigp.alerj.rj.gov.br/api/v1/",
                Estado = Estado.RioDeJaneiro,
                ChaveImportacao = ChaveDespesaTemp.Matricula
            };
        }

        // https://docigp.alerj.rj.gov.br/api/v1/congressmen?query={"filter":{"text":null,"checkboxes":{"withMandate":false,"withoutMandate":false,"withPendency":false,"withoutPendency":false,"unread":false,"joined":true,"notJoined":false,"filler":false},"selects":{"legislatureId":2,"filler":false}},"pagination":{"total":86,"per_page":250,"current_page":1,"last_page":9,"from":1,"to":10,"pages":[1,2,3,4,5]}}
        // https://docigp.alerj.rj.gov.br/api/v1/congressmen/95/legislatures/2/budgets?query={"filter":{"text":null,"checkboxes":{"filler":false},"selects":{"filler":false}},"pagination":{"per_page":250,"current_page":1},"order":{}}
        // https://docigp.alerj.rj.gov.br/api/v1/congressmen/95/legislatures/2/budgets/4321/entries?query={"filter":{"text":null,"checkboxes":{"filler":false},"selects":{"filler":false}},"pagination":{"total":6,"per_page":250,"current_page":1,"last_page":1,"from":1,"to":6,"pages":[1]}}

        // https://docigp.alerj.rj.gov.br/api/v1/cost-centers?query={"filter":{"text":null,"checkboxes":{"filler":false},"selects":{"filler":false}},"pagination":{"per_page":5,"current_page":1},"order":{}}
        // https://docigp.alerj.rj.gov.br/api/v1/entry-types?query={"filter":{"text":null,"checkboxes":{"filler":false},"selects":{"filler":false}},"pagination":{"total":7,"per_page":10000,"current_page":1,"last_page":1,"from":1,"to":7,"pages":[1]}}
        public override void ImportarDespesas(IBrowsingContext context, int ano)
        {
            // TODO: Criar importação por legislatura
            if (ano != 2023)
            {
                logger.LogWarning("Importação já realizada para o ano de {Ano}", ano);
                throw new BusinessException($"Importação já realizada para o ano de {ano}");
            }

            int legislatura = 2; // 2023-2027

            var query = "{\"filter\":{\"text\":null,\"checkboxes\":{\"withMandate\":false,\"withoutMandate\":false,\"withPendency\":false,\"withoutPendency\":false,\"unread\":false,\"joined\":true,\"notJoined\":false,\"filler\":false},\"selects\":{\"filler\":false}},\"pagination\":{\"total\":83,\"per_page\":\"250\",\"current_page\":1,\"last_page\":9,\"from\":1,\"to\":10,\"pages\":[1,2,3,4,5]}}";
            var address = $"{config.BaseAddress}congressmen?query=" + WebUtility.UrlEncode(query);
            Congressman objDeputadosRJ = RestApiGet<Congressman>(address);

            foreach (CongressmanDetails deputado in objDeputadosRJ.Data)
            {
                query = "{\"filter\":{\"text\":null,\"checkboxes\":{\"filler\":false},\"selects\":{\"filler\":false}},\"pagination\":{\"per_page\":250,\"current_page\":1},\"order\":{}}";
                address = $"{config.BaseAddress}congressmen/{deputado.Id}/legislatures/{legislatura}/budgets?query=" + WebUtility.UrlEncode(query);
                Budgets orcamentoMensal = RestApiGet<Budgets>(address);

                if (orcamentoMensal.Links.Pagination.LastPage > 1)
                    throw new NotImplementedException();

                foreach (BudgetDetails orcamento in orcamentoMensal.Data)
                {
                    query = "{\"filter\":{\"text\":null,\"checkboxes\":{\"filler\":false},\"selects\":{\"filler\":false}},\"pagination\":{\"total\":6,\"per_page\":250,\"current_page\":1,\"last_page\":1,\"from\":1,\"to\":6,\"pages\":[1]}}";
                    address = $"{config.BaseAddress}congressmen/{deputado.Id}/legislatures/{legislatura}/budgets/{orcamento.Id}/entries?query=" + WebUtility.UrlEncode(query);
                    Entries despesas = RestApiGet<Entries>(address);

                    if (despesas.Links.Pagination.LastPage > 1)
                        throw new NotImplementedException();

                    foreach (EntryDetails despesa in despesas.Data)
                    {
                        if (despesa.CostCenterCode == "4") continue; // 4 - Devolução de saldo
                        if (Convert.ToDecimal(despesa.Value) > 1)
                            throw new NotImplementedException();

                        var despesaTemp = new CamaraEstadualDespesaTemp()
                        {
                            Nome = deputado.Nickname.ToTitleCase(),
                            Cpf = deputado.Id.ToString(),
                            Ano = (short)orcamento.Year,
                            Mes = Convert.ToInt16(orcamento.Month),
                            TipoDespesa = $"{despesa.CostCenterCode} - {despesa.CostCenterName}",
                            Valor = (decimal)despesa.ValueAbs,
                            DataEmissao = Convert.ToDateTime(despesa.Date, cultureInfo),
                            CnpjCpf = Utils.RemoveCaracteresNaoNumericos(despesa.CpfCnpj),
                            Empresa = despesa.Name,
                            Documento = despesa.Id.ToString(),
                            Observacao = $"Objeto: {despesa.Object};"
                        };

                        if (despesa.DocumentsCount > 1)
                            despesaTemp.Observacao += $" Documentos: {despesa.DocumentsCount};";
                        if (!string.IsNullOrEmpty(despesa.DocumentNumber))
                            despesaTemp.Observacao += $" Num. Doc.: {despesa.DocumentNumber};";

                        InserirDespesaTemp(despesaTemp);
                    }
                }
            }
        }

        public override void DefinirCompetencias(int ano)
        {
            competenciaInicial = $"{ano}01";
            competenciaFinal = $"{ano + 4}12";
        }
    }
}
