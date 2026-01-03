using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.MinasGerais.Entities
{
    public class DeputadoDetalhesMG
    {
        [JsonPropertyName("deputado")]
        public Deputado Deputado { get; set; }
    }
}
