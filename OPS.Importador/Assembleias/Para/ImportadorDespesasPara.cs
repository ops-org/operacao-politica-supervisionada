using System.Globalization;
using System.Text.Json;
using AngleSharp;
using OPS.Core.Enumerators;
using OPS.Importador.Assembleias.Para.Entities;
using OPS.Importador.Comum.Despesa;

namespace OPS.Importador.Assembleias.Para
{
    public class ImportadorDespesasPara : ImportadorDespesasRestApiAnual
    {
        private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("en-US");

        public ImportadorDespesasPara(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            config = new ImportadorCotaParlamentarBaseConfig()
            {
                BaseAddress = "https://alepa.pa.gov.br/Transparencia/",
                Estado = Estados.Para,
                ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
            };
        }

        public override async Task ImportarDespesas(IBrowsingContext context, int ano)
        {
            // Verba Indenizatória
            //https://alepa.pa.gov.br/api/dashboard/data?dashboardId=dashboard7&parameters=[{"name":"CPF_Deputados","value":"7BD68C11-DC21-4571-8EF6-AAB6E15355EF","type":"System.String","allowMultiselect":true,"selectAll":true}]&itemId=gridDashboardItem2&query={"Filter":[{"dimensions":[{"@ItemType":"Dimension","@DataMember":"Ano","@DefaultId":"DataItem0","NumericFormat":{"@FormatType":"General"},"@SortOrder":"Descending"}],"values":[[2024]]}]}
            // Diárias dos Parlamentares
            //https://alepa.pa.gov.br/api/dashboard/data?dashboardId=dashboard8&parameters=[{"name":"CPF_Deputados","value":"7BD68C11-DC21-4571-8EF6-AAB6E15355EF","type":"System.String","allowMultiselect":true,"selectAll":true}]&itemId=gridDashboardItem2&query={"Filter":[{"dimensions":[{"@ItemType":"Dimension","@DataMember":"Ano","@DefaultId":"DataItem0","NumericFormat":{"@FormatType":"General"},"@SortOrder":"Descending"}],"values":[[2023]]}]}


            var addressVerbaIndenizatoria = GetApiUrl(Dashboard.VerbaIndenizatoria, ano);
            DeputadoPara objVerbaIndenizatoriaPara = await RestApiGet<DeputadoPara>(addressVerbaIndenizatoria);
            ImportarDespesas(objVerbaIndenizatoriaPara, ano);

            var addressDiarias = GetApiUrl(Dashboard.DiariasDosParlamentares, ano);
            DeputadoPara objDiariasPara = await RestApiGet<DeputadoPara>(addressDiarias);
            ImportarDespesas(objDiariasPara, ano);
        }

        private void ImportarDespesas(DeputadoPara objDespesasPara, int ano)
        {
            var storageDto = objDespesasPara.ItemData.DataStorageDTO;
            var slices = storageDto.Slices[0];

            foreach (var jsonData in slices.Data)
            {
                var values = JsonSerializer.Deserialize<int[]>(jsonData.Key);

                var nome = storageDto.EncodeMaps.DataItem0[values[1]];
                var tipoDespesa = storageDto.EncodeMaps.DataItem5[values[2]];
                var observacao = storageDto.EncodeMaps.DataItem2[values[3]];
                var dataEmissao = DateOnly.FromDateTime(storageDto.EncodeMaps.DataItem1[values[0]]);

                // TODO: Para Verba Indenizatória o gasto pode ter sido do mês anterior, conforme consta na descrição. Mas desconsideramos pois nem todos os itens trazem descrição completa.
                foreach (var item in jsonData.Value)
                {
                    if (!item.Value.HasValue || item.Value == 0) continue;

                    var despesaTemp = new CamaraEstadualDespesaTemp()
                    {
                        Nome = nome,
                        TipoDespesa = tipoDespesa,
                        Observacao = observacao,
                        DataEmissao = dataEmissao,
                        Ano = (short)dataEmissao.Year,
                        Mes = (short?)dataEmissao.Month,
                        Valor = decimal.Round(item.Value.Value, 2)
                    };

                    InserirDespesaTemp(despesaTemp);
                }
            }
        }

        private string GetApiUrl(Dashboard dashboard, int ano)
        {
            return $"https://alepa.pa.gov.br/api/dashboard/data?dashboardId=dashboard{dashboard.GetHashCode()}&parameters=[{{\"name\":\"CPF_Deputados\",\"value\":\"7BD68C11-DC21-4571-8EF6-AAB6E15355EF\",\"type\":\"System.String\",\"allowMultiselect\":true,\"selectAll\":true}}]&itemId=gridDashboardItem2&query={{\"Filter\":[{{\"dimensions\":[{{\"@ItemType\":\"Dimension\",\"@DataMember\":\"Ano\",\"@DefaultId\":\"DataItem0\",\"NumericFormat\":{{\"@FormatType\":\"General\"}},\"@SortOrder\":\"Descending\"}}],\"values\":[[{ano}]]}}]}}";
        }

        private enum Dashboard
        {
            VerbaIndenizatoria = 7,
            DiariasDosParlamentares = 8
        }
    }
}
