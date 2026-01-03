using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioDeJaneiro.Entities
{
    public class CongressmanDetails
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// https://www.alerj.rj.gov.br/Deputados/PerfilDeputado/{RemoteId}?Legislatura=20
        /// </summary>
        [JsonPropertyName("remote_id")]
        public int? RemoteId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }

        //[JsonPropertyName("party_id")]
        //public int PartyId { get; set; }

        [JsonPropertyName("photo_url")]
        public string PhotoUrl { get; set; }

        //[JsonPropertyName("thumbnail_url")]
        //public string ThumbnailUrl { get; set; }

        //[JsonPropertyName("department_id")]
        //public int DepartmentId { get; set; }

        //[JsonPropertyName("created_by_id")]
        //public int CreatedById { get; set; }

        //[JsonPropertyName("updated_by_id")]
        //public int? UpdatedById { get; set; }

        //[JsonPropertyName("created_at")]
        //public DateTime CreatedAt { get; set; }

        //[JsonPropertyName("updated_at")]
        //public DateTime UpdatedAt { get; set; }

        //[JsonPropertyName("has_mandate")]
        //public bool HasMandate { get; set; }

        //[JsonPropertyName("has_pendency")]
        //public bool HasPendency { get; set; }

        //[JsonPropertyName("is_published")]
        //public bool IsPublished { get; set; }

        //[JsonPropertyName("thumbnail_url_linkable")]
        //public string ThumbnailUrlLinkable { get; set; }

        //[JsonPropertyName("photo_url_linkable")]
        //public string PhotoUrlLinkable { get; set; }

        [JsonPropertyName("party")]
        public Party Party { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; }
    }
}
