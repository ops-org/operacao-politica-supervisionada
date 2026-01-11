using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Estados.Para.Entities
{
    public class DimensionDescriptors
    {
        [JsonPropertyName("Default")]
        public List<Default> Default { get; set; }
    }
}

