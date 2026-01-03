using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioDeJaneiro.Entities
{
    public class Budgets
    {
        //[JsonPropertyName("current_page")]
        //public int CurrentPage { get; set; }

        [JsonPropertyName("rows")]
        public List<BudgetDetails> Data { get; set; }

        //[JsonPropertyName("first_page_url")]
        //public string FirstPageUrl { get; set; }

        //[JsonPropertyName("from")]
        //public int From { get; set; }

        //[JsonPropertyName("last_page")]
        //public int LastPage { get; set; }

        //[JsonPropertyName("last_page_url")]
        //public string LastPageUrl { get; set; }

        [JsonPropertyName("links")]
        public Link Links { get; set; }

        //[JsonPropertyName("next_page_url")]
        //public object NextPageUrl { get; set; }

        ////[JsonPropertyName("path")]
        ////public string Path { get; set; }

        ////[JsonPropertyName("per_page")]
        ////public int PerPage { get; set; }

        ////[JsonPropertyName("prev_page_url")]
        ////public object PrevPageUrl { get; set; }

        ////[JsonPropertyName("to")]
        ////public int To { get; set; }

        //[JsonPropertyName("total")]
        //public int Total { get; set; }
    }
}
