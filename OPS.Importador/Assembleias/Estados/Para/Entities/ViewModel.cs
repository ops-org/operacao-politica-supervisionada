using System.Collections.Generic;
using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Estados.Para.Entities
{
    public class ViewModel
    {
        [JsonPropertyName("Columns")]
        public List<Column> Columns { get; set; }

        [JsonPropertyName("AllowCellMerge")]
        public bool AllowCellMerge { get; set; }

        [JsonPropertyName("EnableBandedRows")]
        public bool EnableBandedRows { get; set; }

        [JsonPropertyName("ShowHorizontalLines")]
        public bool ShowHorizontalLines { get; set; }

        [JsonPropertyName("ShowVerticalLines")]
        public bool ShowVerticalLines { get; set; }

        [JsonPropertyName("ShowColumnHeaders")]
        public bool ShowColumnHeaders { get; set; }

        [JsonPropertyName("ColumnWidthMode")]
        public string ColumnWidthMode { get; set; }

        [JsonPropertyName("WordWrap")]
        public bool WordWrap { get; set; }

        [JsonPropertyName("SelectionDataMembers")]
        public List<string> SelectionDataMembers { get; set; }

        [JsonPropertyName("RowIdentificatorDataMembers")]
        public List<string> RowIdentificatorDataMembers { get; set; }

        [JsonPropertyName("ColumnAxisName")]
        public string ColumnAxisName { get; set; }

        [JsonPropertyName("SparklineAxisName")]
        public object SparklineAxisName { get; set; }

        [JsonPropertyName("HasDimensionColumns")]
        public bool HasDimensionColumns { get; set; }

        [JsonPropertyName("ShowFooter")]
        public bool ShowFooter { get; set; }

        [JsonPropertyName("TotalsCount")]
        public int TotalsCount { get; set; }

        [JsonPropertyName("UpdateTotalsOnColumnFilterChanged")]
        public bool UpdateTotalsOnColumnFilterChanged { get; set; }

        [JsonPropertyName("ShowFilterRow")]
        public bool ShowFilterRow { get; set; }

        [JsonPropertyName("SupportDataAwareExport")]
        public bool SupportDataAwareExport { get; set; }

        [JsonPropertyName("ShowCaption")]
        public bool ShowCaption { get; set; }

        [JsonPropertyName("Caption")]
        public string Caption { get; set; }

        [JsonPropertyName("ShouldIgnoreUpdate")]
        public bool ShouldIgnoreUpdate { get; set; }

        [JsonPropertyName("ContentDescription")]
        public object ContentDescription { get; set; }
    }
}

