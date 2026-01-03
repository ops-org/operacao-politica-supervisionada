using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Estados.Para.Entities
{
    public class NumericFormat
    {
        [JsonPropertyName("FormatType")]
        public string FormatType { get; set; }

        [JsonPropertyName("Precision")]
        public int Precision { get; set; }

        [JsonPropertyName("Unit")]
        public string Unit { get; set; }

        [JsonPropertyName("IncludeGroupSeparator")]
        public bool IncludeGroupSeparator { get; set; }

        [JsonPropertyName("ForcePlusSign")]
        public bool ForcePlusSign { get; set; }

        [JsonPropertyName("SignificantDigits")]
        public int SignificantDigits { get; set; }

        [JsonPropertyName("CurrencyCulture")]
        public string CurrencyCulture { get; set; }

        [JsonPropertyName("CustomFormatString")]
        public object CustomFormatString { get; set; }

        [JsonPropertyName("Currency")]
        public string Currency { get; set; }
    }
}

