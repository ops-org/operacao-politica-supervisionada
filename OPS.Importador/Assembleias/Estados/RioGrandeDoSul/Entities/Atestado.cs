using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul.Entities
{
    public class Atestado
    {
        [JsonPropertyName("idAtestado")]
        public int IdAtestado { get; set; }

        [JsonPropertyName("idRequisicaoDiaria")]
        public int IdRequisicaoDiaria { get; set; }

        [JsonPropertyName("idRequisicaoCota")]
        public int IdRequisicaoCota { get; set; }

        [JsonPropertyName("dataAtestado")]
        public string DataAtestado { get; set; }

        [JsonPropertyName("nomeAutoridade")]
        public string NomeAutoridade { get; set; }

        [JsonPropertyName("idTipoAutoridade")]
        public int IdTipoAutoridade { get; set; }

        [JsonPropertyName("nomeTipo")]
        public string NomeTipo { get; set; }
    }
}
