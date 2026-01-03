using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OPS.Importador.Assembleias.Estados.RioGrandeDoSul.Entities
{
    public class DiariaDetalhesRS
    {
        [JsonPropertyName("atestados")]
        public List<Atestado> Atestados { get; set; }

        [JsonPropertyName("bilhetes")]
        public List<Bilhete> Bilhetes { get; set; }

        [JsonPropertyName("certificados")]
        public List<Certificado> Certificados { get; set; }

        [JsonPropertyName("notasfiscais")]
        public List<Notasfiscai> Notasfiscais { get; set; }

        [JsonPropertyName("transportes")]
        public List<object> Transportes { get; set; }

        [JsonPropertyName("relatorios")]
        public List<object> Relatorios { get; set; }
    }
}
