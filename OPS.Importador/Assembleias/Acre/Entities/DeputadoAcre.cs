using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Acre.Entities
{
    public class DeputadoAcre
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nome_parlamentar")]
        public string NomeParlamentar { get; set; }

        [JsonPropertyName("fotografia_cropped")]
        public string FotografiaCropped { get; set; }

        [JsonPropertyName("fotografia")]
        public string Fotografia { get; set; }

        [JsonPropertyName("ativo")]
        public bool Ativo { get; set; }

        [JsonPropertyName("partido")]
        public string Partido { get; set; }

        [JsonPropertyName("titular")]
        public string Titular { get; set; }
    }
}
