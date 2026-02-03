using System.Text.Json.Serialization;

namespace OPS.Core.DTOs
{
    public class FiltroDenunciaDTO
    {
        [JsonPropertyName("sorting")]
        public string Sorting { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("mensagens_nao_lidas")]
        public bool MensagensNaoLidas { get; set; }

        [JsonPropertyName("aguardando_revisao")]
        public bool AguardandoRevisao { get; set; }

        [JsonPropertyName("pendente_informacao")]
        public bool PendenteInformacao { get; set; }

        [JsonPropertyName("duvidoso")]
        public bool Duvidoso { get; set; }

        [JsonPropertyName("dossie")]
        public bool Dossie { get; set; }

        [JsonPropertyName("repetido")]
        public bool Repetido { get; set; }

        [JsonPropertyName("nao_procede")]
        public bool NaoProcede { get; set; }

        public FiltroDenunciaDTO()
        {
            this.Count = 100;
            this.Page = 1;
        }
    }
}
