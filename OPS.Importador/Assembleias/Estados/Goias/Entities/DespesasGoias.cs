using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.Goias.Entities
{
    public class DespesasGoias
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("ano")]
        public int Ano { get; set; }

        [JsonPropertyName("mes")]
        public int Mes { get; set; }

        [JsonPropertyName("deputado")]
        public DeputadoGoias Deputado { get; set; }

        //[JsonPropertyName("valor_apresentado")]
        //public decimal ValorApresentado { get; set; }

        //[JsonPropertyName("valor_indenizado")]
        //public decimal ValorIndenizado { get; set; }

        [JsonPropertyName("grupos")]
        public List<Grupo> Grupos { get; set; }
    }
}
