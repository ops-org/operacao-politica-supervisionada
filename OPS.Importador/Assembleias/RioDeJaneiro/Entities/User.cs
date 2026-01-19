using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.RioDeJaneiro.Entities
{
    public class User
    {
        //[JsonPropertyName("id")]
        //public int Id { get; set; }

        //[JsonPropertyName("name")]
        //public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        //[JsonPropertyName("email_verified_at")]
        //public object EmailVerifiedAt { get; set; }

        //[JsonPropertyName("per_page")]
        //public int PerPage { get; set; }

        //[JsonPropertyName("created_by_id")]
        //public object CreatedById { get; set; }

        //[JsonPropertyName("updated_by_id")]
        //public object UpdatedById { get; set; }

        //[JsonPropertyName("created_at")]
        //public DateTime CreatedAt { get; set; }

        //[JsonPropertyName("updated_at")]
        //public DateTime UpdatedAt { get; set; }

        //[JsonPropertyName("username")]
        //public string Username { get; set; }

        //[JsonPropertyName("department_id")]
        //public int DepartmentId { get; set; }

        //[JsonPropertyName("congressman_id")]
        //public int CongressmanId { get; set; }

        //[JsonPropertyName("last_login_at")]
        //public string LastLoginAt { get; set; }

        //[JsonPropertyName("disabled_at")]
        //public object DisabledAt { get; set; }
    }
}
