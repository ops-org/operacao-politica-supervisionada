using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.RioDeJaneiro.Entities
{
    public class Pagination
    {
        [JsonPropertyName("total")]
        public int Total { get; set; }

        //[JsonPropertyName("per_page")]
        //public int PerPage { get; set; }

        //[JsonPropertyName("current_page")]
        //public int CurrentPage { get; set; }

        [JsonPropertyName("last_page")]
        public int LastPage { get; set; }

        //[JsonPropertyName("from")]
        //public int From { get; set; }

        //[JsonPropertyName("to")]
        //public int To { get; set; }

        //[JsonPropertyName("pages")]
        //public List<int> Pages { get; set; }
    }
}
