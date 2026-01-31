using System.Text.Json.Serialization;

namespace OPS.Importador.Fornecedores.ReceitaWS
{
    #region ReceitaWS DTO

    public class Simples
    {
        [JsonPropertyName("optante")]
        public bool? Optante { get; set; }

        [JsonPropertyName("data_opcao")]
        public string DataOpcao { get; set; }

        [JsonPropertyName("data_exclusao")]
        public string DataExclusao { get; set; }

        [JsonPropertyName("ultima_atualizacao")]
        public string UltimaAtualizacao { get; set; }
    }


    #endregion ReceitaWS DTO
}