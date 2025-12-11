using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using OPS.Core.Entity;
using OPS.Core.Enumerator;
using OPS.Core.Utilities;
using OPS.Importador.Assembleias.Comum;
using OPS.Importador.Assembleias.Despesa;
using OPS.Importador.Assembleias.Parlamentar;
using OPS.Importador.Utilities;

namespace OPS.Importador.Assembleias;

public class Para : ImportadorBase
{
    public Para(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        importadorParlamentar = new ImportadorParlamentarPara(serviceProvider);
        importadorDespesas = new ImportadorDespesasPara(serviceProvider);
    }
}

public class ImportadorDespesasPara : ImportadorDespesasRestApiAnual
{
    private CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("en-US");

    public ImportadorDespesasPara(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        config = new ImportadorCotaParlamentarBaseConfig()
        {
            BaseAddress = "https://alepa.pa.gov.br/Transparencia/",
            Estado = Estado.Para,
            ChaveImportacao = ChaveDespesaTemp.NomeParlamentar
        };
    }

    public override void ImportarDespesas(IBrowsingContext context, int ano)
    {
        // Verba Indenizatória
        //https://alepa.pa.gov.br/api/dashboard/data?dashboardId=dashboard7&parameters=[{"name":"CPF_Deputados","value":"7BD68C11-DC21-4571-8EF6-AAB6E15355EF","type":"System.String","allowMultiselect":true,"selectAll":true}]&itemId=gridDashboardItem2&query={"Filter":[{"dimensions":[{"@ItemType":"Dimension","@DataMember":"Ano","@DefaultId":"DataItem0","NumericFormat":{"@FormatType":"General"},"@SortOrder":"Descending"}],"values":[[2024]]}]}
        // Diárias dos Parlamentares
        //https://alepa.pa.gov.br/api/dashboard/data?dashboardId=dashboard8&parameters=[{"name":"CPF_Deputados","value":"7BD68C11-DC21-4571-8EF6-AAB6E15355EF","type":"System.String","allowMultiselect":true,"selectAll":true}]&itemId=gridDashboardItem2&query={"Filter":[{"dimensions":[{"@ItemType":"Dimension","@DataMember":"Ano","@DefaultId":"DataItem0","NumericFormat":{"@FormatType":"General"},"@SortOrder":"Descending"}],"values":[[2023]]}]}


        var addressVerbaIndenizatoria = GetApiUrl(Dashboard.VerbaIndenizatoria, ano);
        DeputadoPara objVerbaIndenizatoriaPara = RestApiGet<DeputadoPara>(addressVerbaIndenizatoria);
        ImportarDespesas(objVerbaIndenizatoriaPara, ano);

        var addressDiarias = GetApiUrl(Dashboard.DiariasDosParlamentares, ano);
        DeputadoPara objDiariasPara = RestApiGet<DeputadoPara>(addressDiarias);
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
            var dataEmissao = Convert.ToDateTime(storageDto.EncodeMaps.DataItem1[values[0]], cultureInfo);

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
                    Valor = item.Value.Value
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

public class ImportadorParlamentarPara : ImportadorParlamentarCrawler
{

    public ImportadorParlamentarPara(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        Configure(new ImportadorParlamentarCrawlerConfig()
        {
            BaseAddress = "https://alepa.pa.gov.br/Institucional/Deputados",
            SeletorListaParlamentares = "main form>script",
            Estado = Estado.Para,
        });
    }

    public override async Task Importar()
    {
        ArgumentNullException.ThrowIfNull(config, nameof(config));

        var context = httpClient.CreateAngleSharpContext();

        var document = await context.OpenAsyncAutoRetry(config.BaseAddress);
        if (document.StatusCode != HttpStatusCode.OK)
        {
            Console.WriteLine($"{config.BaseAddress} {document.StatusCode}");
        };

        var scriptParlamentares = document.QuerySelector(config.SeletorListaParlamentares).ToHtml();
        string pattern = @"'(\d+)'";
        RegexOptions options = RegexOptions.Multiline;

        var maches = Regex.Matches(scriptParlamentares, pattern, options);
        if (maches.Count == 0) throw new Exception("Erro ao buscar a lista de deputados");

        foreach (Match m in maches)
        {
            var matricula = m.Groups[1].Value;
            var deputado = GetDeputadoByMatriculaOrNew(Convert.ToUInt32(matricula));
            if (deputado == null) continue;

            deputado.UrlPerfil = $"https://alepa.pa.gov.br/Institucional/Deputado/{matricula}";
            var subDocument = await context.OpenAsyncAutoRetry(deputado.UrlPerfil);
            if (document.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError("Erro ao consultar parlamentar: {NomeDeputado} {StatusCode}", deputado.UrlPerfil, subDocument.StatusCode);
                continue;
            };
            ColetarDadosPerfil(deputado, subDocument);

            InsertOrUpdate(deputado);
        }

        logger.LogInformation("Parlamentares Inseridos: {Inseridos}; Atualizados {Atualizados};", base.registrosInseridos, base.registrosAtualizados);
    }

    public override void ColetarDadosPerfil(DeputadoEstadual deputado, IDocument subDocument)
    {
        deputado.NomeParlamentar = subDocument.QuerySelector("main>h1").TextContent.Trim().ToTitleCase();
        deputado.UrlFoto = (subDocument.QuerySelector(".card-politico-img-wrapper img") as IHtmlImageElement)?.Source.Split("?")[0];

        var perfil = subDocument.QuerySelectorAll(".card-politico-info-wrapper>span")
            .Where(xx => xx.TextContent.Contains(":"))
            .Select(x => new { Key = x.TextContent.Split(':')[0].Trim(), Value = x.TextContent.Split(':')[1].Trim() });

        deputado.NomeCivil = perfil.First(x => x.Key == "Nome completo").Value;
        deputado.Nascimento = DateOnly.Parse(perfil.First(x => x.Key == "Nascimento").Value.Replace(".", "/"));
        deputado.Profissao = perfil.First(x => x.Key == "Profissão").Value.ToTitleCase().NullIfEmpty();
        deputado.Email = subDocument.QuerySelector(".card-politico-info-wrapper a b").TextContent.Trim().NullIfEmpty();

        deputado.IdPartido = BuscarIdPartido(perfil.First(x => x.Key == "Partido Político").Value);
        deputado.Telefone = subDocument.QuerySelectorAll(".card-politico-info-wrapper p").FirstOrDefault(x => x.TextContent.StartsWith("Tel:"))?.TextContent.Replace("Tel:", "").Trim().NullIfEmpty();

        ImportacaoUtils.MapearRedeSocial(deputado, subDocument.QuerySelectorAll(".card-politico-social-wrapper a"));
    }

    public override DeputadoEstadual ColetarDadosLista(IElement document)
    {
        throw new NotImplementedException();
    }
}

//public class CaptionViewModel
//{
//    [JsonPropertyName("ShowCaption")]
//    public bool ShowCaption { get; set; }

//    [JsonPropertyName("Caption")]
//    public string Caption { get; set; }

//    [JsonPropertyName("Text")]
//    public string Text { get; set; }
//}

//public class Column
//{
//    [JsonPropertyName("Caption")]
//    public string Caption { get; set; }

//    [JsonPropertyName("DataId")]
//    public string DataId { get; set; }

//    [JsonPropertyName("DataAttributeId")]
//    public object DataAttributeId { get; set; }

//    [JsonPropertyName("ColumnType")]
//    public string ColumnType { get; set; }

//    [JsonPropertyName("DeltaDisplayMode")]
//    public string DeltaDisplayMode { get; set; }

//    [JsonPropertyName("DisplayMode")]
//    public string DisplayMode { get; set; }

//    [JsonPropertyName("IgnoreDeltaColor")]
//    public bool IgnoreDeltaColor { get; set; }

//    [JsonPropertyName("IgnoreDeltaIndication")]
//    public bool IgnoreDeltaIndication { get; set; }

//    [JsonPropertyName("BarViewModel")]
//    public object BarViewModel { get; set; }

//    [JsonPropertyName("AllowCellMerge")]
//    public bool AllowCellMerge { get; set; }

//    [JsonPropertyName("HorzAlignment")]
//    public string HorzAlignment { get; set; }

//    [JsonPropertyName("ShowStartEndValues")]
//    public bool ShowStartEndValues { get; set; }

//    [JsonPropertyName("SparklineOptions")]
//    public object SparklineOptions { get; set; }

//    [JsonPropertyName("GridColumnFilterMode")]
//    public string GridColumnFilterMode { get; set; }

//    [JsonPropertyName("Weight")]
//    public double Weight { get; set; }

//    [JsonPropertyName("FixedWidth")]
//    public int FixedWidth { get; set; }

//    [JsonPropertyName("WidthType")]
//    public string WidthType { get; set; }

//    [JsonPropertyName("ActualIndex")]
//    public int ActualIndex { get; set; }

//    [JsonPropertyName("DefaultBestCharacterCount")]
//    public int DefaultBestCharacterCount { get; set; }

//    [JsonPropertyName("DeltaValueType")]
//    public string DeltaValueType { get; set; }

//    [JsonPropertyName("UriPattern")]
//    public object UriPattern { get; set; }

//    [JsonPropertyName("Totals")]
//    public List<object> Totals { get; set; }

//    [JsonPropertyName("SupportClientFilter")]
//    public bool SupportClientFilter { get; set; }
//}

//public class ConditionalFormattingModel
//{
//    [JsonPropertyName("FormatConditionStyleSettings")]
//    public List<object> FormatConditionStyleSettings { get; set; }

//    [JsonPropertyName("RuleModels")]
//    public List<object> RuleModels { get; set; }
//}

public class DataStorageDTO
{
    [JsonPropertyName("EncodeMaps")]
    public EncodeMaps EncodeMaps { get; set; }

    [JsonPropertyName("Slices")]
    public List<Slice> Slices { get; set; }
}

//public class DateTimeFormat
//{
//    [JsonPropertyName("GroupInterval")]
//    public string GroupInterval { get; set; }

//    [JsonPropertyName("ExactDateFormat")]
//    public string ExactDateFormat { get; set; }

//    [JsonPropertyName("YearFormat")]
//    public string YearFormat { get; set; }

//    [JsonPropertyName("QuarterFormat")]
//    public string QuarterFormat { get; set; }

//    [JsonPropertyName("MonthFormat")]
//    public string MonthFormat { get; set; }

//    [JsonPropertyName("DayOfWeekFormat")]
//    public string DayOfWeekFormat { get; set; }

//    [JsonPropertyName("DateFormat")]
//    public string DateFormat { get; set; }

//    [JsonPropertyName("DateHourFormat")]
//    public string DateHourFormat { get; set; }

//    [JsonPropertyName("DateHourMinuteFormat")]
//    public string DateHourMinuteFormat { get; set; }

//    [JsonPropertyName("DateTimeFormat")]
//    public string DtFormat { get; set; }

//    [JsonPropertyName("HourFormat")]
//    public string HourFormat { get; set; }

//    [JsonPropertyName("FirstDayOfWeek")]
//    public int FirstDayOfWeek { get; set; }

//    [JsonPropertyName("CalendarWeekRule")]
//    public string CalendarWeekRule { get; set; }
//}

//public class Default
//{
//    [JsonPropertyName("Level")]
//    public int Level { get; set; }

//    [JsonPropertyName("DateTimeGroupInterval")]
//    public string DateTimeGroupInterval { get; set; }

//    [JsonPropertyName("TextGroupInterval")]
//    public string TextGroupInterval { get; set; }

//    [JsonPropertyName("ID")]
//    public string ID { get; set; }

//    [JsonPropertyName("Name")]
//    public string Name { get; set; }

//    [JsonPropertyName("DataMember")]
//    public string DataMember { get; set; }

//    [JsonPropertyName("Format")]
//    public Format Format { get; set; }

//    [JsonPropertyName("FinalDataType")]
//    public string FinalDataType { get; set; }

//    [JsonPropertyName("DataType")]
//    public string DataType { get; set; }
//}

//public class DimensionDescriptors
//{
//    [JsonPropertyName("Default")]
//    public List<Default> Default { get; set; }
//}

public class EncodeMaps
{
    [JsonPropertyName("DataItem1")]
    public List<DateTime> DataItem1 { get; set; }

    [JsonPropertyName("DataItem0")]
    public List<string> DataItem0 { get; set; }

    [JsonPropertyName("DataItem5")]
    public List<string> DataItem5 { get; set; }

    [JsonPropertyName("DataItem2")]
    public List<string> DataItem2 { get; set; }

    [JsonPropertyName("DataItem3")]
    public List<object> DataItem3 { get; set; }
}

//public class Format
//{
//    [JsonPropertyName("DataType")]
//    public string DataType { get; set; }

//    [JsonPropertyName("NumericFormat")]
//    public object NumericFormat { get; set; }

//    [JsonPropertyName("DateTimeFormat")]
//    public DateTimeFormat DateTimeFormat { get; set; }
//}

public class ItemData
{
    //[JsonPropertyName("MetaData")]
    //public MetaData MetaData { get; set; }

    [JsonPropertyName("DataStorageDTO")]
    public DataStorageDTO DataStorageDTO { get; set; }

    //[JsonPropertyName("SortOrderSlices")]
    //public List<List<string>> SortOrderSlices { get; set; }

    //[JsonPropertyName("Reduced")]
    //public bool Reduced { get; set; }
}

//public class MeasureDescriptor
//{
//    [JsonPropertyName("SummaryType")]
//    public string SummaryType { get; set; }

//    [JsonPropertyName("FilterString")]
//    public object FilterString { get; set; }

//    [JsonPropertyName("Calculation")]
//    public object Calculation { get; set; }

//    [JsonPropertyName("WindowDefinition")]
//    public object WindowDefinition { get; set; }

//    [JsonPropertyName("Expression")]
//    public object Expression { get; set; }

//    [JsonPropertyName("DataItemId")]
//    public object DataItemId { get; set; }

//    [JsonPropertyName("ID")]
//    public string ID { get; set; }

//    [JsonPropertyName("Name")]
//    public string Name { get; set; }

//    [JsonPropertyName("DataMember")]
//    public string DataMember { get; set; }

//    [JsonPropertyName("Format")]
//    public Format Format { get; set; }

//    [JsonPropertyName("FinalDataType")]
//    public string FinalDataType { get; set; }

//    [JsonPropertyName("DataType")]
//    public string DataType { get; set; }
//}

//public class MetaData
//{
//    [JsonPropertyName("DimensionDescriptors")]
//    public DimensionDescriptors DimensionDescriptors { get; set; }

//    [JsonPropertyName("MeasureDescriptors")]
//    public List<MeasureDescriptor> MeasureDescriptors { get; set; }

//    [JsonPropertyName("ColorMeasureDescriptors")]
//    public List<object> ColorMeasureDescriptors { get; set; }

//    [JsonPropertyName("FormatConditionMeasureDescriptors")]
//    public List<object> FormatConditionMeasureDescriptors { get; set; }

//    [JsonPropertyName("NormalizedValueMeasureDescriptors")]
//    public List<object> NormalizedValueMeasureDescriptors { get; set; }

//    [JsonPropertyName("ZeroPositionMeasureDescriptors")]
//    public List<object> ZeroPositionMeasureDescriptors { get; set; }

//    [JsonPropertyName("DeltaDescriptors")]
//    public List<object> DeltaDescriptors { get; set; }

//    [JsonPropertyName("DataSourceColumns")]
//    public List<string> DataSourceColumns { get; set; }

//    [JsonPropertyName("ColumnHierarchy")]
//    public string ColumnHierarchy { get; set; }

//    [JsonPropertyName("RowHierarchy")]
//    public object RowHierarchy { get; set; }
//}

//public class NumericFormat
//{
//    [JsonPropertyName("FormatType")]
//    public string FormatType { get; set; }

//    [JsonPropertyName("Precision")]
//    public int Precision { get; set; }

//    [JsonPropertyName("Unit")]
//    public string Unit { get; set; }

//    [JsonPropertyName("IncludeGroupSeparator")]
//    public bool IncludeGroupSeparator { get; set; }

//    [JsonPropertyName("ForcePlusSign")]
//    public bool ForcePlusSign { get; set; }

//    [JsonPropertyName("SignificantDigits")]
//    public int SignificantDigits { get; set; }

//    [JsonPropertyName("CurrencyCulture")]
//    public string CurrencyCulture { get; set; }

//    [JsonPropertyName("CustomFormatString")]
//    public object CustomFormatString { get; set; }

//    [JsonPropertyName("Currency")]
//    public string Currency { get; set; }
//}

public class DeputadoPara
{
    //[JsonPropertyName("MapAttributesNames")]
    //public object MapAttributesNames { get; set; }

    //[JsonPropertyName("ClustersContent")]
    //public object ClustersContent { get; set; }

    //[JsonPropertyName("FullViewport")]
    //public object FullViewport { get; set; }

    //[JsonPropertyName("Name")]
    //public string Name { get; set; }

    //[JsonPropertyName("Type")]
    //public string Type { get; set; }

    //[JsonPropertyName("CustomItemType")]
    //public object CustomItemType { get; set; }

    //[JsonPropertyName("ParentContainer")]
    //public object ParentContainer { get; set; }

    //[JsonPropertyName("ContentType")]
    //public string ContentType { get; set; }

    //[JsonPropertyName("DataSource")]
    //public object DataSource { get; set; }

    [JsonPropertyName("ItemData")]
    public ItemData ItemData { get; set; }

    //[JsonPropertyName("DataSourceMembers")]
    //public object DataSourceMembers { get; set; }

    //[JsonPropertyName("SelectedValues")]
    //public object SelectedValues { get; set; }

    //[JsonPropertyName("AxisNames")]
    //public List<object> AxisNames { get; set; }

    //[JsonPropertyName("DimensionIds")]
    //public List<object> DimensionIds { get; set; }

    //[JsonPropertyName("ViewModel")]
    //public ViewModel ViewModel { get; set; }

    //[JsonPropertyName("ConditionalFormattingModel")]
    //public ConditionalFormattingModel ConditionalFormattingModel { get; set; }

    //[JsonPropertyName("CaptionViewModel")]
    //public CaptionViewModel CaptionViewModel { get; set; }

    //[JsonPropertyName("Parameters")]
    //public object Parameters { get; set; }

    //[JsonPropertyName("DrillDownValues")]
    //public object DrillDownValues { get; set; }

    //[JsonPropertyName("DrillDownUniqueValues")]
    //public object DrillDownUniqueValues { get; set; }
}

public class Slice
{
    [JsonPropertyName("KeyIds")]
    public List<string> KeyIds { get; set; }

    //[JsonPropertyName("ValueIds")]
    //public ValueIds ValueIds { get; set; }

    [JsonPropertyName("Data")]
    public Dictionary<string, Dictionary<string, decimal?>> Data { get; set; }
}

//public class ValueIds
//{
//    [JsonPropertyName("DataItem3")]
//    public int DataItem3 { get; set; }
//}

//public class ViewModel
//{
//    [JsonPropertyName("Columns")]
//    public List<Column> Columns { get; set; }

//    [JsonPropertyName("AllowCellMerge")]
//    public bool AllowCellMerge { get; set; }

//    [JsonPropertyName("EnableBandedRows")]
//    public bool EnableBandedRows { get; set; }

//    [JsonPropertyName("ShowHorizontalLines")]
//    public bool ShowHorizontalLines { get; set; }

//    [JsonPropertyName("ShowVerticalLines")]
//    public bool ShowVerticalLines { get; set; }

//    [JsonPropertyName("ShowColumnHeaders")]
//    public bool ShowColumnHeaders { get; set; }

//    [JsonPropertyName("ColumnWidthMode")]
//    public string ColumnWidthMode { get; set; }

//    [JsonPropertyName("WordWrap")]
//    public bool WordWrap { get; set; }

//    [JsonPropertyName("SelectionDataMembers")]
//    public List<string> SelectionDataMembers { get; set; }

//    [JsonPropertyName("RowIdentificatorDataMembers")]
//    public List<string> RowIdentificatorDataMembers { get; set; }

//    [JsonPropertyName("ColumnAxisName")]
//    public string ColumnAxisName { get; set; }

//    [JsonPropertyName("SparklineAxisName")]
//    public object SparklineAxisName { get; set; }

//    [JsonPropertyName("HasDimensionColumns")]
//    public bool HasDimensionColumns { get; set; }

//    [JsonPropertyName("ShowFooter")]
//    public bool ShowFooter { get; set; }

//    [JsonPropertyName("TotalsCount")]
//    public int TotalsCount { get; set; }

//    [JsonPropertyName("UpdateTotalsOnColumnFilterChanged")]
//    public bool UpdateTotalsOnColumnFilterChanged { get; set; }

//    [JsonPropertyName("ShowFilterRow")]
//    public bool ShowFilterRow { get; set; }

//    [JsonPropertyName("SupportDataAwareExport")]
//    public bool SupportDataAwareExport { get; set; }

//    [JsonPropertyName("ShowCaption")]
//    public bool ShowCaption { get; set; }

//    [JsonPropertyName("Caption")]
//    public string Caption { get; set; }

//    [JsonPropertyName("ShouldIgnoreUpdate")]
//    public bool ShouldIgnoreUpdate { get; set; }

//    [JsonPropertyName("ContentDescription")]
//    public object ContentDescription { get; set; }
//}
