using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Estados.Para.Entities
{
    public class MetaData
    {
        [JsonPropertyName("DimensionDescriptors")]
        public DimensionDescriptors DimensionDescriptors { get; set; }

        [JsonPropertyName("MeasureDescriptors")]
        public List<MeasureDescriptor> MeasureDescriptors { get; set; }

        [JsonPropertyName("ColorMeasureDescriptors")]
        public List<object> ColorMeasureDescriptors { get; set; }

        [JsonPropertyName("FormatConditionMeasureDescriptors")]
        public List<object> FormatConditionMeasureDescriptors { get; set; }

        [JsonPropertyName("NormalizedValueMeasureDescriptors")]
        public List<object> NormalizedValueMeasureDescriptors { get; set; }

        [JsonPropertyName("ZeroPositionMeasureDescriptors")]
        public List<object> ZeroPositionMeasureDescriptors { get; set; }

        [JsonPropertyName("DeltaDescriptors")]
        public List<object> DeltaDescriptors { get; set; }

        [JsonPropertyName("DataSourceColumns")]
        public List<string> DataSourceColumns { get; set; }

        [JsonPropertyName("ColumnHierarchy")]
        public string ColumnHierarchy { get; set; }

        [JsonPropertyName("RowHierarchy")]
        public object RowHierarchy { get; set; }
    }
}

