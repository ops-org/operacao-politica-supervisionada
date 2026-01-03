using System.Collections.Generic;
using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Estados.Para.Entities
{
    public class ConditionalFormattingModel
    {
        [JsonPropertyName("FormatConditionStyleSettings")]
        public List<object> FormatConditionStyleSettings { get; set; }

        [JsonPropertyName("RuleModels")]
        public List<object> RuleModels { get; set; }
    }
}

