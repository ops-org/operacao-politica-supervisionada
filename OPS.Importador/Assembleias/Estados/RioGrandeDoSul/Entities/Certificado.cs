using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul.Entities
{
    public class Certificado
    {
        [JsonPropertyName("idRequisicaoDiaria")]
        public int IdRequisicaoDiaria { get; set; }

        [JsonPropertyName("idCertifDiploma")]
        public int IdCertifDiploma { get; set; }

        [JsonPropertyName("evento")]
        public string Evento { get; set; }

        [JsonPropertyName("instituicao")]
        public string Instituicao { get; set; }

        [JsonPropertyName("local")]
        public string Local { get; set; }

        [JsonPropertyName("cargaHoraria")]
        public string CargaHoraria { get; set; }

        [JsonPropertyName("dataDe")]
        public string DataDe { get; set; }

        [JsonPropertyName("dataAte")]
        public string DataAte { get; set; }

        [JsonPropertyName("idRequisicaoCota")]
        public int IdRequisicaoCota { get; set; }

        [JsonPropertyName("instrutor")]
        public string Instrutor { get; set; }
    }
}
