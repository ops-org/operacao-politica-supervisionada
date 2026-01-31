using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Goias.Entities
{
    public class Grupo
    {
        [JsonPropertyName("descricao")]
        public string Descricao { get; set; }

        //[JsonPropertyName("valor_apresentado")]
        //public object ValorApresentado { get; set; }

        //[JsonPropertyName("valor_indenizado")]
        //public object ValorIndenizado { get; set; }

        [JsonPropertyName("subgrupos")]
        public List<Subgrupo> Subgrupos { get; set; }
    }
}
