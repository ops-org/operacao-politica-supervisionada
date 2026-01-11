using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Goias.Entities
{
    public class Subgrupo
    {
        [JsonPropertyName("descricao")]
        public string Descricao { get; set; }

        //[JsonPropertyName("valor_apresentado")]
        //public decimal ValorApresentado { get; set; }

        //[JsonPropertyName("valor_indenizado")]
        //public decimal ValorIndenizado { get; set; }

        [JsonPropertyName("lancamentos")]
        public List<Lancamento> Lancamentos { get; set; }
    }
}
