using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.MinasGerais.Entities
{
    public class DeputadoListMG
    {
        [JsonPropertyName("list")]
        public List<DeputadoMG> List { get; set; }
    }
}
