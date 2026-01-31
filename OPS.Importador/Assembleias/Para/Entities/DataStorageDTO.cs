using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Para.Entities
{
    public class DataStorageDTO
    {
        [JsonPropertyName("EncodeMaps")]
        public EncodeMaps EncodeMaps { get; set; }

        [JsonPropertyName("Slices")]
        public List<Slice> Slices { get; set; }
    }
}
