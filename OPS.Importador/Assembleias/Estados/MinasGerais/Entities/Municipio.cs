using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.MinasGerais.Entities
{
    public class Municipio
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; }

        [JsonPropertyName("uf")]
        public string Uf { get; set; }

        [JsonPropertyName("codigoIbge9")]
        public int CodigoIbge9 { get; set; }
    }
}
