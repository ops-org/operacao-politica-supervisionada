using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Estados.Para.Entities
{
    public class CaptionViewModel
    {
        [JsonPropertyName("ShowCaption")]
        public bool ShowCaption { get; set; }

        [JsonPropertyName("Caption")]
        public string Caption { get; set; }

        [JsonPropertyName("Text")]
        public string Text { get; set; }
    }
}
