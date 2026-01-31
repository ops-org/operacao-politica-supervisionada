using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Para.Entities
{
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
}
