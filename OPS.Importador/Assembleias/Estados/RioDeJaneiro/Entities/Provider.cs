using System.Text.Json.Serialization;



namespace OPS.Importador.Assembleias.Estados.RioDeJaneiro.Entities
{
    public class Provider
    {
        //[JsonPropertyName("id")]
        //public int Id { get; set; }

        [JsonPropertyName("cpf_cnpj")]
        public string CpfCnpj { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        //[JsonPropertyName("created_by_id")]
        //public int CreatedById { get; set; }

        //[JsonPropertyName("updated_by_id")]
        //public int? UpdatedById { get; set; }

        //[JsonPropertyName("created_at")]
        //public DateTime CreatedAt { get; set; }

        //[JsonPropertyName("updated_at")]
        //public DateTime UpdatedAt { get; set; }

        //[JsonPropertyName("zipcode")]
        //public object Zipcode { get; set; }

        //[JsonPropertyName("street")]
        //public object Street { get; set; }

        //[JsonPropertyName("number")]
        //public object Number { get; set; }

        //[JsonPropertyName("complement")]
        //public object Complement { get; set; }

        //[JsonPropertyName("neighborhood")]
        //public object Neighborhood { get; set; }

        //[JsonPropertyName("city")]
        //public object City { get; set; }

        //[JsonPropertyName("state")]
        //public object State { get; set; }

        //[JsonPropertyName("fullAddress")]
        //public object FullAddress { get; set; }

        //[JsonPropertyName("is_blocked")]
        //public bool IsBlocked { get; set; }
    }
}

