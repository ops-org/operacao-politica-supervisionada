using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul.Entities
{
    public class DeputadoRS
    {
        [JsonPropertyName("codigoPro")]
        public int CodigoPro { get; set; }

        [JsonPropertyName("idDeputado")]
        public int IdDeputado { get; set; }

        [JsonPropertyName("nomeDeputado")]
        public string NomeDeputado { get; set; }

        [JsonPropertyName("emailDeputado")]
        public string EmailDeputado { get; set; }

        [JsonPropertyName("telefoneDeputado")]
        public string TelefoneDeputado { get; set; }

        [JsonPropertyName("siglaPartido")]
        public string SiglaPartido { get; set; }

        [JsonPropertyName("nomePartido")]
        public string NomePartido { get; set; }

        [JsonPropertyName("codStatus")]
        public int CodStatus { get; set; }

        [JsonPropertyName("fotoGrandeDeputado")]
        public string FotoGrandeDeputado { get; set; }
    }
}
