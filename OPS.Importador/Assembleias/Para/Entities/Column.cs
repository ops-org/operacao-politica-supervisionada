using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Para.Entities
{
    public class Column
    {
        [JsonPropertyName("Caption")]
        public string Caption { get; set; }

        [JsonPropertyName("DataId")]
        public string DataId { get; set; }

        [JsonPropertyName("DataAttributeId")]
        public object DataAttributeId { get; set; }

        [JsonPropertyName("ColumnType")]
        public string ColumnType { get; set; }

        [JsonPropertyName("DeltaDisplayMode")]
        public string DeltaDisplayMode { get; set; }

        [JsonPropertyName("DisplayMode")]
        public string DisplayMode { get; set; }

        [JsonPropertyName("IgnoreDeltaColor")]
        public bool IgnoreDeltaColor { get; set; }

        [JsonPropertyName("IgnoreDeltaIndication")]
        public bool IgnoreDeltaIndication { get; set; }

        [JsonPropertyName("BarViewModel")]
        public object BarViewModel { get; set; }

        [JsonPropertyName("AllowCellMerge")]
        public bool AllowCellMerge { get; set; }

        [JsonPropertyName("HorzAlignment")]
        public string HorzAlignment { get; set; }

        [JsonPropertyName("ShowStartEndValues")]
        public bool ShowStartEndValues { get; set; }

        [JsonPropertyName("SparklineOptions")]
        public object SparklineOptions { get; set; }

        [JsonPropertyName("GridColumnFilterMode")]
        public string GridColumnFilterMode { get; set; }

        [JsonPropertyName("Weight")]
        public double Weight { get; set; }

        [JsonPropertyName("FixedWidth")]
        public int FixedWidth { get; set; }

        [JsonPropertyName("WidthType")]
        public string WidthType { get; set; }

        [JsonPropertyName("ActualIndex")]
        public int ActualIndex { get; set; }

        [JsonPropertyName("DefaultBestCharacterCount")]
        public int DefaultBestCharacterCount { get; set; }

        [JsonPropertyName("DeltaValueType")]
        public string DeltaValueType { get; set; }

        [JsonPropertyName("UriPattern")]
        public object UriPattern { get; set; }

        [JsonPropertyName("Totals")]
        public List<object> Totals { get; set; }

        [JsonPropertyName("SupportClientFilter")]
        public bool SupportClientFilter { get; set; }
    }
}

