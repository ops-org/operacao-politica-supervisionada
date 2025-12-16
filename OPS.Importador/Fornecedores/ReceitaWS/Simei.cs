using System;
using System.Text.Json.Serialization;

namespace OPS.Importador.Fornecedores.ReceitaWS
{
    #region ReceitaWS DTO

    public class Simei
    {
        [JsonPropertyName("optante")]
        public bool? Optante { get; set; }

        [JsonPropertyName("data_opcao")]
        public DateTime? DataOpcao { get; set; }

        [JsonPropertyName("data_exclusao")]
        public DateTime? DataExclusao { get; set; }

        [JsonPropertyName("ultima_atualizacao")]
        public DateTime? UltimaAtualizacao { get; set; }
    }


    #endregion ReceitaWS DTO
}