using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.MinasGerais.Entities
{
    public class ListaMensalDespesasMG
    {
        [JsonPropertyName("list")]
        public List<List> List { get; set; }
    }
}
