using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul.Entities
{
    public class Bilhete
    {
        [JsonPropertyName("idRequisicaoDiaria")]
        public int IdRequisicaoDiaria { get; set; }

        [JsonPropertyName("localizador")]
        public string Localizador { get; set; }

        [JsonPropertyName("destino")]
        public string Destino { get; set; }

        [JsonPropertyName("dataSaida")]
        public string DataSaida { get; set; }

        [JsonPropertyName("dataRetorno")]
        public string DataRetorno { get; set; }

        [JsonPropertyName("cgcCompanhiaAerea")]
        public string CgcCompanhiaAerea { get; set; }

        [JsonPropertyName("nomeCompanhia")]
        public string NomeCompanhia { get; set; }
    }
}
