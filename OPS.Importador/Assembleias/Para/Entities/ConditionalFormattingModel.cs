using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Para.Entities
{
    public class ConditionalFormattingModel
    {
        [JsonPropertyName("FormatConditionStyleSettings")]
        public List<object> FormatConditionStyleSettings { get; set; }

        [JsonPropertyName("RuleModels")]
        public List<object> RuleModels { get; set; }
    }
}

