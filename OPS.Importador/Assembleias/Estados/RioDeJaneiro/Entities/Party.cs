using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioDeJaneiro.Entities
{
    public class Party
    {
        //[JsonPropertyName("id")]
        //public int Id { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        //[JsonPropertyName("name")]
        //public string Name { get; set; }

        //[JsonPropertyName("created_by_id")]
        //public int CreatedById { get; set; }

        //[JsonPropertyName("updated_by_id")]
        //public int? UpdatedById { get; set; }

        //[JsonPropertyName("created_at")]
        //public DateTime CreatedAt { get; set; }

        //[JsonPropertyName("updated_at")]
        //public DateTime UpdatedAt { get; set; }
    }
}
